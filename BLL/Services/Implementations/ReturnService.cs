using BLL.DTOs.Return;
using BLL.Exceptions;
using BLL.Helpers;
using BLL.Services.Interfaces;
using Common.Enums;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    /// <summary>
    /// Dịch vụ xử lý yêu cầu đổi trả — toàn bộ luồng nghiệp vụ.
    /// </summary>
    public sealed class ReturnService : IReturnService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly IReturnPolicyRepository _policyRepo;

        public ReturnService(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IReturnPolicyRepository policyRepo)
        {
            _uow = uow;
            _currentUser = currentUser;
            _policyRepo = policyRepo;
        }

        // ============================================================
        // TẠO YÊU CẦU ĐỔI TRẢ
        // ============================================================

        public async Task<string> CreateRequestAsync(CreateReturnRequestDto dto)
        {
            // Phân quyền: NV kho không được tạo yêu cầu
            if (_currentUser.IsWarehouse)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Nhân viên kho không có quyền tạo yêu cầu đổi trả.");

            // Tải hóa đơn kèm chi tiết
            var invoice = await _uow.Invoices.GetByIdWithDetailsAsync(dto.InvoiceId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy hóa đơn Id={dto.InvoiceId}.");

            // Hóa đơn phải ở trạng thái cho phép đổi trả
            if (invoice.Status is InvoiceStatus.Cancelled or InvoiceStatus.FullyReturned)
                throw new BusinessException(BusinessErrorCode.InvoiceNotEligible,
                    $"Hóa đơn {invoice.InvoiceNo} không thể tạo yêu cầu đổi trả " +
                    $"(trạng thái hiện tại: {invoice.Status}).");

            // Kiểm tra thời hạn đổi trả theo chính sách
            var policy = await _policyRepo.GetActivePolicyAsync()
                ?? throw new BusinessException("Chưa có chính sách đổi trả nào được kích hoạt.");

            DateTime deadline = invoice.CreatedAt.Date.AddDays(policy.ReturnWindowDays);
            if (DateTime.Today > deadline && !_currentUser.IsAdmin && !_currentUser.IsManager)
                throw new BusinessException(BusinessErrorCode.ReturnWindowExpired,
                    $"Đã quá thời hạn đổi trả {policy.ReturnWindowDays} ngày " +
                    $"(hạn cuối: {deadline:dd/MM/yyyy}). Cần Quản lý hoặc Admin xử lý.");

            // Validate từng dòng hàng
            if (dto.Items == null || !dto.Items.Any())
                throw new BusinessException("Danh sách sản phẩm cần trả không được để trống.");

            decimal totalRefund = 0m;
            var returnItems = new List<ReturnItem>();

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Qty <= 0)
                    throw new BusinessException($"Số lượng trả phải lớn hơn 0.");

                // Tìm dòng hàng trong hóa đơn
                var invoiceItem = invoice.Items.FirstOrDefault(i => i.Id == itemDto.InvoiceItemId)
                    ?? throw new BusinessException(BusinessErrorCode.NotFound,
                        $"Dòng hàng Id={itemDto.InvoiceItemId} không thuộc hóa đơn này.");

                // Kiểm tra sản phẩm được phép trả
                if (!invoiceItem.IsReturnable)
                    throw new BusinessException(BusinessErrorCode.ItemNotReturnable,
                        $"Sản phẩm '{invoiceItem.ProductName}' không được phép đổi trả.");

                // Tính Available = Qty - ReturnedQty - số đang giữ chỗ
                int reserved = await _uow.Returns.GetReservedQtyForInvoiceItemAsync(invoiceItem.Id);
                int available = invoiceItem.Qty - invoiceItem.ReturnedQty - reserved;

                if (itemDto.Qty > available)
                    throw new BusinessException(BusinessErrorCode.QtyExceedsReturnable,
                        $"Sản phẩm '{invoiceItem.ProductName}': yêu cầu trả {itemDto.Qty}, " +
                        $"nhưng chỉ còn {available} khả dụng.");

                // Tính tiền hoàn cho dòng này
                // RefundAmount = LineTotal / Qty × số lượng trả (làm tròn về phía xuống để an toàn)
                decimal unitRefund = invoiceItem.LineTotal / invoiceItem.Qty;
                decimal refundForLine = Math.Round(unitRefund * itemDto.Qty, 2, MidpointRounding.ToEven);
                totalRefund += refundForLine;

                returnItems.Add(new ReturnItem
                {
                    InvoiceItemId = invoiceItem.Id,
                    ProductId = invoiceItem.ProductId,
                    Qty = itemDto.Qty,
                    Condition = ItemCondition.Good, // mặc định, NV kho cập nhật sau ở bước Receive
                    Restock = true,
                    RefundAmount = refundForLine
                });
            }

            // Sinh RequestNo duy nhất (xử lý trong transaction để tránh trùng khi đồng thời)
            string requestNo = await _uow.ExecuteInTransactionAsync(async () =>
            {
                var request = new ReturnRequest
                {
                    RequestNo = "YC-TEMP", // tạm thời
                    InvoiceId = dto.InvoiceId,
                    Type = dto.Type,
                    Channel = dto.Channel,
                    Status = ReturnStatus.Pending,
                    ReasonCode = dto.ReasonCode,
                    ReasonNote = dto.ReasonNote,
                    TotalRefund = totalRefund,
                    CreatedByUserId = _currentUser.UserId,
                    CreatedAt = DateTime.Now,
                    Items = returnItems
                };

                // Thêm file đính kèm nếu có
                if (dto.AttachmentPaths?.Count > 0)
                {
                    request.Attachments = dto.AttachmentPaths
                        .Select(path => new ReturnAttachment { FilePath = path })
                        .ToList();
                }

                await _uow.Returns.AddAsync(request);
                await _uow.SaveChangesAsync(); // để có Id

                // Cập nhật RequestNo từ Id (đảm bảo duy nhất)
                request.RequestNo = $"YC-{request.Id:D6}";
                _uow.Returns.Update(request);
                await _uow.SaveChangesAsync();

                // Ghi AuditLog
                await _uow.AuditLogs.AddAsync(new AuditLog
                {
                    Action = "CREATE",
                    EntityType = "ReturnRequest",
                    EntityId = request.Id,
                    NewValues = $"{{\"RequestNo\":\"{request.RequestNo}\",\"InvoiceId\":{dto.InvoiceId}," +
                                $"\"Type\":\"{dto.Type}\",\"Channel\":\"{dto.Channel}\"}}",
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.Now
                });
                await _uow.SaveChangesAsync();

                return request.RequestNo;
            });

            return requestNo;
        }

        // ============================================================
        // DUYỆT YÊU CẦU
        // ============================================================

        public async Task ApproveAsync(int requestId)
        {
            var request = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy yêu cầu Id={requestId}.");

            // Chỉ có thể duyệt khi đang Pending
            if (request.Status != ReturnStatus.Pending)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    $"Yêu cầu {request.RequestNo} đang ở trạng thái '{request.Status}', " +
                    "không thể duyệt.");

            // Lấy chính sách để kiểm tra ngưỡng
            var policy = await _policyRepo.GetActivePolicyAsync();
            bool exceedsThreshold = policy != null && request.TotalRefund > policy.ApprovalThresholdAmount;

            // Phân quyền duyệt
            // - Nhân viên bán hàng: chỉ được duyệt kênh Counter + dưới ngưỡng
            // - Quản lý/Admin: không hạn chế
            if (_currentUser.IsSeller)
            {
                if (request.Channel != ReturnChannel.Counter || exceedsThreshold)
                    throw new BusinessException(BusinessErrorCode.ApprovalRequired,
                        exceedsThreshold
                            ? $"Tổng tiền hoàn {request.TotalRefund:N0}đ vượt ngưỡng, cần Quản lý hoặc Admin duyệt."
                            : "Kênh Shipping cần Quản lý hoặc Admin duyệt.");
            }
            else if (_currentUser.IsWarehouse)
            {
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Nhân viên kho không có quyền duyệt yêu cầu đổi trả.");
            }

            // Nếu kênh Counter và dưới ngưỡng → gộp bước Approved + Received (tại quầy nhận ngay)
            bool isCounterUnderThreshold = request.Channel == ReturnChannel.Counter && !exceedsThreshold;
            request.Status = isCounterUnderThreshold ? ReturnStatus.Received : ReturnStatus.Approved;
            request.ApprovedByUserId = _currentUser.UserId;
            request.ApprovedAt = DateTime.Now;

            if (isCounterUnderThreshold)
            {
                // Nhận hàng tại chỗ — thủ kho không cần xác nhận thêm
                request.ReceivedByUserId = _currentUser.UserId;
                request.ReceivedAt = DateTime.Now;
            }

            _uow.Returns.Update(request);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Action = isCounterUnderThreshold ? "APPROVE_AND_RECEIVE" : "APPROVE",
                EntityType = "ReturnRequest",
                EntityId = request.Id,
                NewValues = $"{{\"Status\":\"{request.Status}\"}}",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.Now
            });

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException(BusinessErrorCode.ConcurrencyConflict,
                    "Yêu cầu đã bị thay đổi bởi người khác. Vui lòng tải lại.");
            }
        }

        // ============================================================
        // TỪ CHỐI YÊU CẦU
        // ============================================================

        public async Task RejectAsync(int requestId, string reason)
        {
            // Phân quyền: chỉ Quản lý và Admin
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Chỉ Quản lý hoặc Admin mới được từ chối yêu cầu đổi trả.");

            var request = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy yêu cầu Id={requestId}.");

            if (request.Status is not (ReturnStatus.Pending or ReturnStatus.Approved))
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    $"Không thể từ chối yêu cầu đang ở trạng thái '{request.Status}'.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessException("Lý do từ chối không được để trống.");

            request.Status = ReturnStatus.Rejected;
            request.RejectReason = reason.Trim();
            _uow.Returns.Update(request);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Action = "REJECT",
                EntityType = "ReturnRequest",
                EntityId = request.Id,
                NewValues = $"{{\"Status\":\"Rejected\",\"RejectReason\":\"{reason.Trim()}\"}}",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.Now
            });

            await _uow.SaveChangesAsync();
        }

        // ============================================================
        // HỦY YÊU CẦU
        // ============================================================

        public async Task CancelAsync(int requestId)
        {
            var request = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy yêu cầu Id={requestId}.");

            // Chỉ được hủy khi Pending
            if (request.Status != ReturnStatus.Pending)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    $"Chỉ có thể hủy yêu cầu đang ở trạng thái Chờ duyệt. " +
                    $"Yêu cầu {request.RequestNo} đang ở trạng thái '{request.Status}'.");

            // Người tạo hoặc Quản lý/Admin mới được hủy
            bool isCreator = request.CreatedByUserId == _currentUser.UserId;
            if (!isCreator && !_currentUser.IsAdmin && !_currentUser.IsManager)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Chỉ người tạo yêu cầu hoặc Quản lý/Admin mới được hủy yêu cầu này.");

            request.Status = ReturnStatus.Cancelled;
            _uow.Returns.Update(request);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Action = "CANCEL",
                EntityType = "ReturnRequest",
                EntityId = request.Id,
                NewValues = "{\"Status\":\"Cancelled\"}",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.Now
            });

            await _uow.SaveChangesAsync();
        }

        // ============================================================
        // NHẬN HÀNG (Nhân viên kho, kênh Shipping)
        // ============================================================

        public async Task ReceiveAsync(int requestId, IReadOnlyList<ReceiveReturnItemDto> items)
        {
            // Nhân viên kho hoặc Quản lý/Admin mới được xác nhận nhận hàng
            if (_currentUser.IsSeller)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Nhân viên bán hàng không có quyền xác nhận nhận hàng kênh Shipping.");

            var request = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy yêu cầu Id={requestId}.");

            if (request.Status != ReturnStatus.Approved)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    $"Chỉ có thể nhận hàng khi yêu cầu ở trạng thái Đã duyệt. " +
                    $"Yêu cầu {request.RequestNo} đang ở trạng thái '{request.Status}'.");

            // Chỉ cần bước này cho kênh Shipping (Counter đã gộp ở ApproveAsync)
            if (request.Channel != ReturnChannel.Shipping)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    "Bước xác nhận nhận hàng chỉ áp dụng cho kênh Gửi hàng/Vận chuyển.");

            // Cập nhật tình trạng từng dòng hàng
            foreach (var itemDto in items)
            {
                var returnItem = request.Items.FirstOrDefault(i => i.Id == itemDto.ReturnItemId)
                    ?? throw new BusinessException(BusinessErrorCode.NotFound,
                        $"Dòng trả hàng Id={itemDto.ReturnItemId} không thuộc yêu cầu này.");

                returnItem.Condition = itemDto.Condition;
                // Chỉ được nhập lại kho bán nếu hàng nguyên vẹn (Good)
                returnItem.Restock = itemDto.Condition == ItemCondition.Good && itemDto.Restock;
            }

            request.Status = ReturnStatus.Received;
            request.ReceivedByUserId = _currentUser.UserId;
            request.ReceivedAt = DateTime.Now;
            _uow.Returns.Update(request);

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Action = "RECEIVE",
                EntityType = "ReturnRequest",
                EntityId = request.Id,
                NewValues = "{\"Status\":\"Received\"}",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.Now
            });

            await _uow.SaveChangesAsync();
        }

        // ============================================================
        // HOÀN TẤT YÊU CẦU — CHẠY TRONG 1 TRANSACTION
        // ============================================================

        public async Task CompleteAsync(int requestId)
        {
            // Phân quyền: NV kho không được hoàn tất (chỉ approve người tạo/quản lý)
            if (_currentUser.IsWarehouse)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Nhân viên kho không có quyền hoàn tất yêu cầu đổi trả.");

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                // ---- 1. Tải yêu cầu với đầy đủ chi tiết ----
                var request = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                    ?? throw new BusinessException(BusinessErrorCode.NotFound,
                        $"Không tìm thấy yêu cầu Id={requestId}.");

                if (request.Status != ReturnStatus.Received)
                    throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                        $"Chỉ hoàn tất được yêu cầu đang ở trạng thái Đã nhận hàng. " +
                        $"Yêu cầu {request.RequestNo} đang ở trạng thái '{request.Status}'.");

                // Tải hóa đơn gốc kèm chi tiết
                var invoice = await _uow.Invoices.GetByIdWithDetailsAsync(request.InvoiceId)
                    ?? throw new BusinessException(BusinessErrorCode.NotFound,
                        $"Không tìm thấy hóa đơn Id={request.InvoiceId}.");

                // ---- 2. Xử lý từng dòng trả hàng ----
                foreach (var returnItem in request.Items)
                {
                    // Tìm dòng hàng gốc trong hóa đơn
                    var invoiceItem = invoice.Items.FirstOrDefault(i => i.Id == returnItem.InvoiceItemId)
                        ?? throw new BusinessException(BusinessErrorCode.NotFound,
                            $"Không tìm thấy dòng hàng InvoiceItem Id={returnItem.InvoiceItemId}.");

                    // Kiểm tra lại Available lần cuối trong transaction (tránh race condition)
                    int reserved = await _uow.Returns.GetReservedQtyForInvoiceItemAsync(
                        invoiceItem.Id, excludeRequestId: requestId);
                    int available = invoiceItem.Qty - invoiceItem.ReturnedQty - reserved;

                    if (returnItem.Qty > available)
                        throw new BusinessException(BusinessErrorCode.QtyExceedsReturnable,
                            $"Sản phẩm '{invoiceItem.ProductName}': số lượng trả {returnItem.Qty} " +
                            $"vượt quá khả dụng {available}. Có thể đã xảy ra xung đột đồng thời.");

                    // Tăng ReturnedQty trên InvoiceItem
                    invoiceItem.ReturnedQty += returnItem.Qty;
                    _uow.InvoiceItems.Update(invoiceItem);

                    // ---- 3. Ghi StockMovement ----
                    if (returnItem.Restock && returnItem.Condition == ItemCondition.Good)
                    {
                        // Hàng nguyên vẹn: nhập lại kho bán, cộng StockQuantity
                        await _uow.StockMovements.AddAsync(new StockMovement
                        {
                            ProductId = returnItem.ProductId,
                            MovementType = StockMovementType.ReturnRestock,
                            Quantity = +returnItem.Qty, // dương = nhập kho
                            UnitPrice = invoiceItem.UnitPrice,
                            ReferenceType = "ReturnRequest",
                            ReferenceId = request.Id,
                            Note = $"Nhập lại kho: {invoiceItem.ProductName} × {returnItem.Qty} " +
                                   $"từ yêu cầu {request.RequestNo}",
                            CreatedByUserId = _currentUser.UserId,
                            CreatedAt = DateTime.Now
                        });

                        // Cộng Product.StockQuantity
                        var product = await _uow.Products.GetByIdAsync(returnItem.ProductId)
                            ?? throw new BusinessException(BusinessErrorCode.NotFound,
                                $"Không tìm thấy sản phẩm Id={returnItem.ProductId}.");
                        product.StockQuantity += returnItem.Qty;
                        _uow.Products.Update(product);
                    }
                    else
                    {
                        // Hàng lỗi/hỏng: chỉ ghi sổ theo dõi hàng lỗi, KHÔNG cộng StockQuantity
                        await _uow.StockMovements.AddAsync(new StockMovement
                        {
                            ProductId = returnItem.ProductId,
                            MovementType = StockMovementType.ReturnDefect,
                            Quantity = +returnItem.Qty, // dương trong sổ hàng lỗi
                            UnitPrice = invoiceItem.UnitPrice,
                            ReferenceType = "ReturnRequest",
                            ReferenceId = request.Id,
                            Note = $"Hàng lỗi/hỏng: {invoiceItem.ProductName} × {returnItem.Qty} " +
                                   $"({returnItem.Condition}) từ yêu cầu {request.RequestNo}",
                            CreatedByUserId = _currentUser.UserId,
                            CreatedAt = DateTime.Now
                        });
                        // Product.StockQuantity KHÔNG thay đổi — hàng lỗi chỉ ghi vào sổ riêng
                    }
                }

                // ---- 4. Tạo Refund records (chia theo InvoicePayment) ----
                decimal remaining = request.TotalRefund;
                var payments = invoice.Payments.OrderByDescending(p => p.Amount).ToList();

                if (request.Type == ReturnType.Return && remaining > 0)
                {
                    // Chia tiền hoàn theo tỷ lệ từng phương thức thanh toán
                    decimal invoiceTotal = invoice.TotalAmount > 0 ? invoice.TotalAmount : 1m;

                    for (int i = 0; i < payments.Count && remaining > 0; i++)
                    {
                        var payment = payments[i];
                        bool isLast = i == payments.Count - 1;

                        // Tỷ lệ của phương thức này trong tổng hóa đơn
                        decimal ratio = payment.Amount / invoiceTotal;
                        // Tiền hoàn qua phương thức này
                        decimal refundForPayment = isLast
                            ? remaining // dồn phần dư vào phương thức cuối
                            : Math.Round(request.TotalRefund * ratio, 2, MidpointRounding.ToEven);

                        refundForPayment = Math.Min(refundForPayment, payment.Amount); // không vượt số đã trả
                        refundForPayment = Math.Min(refundForPayment, remaining);

                        if (refundForPayment <= 0) continue;

                        await _uow.Refunds.AddAsync(new Refund
                        {
                            ReturnRequestId = request.Id,
                            InvoicePaymentId = payment.Id,
                            Method = payment.Method,
                            Amount = refundForPayment,
                            RefundedAt = DateTime.Now,
                            ProcessedByUserId = _currentUser.UserId
                        });

                        remaining -= refundForPayment;
                    }
                }

                // ---- 5. Nếu là đổi hàng: tạo hóa đơn Exchange ----
                if (request.Type == ReturnType.Exchange)
                {
                    // Tạo hóa đơn đổi hàng skeleton (GUI / module POS sẽ thêm sản phẩm đổi vào sau)
                    var exchangeInvoice = new Invoice
                    {
                        InvoiceNo = "HD-TEMP",
                        ParentInvoiceId = invoice.Id,
                        CustomerId = invoice.CustomerId,
                        CreatedByUserId = _currentUser.UserId,
                        Status = InvoiceStatus.Paid,
                        Type = InvoiceType.Exchange,
                        SubTotal = 0,
                        DiscountAmount = 0,
                        TotalAmount = 0,
                        CreatedAt = DateTime.Now
                    };

                    await _uow.Invoices.AddAsync(exchangeInvoice);
                    await _uow.SaveChangesAsync(); // lấy Id

                    exchangeInvoice.InvoiceNo = $"HD-{exchangeInvoice.Id:D6}";
                    _uow.Invoices.Update(exchangeInvoice);

                    request.ExchangeInvoiceId = exchangeInvoice.Id;
                }

                // ---- 6. Cập nhật trạng thái hóa đơn gốc ----
                bool allReturned = invoice.Items.All(item => item.ReturnedQty >= item.Qty);
                invoice.Status = allReturned
                    ? InvoiceStatus.FullyReturned
                    : InvoiceStatus.PartiallyReturned;
                _uow.Invoices.Update(invoice);

                // ---- 7. Đặt trạng thái yêu cầu = Completed ----
                request.Status = ReturnStatus.Completed;
                _uow.Returns.Update(request);

                // ---- 8. Ghi AuditLog ----
                await _uow.AuditLogs.AddAsync(new AuditLog
                {
                    Action = "COMPLETE",
                    EntityType = "ReturnRequest",
                    EntityId = request.Id,
                    NewValues = $"{{\"Status\":\"Completed\",\"TotalRefund\":{request.TotalRefund}," +
                                $"\"InvoiceStatus\":\"{invoice.Status}\"}}",
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.Now
                });

                // ---- 9. SaveChanges cuối cùng trong transaction ----
                try
                {
                    await _uow.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Bắt xung đột RowVersion trên InvoiceItem hoặc ReturnRequest
                    throw new BusinessException(BusinessErrorCode.ConcurrencyConflict,
                        "Dữ liệu đã bị thay đổi bởi người khác trong quá trình xử lý. " +
                        "Vui lòng tải lại trang và thử lại.");
                }
            });
        }

        // ============================================================
        // ĐỌC DỮ LIỆU
        // ============================================================

        public async Task<ReturnRequestDto> GetDetailAsync(int requestId)
        {
            var r = await _uow.Returns.GetByIdWithDetailsAsync(requestId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy yêu cầu đổi trả Id={requestId}.");

            return MapToDto(r);
        }

        public async Task<(IReadOnlyList<ReturnRequestListItemDto> Items, int TotalCount)> SearchAsync(
            ReturnStatus? status,
            string? keyword,
            int pageIndex = 0,
            int pageSize = 20)
        {
            // IReturnRepository.SearchRequestsAsync hiện chỉ lọc theo status + phân trang.
            // Lọc keyword thêm ở in-memory sau khi lấy về (tập dữ liệu thường nhỏ).
            var (entities, total) = await _uow.Returns.SearchRequestsAsync(status, pageIndex, pageSize);

            IEnumerable<ReturnRequest> filtered = entities;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                filtered = entities.Where(r =>
                    r.RequestNo.ToLower().Contains(kw) ||
                    (r.Invoice?.InvoiceNo?.ToLower().Contains(kw) ?? false) ||
                    (r.Invoice?.Customer?.Phone?.Contains(kw) ?? false));
                total = filtered.Count();
            }

            var dtos = filtered.Select(MapToListItem).ToList();
            return (dtos, total);
        }

        public async Task<Dictionary<ReturnStatus, int>> GetStatusCountsAsync()
        {
            var result = new Dictionary<ReturnStatus, int>();
            foreach (ReturnStatus s in Enum.GetValues<ReturnStatus>())
            {
                result[s] = await _uow.Returns.CountByStatusAsync(s);
            }
            return result;
        }

        // ============================================================
        // CHÍNH SÁCH ĐỔI TRẢ
        // ============================================================

        public async Task<ReturnPolicyDto> GetPolicyAsync()
        {
            var policy = await _policyRepo.GetActivePolicyAsync()
                ?? throw new BusinessException("Chưa có chính sách đổi trả nào được kích hoạt.");

            return new ReturnPolicyDto
            {
                Id = policy.Id,
                ReturnWindowDays = policy.ReturnWindowDays,
                ApprovalThresholdAmount = policy.ApprovalThresholdAmount,
                IsActive = policy.IsActive,
                UpdatedAt = policy.UpdatedAt,
                UpdatedByUserName = policy.UpdatedByUser?.FullName
            };
        }

        public async Task UpdatePolicyAsync(ReturnPolicyDto dto)
        {
            // Chỉ Admin mới được sửa chính sách đổi trả
            if (!_currentUser.IsAdmin)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Chỉ Admin mới có quyền thay đổi chính sách đổi trả.");

            if (dto.ReturnWindowDays <= 0)
                throw new BusinessException("Thời hạn đổi trả phải lớn hơn 0 ngày.");
            if (dto.ApprovalThresholdAmount < 0)
                throw new BusinessException("Ngưỡng duyệt không được âm.");

            var policy = await _policyRepo.GetActivePolicyAsync()
                ?? throw new BusinessException("Không tìm thấy chính sách đổi trả đang kích hoạt.");

            policy.ReturnWindowDays = dto.ReturnWindowDays;
            policy.ApprovalThresholdAmount = dto.ApprovalThresholdAmount;
            policy.IsActive = dto.IsActive;
            policy.UpdatedAt = DateTime.Now;
            policy.UpdatedByUserId = _currentUser.UserId;

            await _uow.SaveChangesAsync();
        }

        // ============================================================
        // PRIVATE HELPERS — MAPPING
        // ============================================================

        private static ReturnRequestDto MapToDto(ReturnRequest r)
        {
            return new ReturnRequestDto
            {
                Id = r.Id,
                RequestNo = r.RequestNo,
                InvoiceId = r.InvoiceId,
                InvoiceNo = r.Invoice?.InvoiceNo ?? string.Empty,
                CustomerName = r.Invoice?.Customer?.Name,
                CustomerPhone = r.Invoice?.Customer?.Phone,
                Type = r.Type,
                Channel = r.Channel,
                Status = r.Status,
                ReasonCode = r.ReasonCode,
                ReasonNote = r.ReasonNote,
                TotalRefund = r.TotalRefund,
                CreatedByUserId = r.CreatedByUserId,
                CreatedByUserName = r.CreatedByUser?.FullName ?? string.Empty,
                CreatedAt = r.CreatedAt,
                ApprovedByUserName = r.ApprovedByUser?.FullName,
                ApprovedAt = r.ApprovedAt,
                ReceivedByUserName = r.ReceivedByUser?.FullName,
                ReceivedAt = r.ReceivedAt,
                RejectReason = r.RejectReason,
                ExchangeInvoiceId = r.ExchangeInvoiceId,
                ExchangeInvoiceNo = r.ExchangeInvoice?.InvoiceNo,
                Items = r.Items.Select(i => new ReturnItemDto(
                    Id: i.Id,
                    InvoiceItemId: i.InvoiceItemId,
                    ProductId: i.ProductId,
                    ProductName: i.InvoiceItem?.ProductName ?? i.Product?.Name ?? string.Empty,
                    Qty: i.Qty,
                    Condition: i.Condition,
                    Restock: i.Restock,
                    RefundAmount: i.RefundAmount
                )).ToList(),
                AttachmentPaths = r.Attachments.Select(a => a.FilePath).ToList(),
                Refunds = r.Refunds.Select(rf => new RefundDto(
                    Id: rf.Id,
                    Method: rf.Method,
                    MethodDisplayName: PaymentMethodHelper.ToDisplayName(rf.Method),
                    Amount: rf.Amount,
                    RefundedAt: rf.RefundedAt,
                    ProcessedByUserName: rf.ProcessedByUser?.FullName ?? string.Empty,
                    Note: rf.Note
                )).ToList()
            };
        }

        private static ReturnRequestListItemDto MapToListItem(ReturnRequest r)
        {
            return new ReturnRequestListItemDto(
                Id: r.Id,
                RequestNo: r.RequestNo,
                InvoiceNo: r.Invoice?.InvoiceNo ?? string.Empty,
                CustomerName: r.Invoice?.Customer?.Name,
                Type: r.Type,
                Channel: r.Channel,
                Status: r.Status,
                TotalRefund: r.TotalRefund,
                CreatedAt: r.CreatedAt,
                CreatedByUserName: r.CreatedByUser?.FullName ?? string.Empty,
                ApprovedByUserName: r.ApprovedByUser?.FullName
            );
        }
    }
}

using BLL.DTOs.Invoice;
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
    /// Dịch vụ quản lý hóa đơn — tìm kiếm, xem chi tiết, hủy.
    /// </summary>
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly IReturnPolicyRepository _policyRepo;

        public InvoiceService(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IReturnPolicyRepository policyRepo)
        {
            _uow = uow;
            _currentUser = currentUser;
            _policyRepo = policyRepo;
        }

        // ============================================================
        // TÌM KIẾM DANH SÁCH HÓA ĐƠN
        // ============================================================

        public async Task<(IReadOnlyList<InvoiceListItemDto> Items, int TotalCount)> SearchAsync(
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            InvoiceStatus? status,
            int pageIndex = 0,
            int pageSize = 20)
        {
            var (entities, total) = await _uow.Invoices.SearchInvoicesAsync(
                keyword, fromDate, toDate, status, pageIndex, pageSize);

            var dtos = entities.Select(MapToListItem).ToList();
            return (dtos, total);
        }

        // ============================================================
        // XEM CHI TIẾT HÓA ĐƠN
        // ============================================================

        public async Task<InvoiceDetailDto> GetDetailAsync(int invoiceId)
        {
            var invoice = await _uow.Invoices.GetByIdWithDetailsAsync(invoiceId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy hóa đơn Id={invoiceId}.");

            return await MapToDetailAsync(invoice);
        }

        public async Task<InvoiceDetailDto> GetDetailByNoAsync(string invoiceNo)
        {
            var invoice = await _uow.Invoices.GetByInvoiceNoWithDetailsAsync(invoiceNo)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy hóa đơn '{invoiceNo}'.");

            return await MapToDetailAsync(invoice);
        }

        // ============================================================
        // HỦY HÓA ĐƠN
        // ============================================================

        public async Task CancelAsync(int invoiceId, string reason)
        {
            // Phân quyền: chỉ Quản lý và Admin mới được hủy hóa đơn
            if (!_currentUser.IsAdmin && !_currentUser.IsManager)
                throw new BusinessException(BusinessErrorCode.Forbidden,
                    "Chỉ Quản lý hoặc Admin mới được phép hủy hóa đơn.");

            var invoice = await _uow.Invoices.GetByIdWithDetailsAsync(invoiceId)
                ?? throw new BusinessException(BusinessErrorCode.NotFound,
                    $"Không tìm thấy hóa đơn Id={invoiceId}.");

            // Kiểm tra hóa đơn có thể hủy không
            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    "Hóa đơn này đã bị hủy trước đó.");

            if (invoice.Status is InvoiceStatus.PartiallyReturned or InvoiceStatus.FullyReturned)
                throw new BusinessException(BusinessErrorCode.InvalidStatusTransition,
                    "Không thể hủy hóa đơn đã có trả hàng.");

            // Kiểm tra không có yêu cầu đổi trả đang xử lý (Pending/Approved/Received)
            bool hasActiveReturn = invoice.ReturnRequests.Any(r =>
                r.Status is ReturnStatus.Pending or ReturnStatus.Approved or ReturnStatus.Received);

            if (hasActiveReturn)
                throw new BusinessException(BusinessErrorCode.InvoiceNotEligible,
                    "Hóa đơn đang có yêu cầu đổi trả chưa xử lý xong. Vui lòng hoàn tất hoặc hủy yêu cầu đó trước.");

            // Hủy hóa đơn
            invoice.Status = InvoiceStatus.Cancelled;
            invoice.CancelReason = reason.Trim();
            invoice.CancelledAt = DateTime.Now;
            invoice.CancelledByUserId = _currentUser.UserId;
            _uow.Invoices.Update(invoice);

            // Ghi AuditLog
            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                Action = "CANCEL",
                EntityType = "Invoice",
                EntityId = invoice.Id,
                OldValues = $"{{\"Status\":\"{InvoiceStatus.Paid}\"}}",
                NewValues = $"{{\"Status\":\"{InvoiceStatus.Cancelled}\",\"CancelReason\":\"{reason.Trim()}\"}}",
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
                    "Hóa đơn đã bị thay đổi bởi người khác. Vui lòng tải lại và thử lại.");
            }
        }

        // ============================================================
        // TÍCH HỢP POS — Tạo hóa đơn bán lẻ trực tiếp từ màn hình POS
        // ============================================================

        public async Task<string> CreateSaleInvoiceAsync(CreateSaleInvoiceDto dto)
        {
            if (dto.Items.Count == 0)
                throw new BusinessException(BusinessErrorCode.General, "Hóa đơn phải có ít nhất một sản phẩm.");

            int userId = _currentUser.UserId > 0 ? _currentUser.UserId : 1;

            return await _uow.ExecuteInTransactionAsync(async () =>
            {
                var invoice = new Invoice
                {
                    InvoiceNo = "HD-TEMP",
                    CustomerId = dto.CustomerId,
                    CreatedByUserId = userId,
                    Status = InvoiceStatus.Paid,
                    Type = InvoiceType.Sale,
                    SubTotal = dto.SubTotal,
                    DiscountAmount = dto.DiscountAmount,
                    TotalAmount = dto.TotalAmount,
                    CreatedAt = DateTime.Now,
                    Items = dto.Items.Select(x => new InvoiceItem
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        Qty = x.Qty,
                        UnitPrice = x.UnitPrice,
                        DiscountAlloc = x.DiscountAlloc,
                        LineTotal = x.LineTotal,
                        ReturnedQty = 0,
                        IsReturnable = true
                    }).ToList(),
                    Payments = new List<InvoicePayment>
                    {
                        new()
                        {
                            Method = dto.PaymentMethod,
                            Amount = dto.TotalAmount,
                            PaidAt = DateTime.Now
                        }
                    }
                };

                await _uow.Invoices.AddAsync(invoice);
                await _uow.SaveChangesAsync();

                // Cập nhật số hóa đơn chuẩn duy nhất
                invoice.InvoiceNo = $"HD-{invoice.Id:D6}";

                // Trừ số lượng tồn kho và ghi nhận biến động kho StockMovement
                foreach (var item in invoice.Items)
                {
                    var product = await _uow.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity = Math.Max(0, product.StockQuantity - item.Qty);
                        _uow.Products.Update(product);
                    }

                    await _uow.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = StockMovementType.Sale,
                        Quantity = -item.Qty,
                        UnitPrice = item.UnitPrice,
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.Id,
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.Now,
                        Note = $"Xuất bán theo {invoice.InvoiceNo}"
                    });
                }

                await _uow.SaveChangesAsync();
                return invoice.InvoiceNo;
            });
        }

        // ============================================================
        // PRIVATE HELPERS — MAPPING
        // ============================================================

        private static InvoiceListItemDto MapToListItem(Invoice inv)
        {
            var paymentSummary = PaymentMethodHelper.SummarizePayments(
                inv.Payments.Select(p => p.Method));

            return new InvoiceListItemDto(
                Id: inv.Id,
                InvoiceNo: inv.InvoiceNo,
                CreatedAt: inv.CreatedAt,
                CustomerName: inv.Customer?.Name,
                CustomerPhone: inv.Customer?.Phone,
                TotalAmount: inv.TotalAmount,
                Status: inv.Status,
                Type: inv.Type,
                PaymentSummary: paymentSummary
            );
        }

        private async Task<InvoiceDetailDto> MapToDetailAsync(Invoice inv)
        {
            // Lấy chính sách đổi trả để tính số ngày còn lại
            var policy = await _policyRepo.GetActivePolicyAsync();
            int daysRemaining = policy != null
                ? (int)(inv.CreatedAt.Date.AddDays(policy.ReturnWindowDays) - DateTime.Today).TotalDays
                : 0;

            // Tính Available cho từng dòng hàng (bao gồm giữ chỗ)
            var itemDtos = new List<InvoiceItemDetailDto>();
            foreach (var item in inv.Items)
            {
                int reserved = await _uow.Returns.GetReservedQtyForInvoiceItemAsync(item.Id);
                int available = Math.Max(0, item.Qty - item.ReturnedQty - reserved);

                itemDtos.Add(new InvoiceItemDetailDto(
                    Id: item.Id,
                    ProductId: item.ProductId,
                    ProductName: item.ProductName,
                    Qty: item.Qty,
                    UnitPrice: item.UnitPrice,
                    DiscountAlloc: item.DiscountAlloc,
                    LineTotal: item.LineTotal,
                    ReturnedQty: item.ReturnedQty,
                    IsReturnable: item.IsReturnable,
                    Available: available
                ));
            }

            // Tóm tắt trạng thái đổi trả
            int totalReturns = inv.ReturnRequests.Count;
            int activeReturns = inv.ReturnRequests.Count(r =>
                r.Status is ReturnStatus.Pending or ReturnStatus.Approved or ReturnStatus.Received);
            string returnSummary = totalReturns == 0
                ? "Chưa có yêu cầu đổi trả"
                : $"{totalReturns} yêu cầu{(activeReturns > 0 ? $" ({activeReturns} đang xử lý)" : string.Empty)}";

            var paymentDtos = inv.Payments.Select(p => new InvoicePaymentDto(
                Id: p.Id,
                Method: p.Method,
                MethodDisplayName: PaymentMethodHelper.ToDisplayName(p.Method),
                Amount: p.Amount,
                TransactionCode: p.TransactionCode,
                PaidAt: p.PaidAt,
                Note: p.Note
            )).ToList();

            return new InvoiceDetailDto
            {
                Id = inv.Id,
                InvoiceNo = inv.InvoiceNo,
                Status = inv.Status,
                Type = inv.Type,
                CreatedAt = inv.CreatedAt,
                CustomerId = inv.CustomerId,
                CustomerName = inv.Customer?.Name,
                CustomerPhone = inv.Customer?.Phone,
                CreatedByUserId = inv.CreatedByUserId,
                CreatedByUserName = inv.CreatedByUser?.FullName ?? string.Empty,
                SubTotal = inv.SubTotal,
                DiscountAmount = inv.DiscountAmount,
                TotalAmount = inv.TotalAmount,
                CancelReason = inv.CancelReason,
                CancelledAt = inv.CancelledAt,
                OrderId = inv.OrderId,
                ParentInvoiceId = inv.ParentInvoiceId,
                Items = itemDtos,
                Payments = paymentDtos,
                DaysRemainingForReturn = daysRemaining,
                ReturnSummary = returnSummary
            };
        }
    }
}

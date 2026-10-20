using BLL.DTOs.Return;
using BLL.Services.Interfaces;
using Common.Enums;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations
{
    /// <summary>
    /// Dịch vụ báo cáo và phân tích đổi trả hàng.
    /// </summary>
    public sealed class ReturnReportService : IReturnReportService
    {
        private readonly IUnitOfWork _uow;

        public ReturnReportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ReturnReportDto> GetReportAsync(DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            // Lấy danh sách yêu cầu đổi trả trong kỳ
            var requests = (await _uow.Returns.FindAsync(r => r.CreatedAt >= start && r.CreatedAt <= end))
                .ToList();

            int totalRequests = requests.Count;
            int completedRequests = requests.Count(r => r.Status == ReturnStatus.Completed);
            int rejectedRequests = requests.Count(r => r.Status == ReturnStatus.Rejected);

            // Lấy danh sách ReturnItem của các yêu cầu Completed để thống kê chính xác
            var completedIds = requests.Where(r => r.Status == ReturnStatus.Completed).Select(r => r.Id).ToHashSet();
            var allReturnItems = (await _uow.ReturnItems.FindAsync(ri => completedIds.Contains(ri.ReturnRequestId)))
                .ToList();

            decimal totalRefundAmount = allReturnItems.Sum(ri => ri.RefundAmount);

            // Thời gian xử lý trung bình (giờ) cho các yêu cầu đã hoàn tất
            double avgHours = 0;
            var processedRequests = requests
                .Where(r => r.Status == ReturnStatus.Completed && (r.ReceivedAt.HasValue || r.ApprovedAt.HasValue))
                .ToList();

            if (processedRequests.Count > 0)
            {
                avgHours = processedRequests.Average(r =>
                {
                    var finishTime = r.ReceivedAt ?? r.ApprovedAt ?? r.CreatedAt;
                    return (finishTime - r.CreatedAt).TotalHours;
                });
            }

            // Top sản phẩm bị trả nhiều nhất
            var topProducts = allReturnItems
                .GroupBy(ri => new { ri.ProductId })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    TotalQty = g.Sum(x => x.Qty),
                    TotalRefund = g.Sum(x => x.RefundAmount),
                    ReqCount = g.Select(x => x.ReturnRequestId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(10)
                .ToList();

            var topProductDtos = new List<ReturnByProductDto>();
            foreach (var item in topProducts)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId);
                topProductDtos.Add(new ReturnByProductDto(
                    ProductId: item.ProductId,
                    ProductName: product?.Name ?? $"SP #{item.ProductId}",
                    TotalReturnedQty: item.TotalQty,
                    TotalRefundAmount: item.TotalRefund,
                    RequestCount: item.ReqCount
                ));
            }

            // Phân bổ theo lý do đổi trả
            var byReason = requests
                .GroupBy(r => string.IsNullOrWhiteSpace(r.ReasonCode) ? "KHAC" : r.ReasonCode.Trim())
                .Select(g => new ReturnByReasonDto(
                    ReasonCode: g.Key,
                    RequestCount: g.Count(),
                    TotalRefundAmount: g.Where(r => r.Status == ReturnStatus.Completed).Sum(r => r.TotalRefund)
                ))
                .OrderByDescending(x => x.RequestCount)
                .ToList();

            return new ReturnReportDto
            {
                FromDate = start,
                ToDate = end,
                TotalRequests = totalRequests,
                CompletedRequests = completedRequests,
                RejectedRequests = rejectedRequests,
                TotalRefundAmount = totalRefundAmount,
                AvgProcessingHours = Math.Round(avgHours, 1),
                TopReturnedProducts = topProductDtos,
                ByReason = byReason
            };
        }

        public async Task<IReadOnlyList<ReturnByProductDto>> GetReturnRateByProductAsync(DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1).AddTicks(-1);

            var requests = (await _uow.Returns.FindAsync(r => r.CreatedAt >= start && r.CreatedAt <= end && r.Status == ReturnStatus.Completed))
                .Select(r => r.Id)
                .ToHashSet();

            var returnItems = (await _uow.ReturnItems.FindAsync(ri => requests.Contains(ri.ReturnRequestId)))
                .ToList();

            var grouped = returnItems
                .GroupBy(ri => ri.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQty = g.Sum(x => x.Qty),
                    TotalRefund = g.Sum(x => x.RefundAmount),
                    ReqCount = g.Select(x => x.ReturnRequestId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalQty)
                .ToList();

            var result = new List<ReturnByProductDto>();
            foreach (var item in grouped)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId);
                result.Add(new ReturnByProductDto(
                    ProductId: item.ProductId,
                    ProductName: product?.Name ?? $"SP #{item.ProductId}",
                    TotalReturnedQty: item.TotalQty,
                    TotalRefundAmount: item.TotalRefund,
                    RequestCount: item.ReqCount
                ));
            }

            return result;
        }

        public async Task<IReadOnlyList<(int Month, decimal TotalRefund)>> GetMonthlyRefundAsync(int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var refunds = (await _uow.Refunds.FindAsync(rf => rf.RefundedAt >= start && rf.RefundedAt <= end))
                .ToList();

            var result = new List<(int Month, decimal TotalRefund)>();
            for (int month = 1; month <= 12; month++)
            {
                decimal sum = refunds.Where(r => r.RefundedAt.Month == month).Sum(r => r.Amount);
                result.Add((month, sum));
            }

            return result;
        }
    }
}

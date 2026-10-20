namespace BLL.DTOs.Return
{
    /// <summary>
    /// DTO báo cáo đổi trả theo kỳ — dùng cho frmBaoCaoDoiTra.
    /// </summary>
    public sealed class ReturnReportDto
    {
        /// <summary>Kỳ báo cáo (ngày bắt đầu).</summary>
        public DateTime FromDate { get; init; }

        /// <summary>Kỳ báo cáo (ngày kết thúc).</summary>
        public DateTime ToDate { get; init; }

        /// <summary>Tổng số yêu cầu đổi trả trong kỳ.</summary>
        public int TotalRequests { get; init; }

        /// <summary>Số yêu cầu hoàn thành (Completed).</summary>
        public int CompletedRequests { get; init; }

        /// <summary>Số yêu cầu bị từ chối (Rejected).</summary>
        public int RejectedRequests { get; init; }

        /// <summary>Tổng tiền đã hoàn trong kỳ.</summary>
        public decimal TotalRefundAmount { get; init; }

        /// <summary>Thời gian xử lý trung bình (từ Pending đến Completed) tính bằng giờ.</summary>
        public double AvgProcessingHours { get; init; }

        /// <summary>Top sản phẩm bị trả nhiều nhất.</summary>
        public IReadOnlyList<ReturnByProductDto> TopReturnedProducts { get; init; } = [];

        /// <summary>Phân bổ theo lý do đổi trả.</summary>
        public IReadOnlyList<ReturnByReasonDto> ByReason { get; init; } = [];
    }

    /// <summary>Số lượng trả và doanh số hoàn theo từng sản phẩm.</summary>
    public sealed record ReturnByProductDto(
        int ProductId,
        string ProductName,
        int TotalReturnedQty,
        decimal TotalRefundAmount,
        int RequestCount
    );

    /// <summary>Phân bổ yêu cầu theo mã lý do.</summary>
    public sealed record ReturnByReasonDto(
        string ReasonCode,
        int RequestCount,
        decimal TotalRefundAmount
    );
}

using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Bản ghi hoàn tiền chi tiết khớp với từng phương thức thanh toán gốc.
    /// </summary>
    public class Refund
    {
        public int Id { get; set; }

        public int ReturnRequestId { get; set; }
        public ReturnRequest? ReturnRequest { get; set; }

        public int? InvoicePaymentId { get; set; }
        public InvoicePayment? InvoicePayment { get; set; }

        /// <summary>Phương thức hoàn tiền (Cash, Card, Transfer, EWallet)</summary>
        public PaymentMethod Method { get; set; }

        /// <summary>Số tiền hoàn</summary>
        public decimal Amount { get; set; }

        public DateTime RefundedAt { get; set; } = DateTime.Now;

        /// <summary>Nhân viên xử lý lệnh hoàn tiền</summary>
        public int ProcessedByUserId { get; set; }
        public User? ProcessedByUser { get; set; }

        public string? Note { get; set; }
    }
}

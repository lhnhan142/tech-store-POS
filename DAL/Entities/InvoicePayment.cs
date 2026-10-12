using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Bản ghi thanh toán cho hóa đơn (hỗ trợ nhiều phương thức trên 1 hóa đơn).
    /// </summary>
    public class InvoicePayment
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        /// <summary>Phương thức thanh toán: Cash, Card, Transfer, EWallet</summary>
        public PaymentMethod Method { get; set; }

        /// <summary>Số tiền đã thanh toán qua phương thức này</summary>
        public decimal Amount { get; set; }

        /// <summary>Mã giao dịch ngân hàng / POS cà thẻ / Ví điện tử</summary>
        public string? TransactionCode { get; set; }

        /// <summary>Ghi chú thanh toán</summary>
        public string? Note { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.Now;

        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}

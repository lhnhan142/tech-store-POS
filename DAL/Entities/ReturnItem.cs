using Common.Enums;

namespace DAL.Entities
{
    /// <summary>
    /// Dòng chi tiết sản phẩm đổi/trả trong yêu cầu.
    /// </summary>
    public class ReturnItem
    {
        public int Id { get; set; }

        public int ReturnRequestId { get; set; }
        public ReturnRequest? ReturnRequest { get; set; }

        public int InvoiceItemId { get; set; }
        public InvoiceItem? InvoiceItem { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Số lượng xin đổi / trả</summary>
        public int Qty { get; set; }

        /// <summary>Tình trạng hàng: Good (nguyên vẹn), Damaged (hỏng), Defective (lỗi kỹ thuật)</summary>
        public ItemCondition Condition { get; set; } = ItemCondition.Good;

        /// <summary>Có nhập lại kho bán hay không (chỉ true khi Condition = Good)</summary>
        public bool Restock { get; set; } = true;

        /// <summary>Số tiền hoàn trả tương ứng cho dòng này</summary>
        public decimal RefundAmount { get; set; }
    }
}

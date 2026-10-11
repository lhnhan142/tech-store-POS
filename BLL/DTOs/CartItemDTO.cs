namespace BLL.DTOs
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public string? ImagePath { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
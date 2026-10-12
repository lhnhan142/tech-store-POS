namespace BLL.Services
{
    public static class PriceCalculator
    {
        public const decimal VatRate = 0.08m;   // đổi theo thuế suất bạn dùng

        public static (decimal Vat, decimal Total) Calculate(decimal subTotal, decimal discount)
        {
            decimal afterDiscount = Math.Max(0, subTotal - discount);
            decimal vat = Math.Round(afterDiscount * VatRate);
            return (vat, afterDiscount + vat);
        }
    }
}
using Common.Enums;

namespace BLL.Helpers
{
    /// <summary>
    /// Tiện ích chuyển đổi PaymentMethod sang tên hiển thị tiếng Việt.
    /// </summary>
    public static class PaymentMethodHelper
    {
        public static string ToDisplayName(PaymentMethod method) => method switch
        {
            PaymentMethod.Cash => "Tiền mặt",
            PaymentMethod.Card => "Thẻ ngân hàng",
            PaymentMethod.Transfer => "Chuyển khoản",
            PaymentMethod.EWallet => "Ví điện tử",
            _ => method.ToString()
        };

        /// <summary>
        /// Tóm tắt danh sách phương thức thanh toán thành chuỗi VD: "Tiền mặt + Chuyển khoản".
        /// </summary>
        public static string SummarizePayments(IEnumerable<PaymentMethod> methods)
        {
            var distinct = methods.Select(ToDisplayName).Distinct().ToList();
            return distinct.Count == 0 ? "—" : string.Join(" + ", distinct);
        }
    }
}

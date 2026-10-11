using System;
using System.Text.RegularExpressions;

namespace PhuKienMayTinh.Common.Utilities
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone, @"^(0|\+84)[3|5|7|8|9][0-9]{8}$");
        }

        public static bool IsValidIdentityCard(string idCard)
        {
            if (string.IsNullOrWhiteSpace(idCard)) return false;
            // 9 chữ số (CMND) hoặc 12 chữ số (CCCD)
            return Regex.IsMatch(idCard, @"^[0-9]{9}$|^[0-9]{12}$");
        }

        public static string FormatCurrency(decimal amount)
        {
            return string.Format("{0:N0} đ", amount);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GUI.Services
{
    public class SePayConfig
    {
        public string ApiUrl { get; set; } = "https://my.sepay.vn/userapi/transactions/list";
        public string ApiToken { get; set; } = "";
        public string AccountNumber { get; set; } = "108882730938";
        public string Prefix { get; set; } = "SEVQR";
    }

    public class SePayTransaction
    {
        public int Id { get; set; }
        public string? BankBrandName { get; set; }
        public string? AccountNumber { get; set; }
        public string? TransactionDate { get; set; }
        public decimal AmountIn { get; set; }
        public string? TransactionContent { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    public class SePayService
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
        private static readonly HashSet<int> _consumedIds = new();
        private static readonly HashSet<string> _consumedRefs = new();
        private static readonly object _lock = new();

        public SePayConfig Config { get; private set; }

        public SePayService()
        {
            Config = LoadConfig();
        }

        private static SePayConfig LoadConfig()
        {
            var config = new SePayConfig();
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                    configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("SePay", out var section))
                    {
                        if (section.TryGetProperty("ApiUrl", out var pUrl)) config.ApiUrl = pUrl.GetString() ?? config.ApiUrl;
                        if (section.TryGetProperty("ApiToken", out var pToken)) config.ApiToken = pToken.GetString() ?? "";
                        if (section.TryGetProperty("AccountNumber", out var pAcc)) config.AccountNumber = pAcc.GetString() ?? config.AccountNumber;
                        if (section.TryGetProperty("Prefix", out var pPref)) config.Prefix = pPref.GetString() ?? config.Prefix;
                    }
                }
            }
            catch { }
            return config;
        }

        private static string GetStringSafe(JsonElement el, string propName)
        {
            if (!el.TryGetProperty(propName, out var prop)) return "";
            if (prop.ValueKind == JsonValueKind.String) return prop.GetString() ?? "";
            if (prop.ValueKind == JsonValueKind.Number) return prop.GetRawText();
            return prop.ToString();
        }

        private static decimal GetDecimalSafe(JsonElement el, string propName)
        {
            if (!el.TryGetProperty(propName, out var prop)) return 0;
            if (prop.ValueKind == JsonValueKind.Number) return prop.GetDecimal();
            if (prop.ValueKind == JsonValueKind.String)
            {
                string s = prop.GetString() ?? "0";
                decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val);
                return val;
            }
            return 0;
        }

        private static int GetIntSafe(JsonElement el, string propName)
        {
            if (!el.TryGetProperty(propName, out var prop)) return 0;
            if (prop.ValueKind == JsonValueKind.Number) return prop.GetInt32();
            if (prop.ValueKind == JsonValueKind.String)
            {
                string s = prop.GetString() ?? "0";
                int.TryParse(s, out int val);
                return val;
            }
            return 0;
        }

        /// <summary>
        /// Kiểm tra giao dịch nhận tiền thật từ VietinBank qua SePay API
        /// </summary>
        public async Task<(bool IsPaid, SePayTransaction? Transaction, string Message)> CheckPaymentAsync(decimal expectedAmount, string expectedMemo, DateTime startTime)
        {
            if (string.IsNullOrWhiteSpace(Config.ApiToken))
            {
                return (false, null, "Chưa cấu hình API Token SePay");
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, Config.ApiUrl);
                request.Headers.Add("Authorization", $"Bearer {Config.ApiToken.Trim()}");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null, $"Lỗi SePay: {response.StatusCode}");
                }

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("transactions", out var listEl) || listEl.ValueKind != JsonValueKind.Array)
                {
                    return (false, null, "Đang chờ khách thanh toán...");
                }

                string cleanMemo = expectedMemo.Replace(" ", "").ToUpperInvariant();
                string shortMemo = expectedMemo.Contains("POS") 
                    ? expectedMemo.Substring(expectedMemo.IndexOf("POS")).Replace(" ", "").ToUpperInvariant() 
                    : cleanMemo;

                foreach (var item in listEl.EnumerateArray())
                {
                    decimal amtIn = GetDecimalSafe(item, "amount_in");
                    string content = GetStringSafe(item, "transaction_content");
                    string cleanContent = content.Replace(" ", "").ToUpperInvariant();
                    string refNum = GetStringSafe(item, "reference_number");
                    string dateStr = GetStringSafe(item, "transaction_date");
                    string bank = GetStringSafe(item, "bank_brand_name");
                    int id = GetIntSafe(item, "id");

                    // 1. Bỏ qua các giao dịch đã được sử dụng ở đơn hàng trước
                    lock (_lock)
                    {
                        if (id > 0 && _consumedIds.Contains(id)) continue;
                        if (!string.IsNullOrEmpty(refNum) && _consumedRefs.Contains(refNum)) continue;
                    }

                    // 2. Kiểm tra thời gian: Chỉ nhận giao dịch từ lúc mở đơn thanh toán (cho phép độ trễ đồng hồ 30 giây)
                    if (DateTime.TryParse(dateStr, out var transTime))
                    {
                        if (transTime < startTime.AddSeconds(-30))
                            continue;
                    }

                    // 3. Số tiền vào phải >= số tiền cần thanh toán
                    if (amtIn < expectedAmount)
                        continue;

                    // 4. Nội dung bắt buộc phải khớp chính xác mã thanh toán của đơn hàng này
                    bool memoMatched = (!string.IsNullOrEmpty(cleanMemo) && cleanContent.Contains(cleanMemo)) ||
                                       (!string.IsNullOrEmpty(shortMemo) && cleanContent.Contains(shortMemo));

                    if (memoMatched)
                    {
                        // Đánh dấu giao dịch này đã được ghi nhận để không bao giờ tái sử dụng lại
                        lock (_lock)
                        {
                            if (id > 0) _consumedIds.Add(id);
                            if (!string.IsNullOrEmpty(refNum)) _consumedRefs.Add(refNum);
                        }

                        var trans = new SePayTransaction
                        {
                            Id = id,
                            BankBrandName = bank,
                            AmountIn = amtIn,
                            TransactionContent = content,
                            ReferenceNumber = refNum,
                            TransactionDate = dateStr
                        };
                        return (true, trans, $"Đã nhận tiền thành công từ {bank}!");
                    }
                }

                return (false, null, "⏳ Đang đợi khách chuyển khoản...");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi kiểm tra: {ex.Message}");
            }
        }
    }
}

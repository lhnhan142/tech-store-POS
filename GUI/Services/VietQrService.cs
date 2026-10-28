using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GUI.Services
{
    public class VietQrConfig
    {
        public string ApiUrl { get; set; } = "https://api.vietqr.io/v2/generate";
        public string ClientId { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string BankBin { get; set; } = "970422"; // 970422 = MB Bank
        public string BankName { get; set; } = "MB BANK (Quân Đội)";
        public string AccountNumber { get; set; } = "0987654321";
        public string AccountName { get; set; } = "TECH STORE POS";
    }

    /// <summary>
    /// Service gọi RESTful API chính thức của VietQR (https://api.vietqr.io/v2/generate).
    /// </summary>
    public class VietQrService
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
        public VietQrConfig Config { get; private set; }

        public VietQrService()
        {
            Config = LoadConfig();
        }

        private static VietQrConfig LoadConfig()
        {
            var config = new VietQrConfig();
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                    configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("VietQR", out var qrSection))
                    {
                        if (qrSection.TryGetProperty("ApiUrl", out var pUrl)) config.ApiUrl = pUrl.GetString() ?? config.ApiUrl;
                        if (qrSection.TryGetProperty("ClientId", out var pClient)) config.ClientId = pClient.GetString() ?? "";
                        if (qrSection.TryGetProperty("ApiKey", out var pKey)) config.ApiKey = pKey.GetString() ?? "";
                        if (qrSection.TryGetProperty("BankBin", out var pBin)) config.BankBin = pBin.GetString() ?? config.BankBin;
                        if (qrSection.TryGetProperty("BankName", out var pBName)) config.BankName = pBName.GetString() ?? config.BankName;
                        if (qrSection.TryGetProperty("AccountNumber", out var pAcc)) config.AccountNumber = pAcc.GetString() ?? config.AccountNumber;
                        if (qrSection.TryGetProperty("AccountName", out var pName)) config.AccountName = pName.GetString() ?? config.AccountName;
                    }
                }
            }
            catch
            {
                // Sử dụng cấu hình mặc định nếu lỗi đọc file
            }
            return config;
        }

        /// <summary>
        /// Gửi HTTP POST request lên máy chủ VietQR để sinh ảnh mã QR thanh toán ngân hàng thật.
        /// </summary>
        public async Task<(Image? QrImage, string StatusMessage, bool IsSuccess)> GenerateQrAsync(decimal amount, string memo = "THANH TOAN POS")
        {
            // 1. Gọi REST API POST chính thức v2 của VietQR (Hỗ trợ cả Public hoặc kèm ClientId/ApiKey)
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, Config.ApiUrl);
                if (!string.IsNullOrWhiteSpace(Config.ClientId))
                    request.Headers.Add("x-client-id", Config.ClientId.Trim());
                if (!string.IsNullOrWhiteSpace(Config.ApiKey))
                    request.Headers.Add("x-api-key", Config.ApiKey.Trim());

                int.TryParse(Config.BankBin, out int acqId);
                if (acqId == 0) acqId = 970415;

                var payload = new
                {
                    accountNo = Config.AccountNumber,
                    accountName = Config.AccountName,
                    acqId = acqId,
                    amount = (long)amount,
                    addInfo = memo,
                    format = "text",
                    template = "qr_only"
                };

                string jsonBody = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string respText = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(respText);
                    var root = doc.RootElement;
                    string code = root.TryGetProperty("code", out var cProp) ? (cProp.GetString() ?? "") : "";

                    if (code == "00" && root.TryGetProperty("data", out var dataEl))
                    {
                        string dataUrl = dataEl.TryGetProperty("qrDataURL", out var urlProp) ? (urlProp.GetString() ?? "") : "";
                        if (!string.IsNullOrEmpty(dataUrl))
                        {
                            int commaIdx = dataUrl.IndexOf(',');
                            string base64 = (commaIdx >= 0) ? dataUrl[(commaIdx + 1)..] : dataUrl;
                            byte[] bytes = Convert.FromBase64String(base64);
                            using var ms = new MemoryStream(bytes);
                            return (Image.FromStream(ms), "🟢 [POST 200 OK] Đã gọi REST API VietQR thành công", true);
                        }
                    }
                }
            }
            catch
            {
                // Tiếp tục thử fallback bên dưới
            }

            // 2. Gọi qua máy chủ VietQR công khai (Public HTTP GET API)
            try
            {
                long amt = (long)amount;
                string cleanMemo = Uri.EscapeDataString(memo);
                string cleanName = Uri.EscapeDataString(Config.AccountName);
                string url = $"https://img.vietqr.io/image/{Config.BankBin}-{Config.AccountNumber}-compact2.png?amount={amt}&addInfo={cleanMemo}&accountName={cleanName}";

                var bytes = await _httpClient.GetByteArrayAsync(url);
                using var ms = new MemoryStream(bytes);
                return (Image.FromStream(ms), "🟢 [GET 200 OK] Đã tạo mã QR ngân hàng thật từ máy chủ VietQR", true);
            }
            catch (Exception ex)
            {
                // 3. Fallback khi mất mạng hoàn toàn (offline)
                return (GenerateFallbackQr(Config.BankName, (long)amount), $"⚠️ Ngoại tuyến: {ex.Message}", false);
            }
        }

        private static Bitmap GenerateFallbackQr(string bankName, long amount)
        {
            var bmp = new Bitmap(260, 260);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            using var pBorder = new Pen(Color.FromArgb(14, 85, 158), 2f);
            g.DrawRectangle(pBorder, 10, 10, 240, 240);

            DrawCorner(g, 22, 22);
            DrawCorner(g, 180, 22);
            DrawCorner(g, 22, 180);

            using var bDot = new SolidBrush(Color.FromArgb(15, 23, 42));
            var rnd = new Random(42);
            for (int x = 24; x < 240; x += 9)
            {
                for (int y = 24; y < 240; y += 9)
                {
                    if ((x < 85 && y < 85) || (x > 165 && y < 85) || (x < 85 && y > 165)) continue;
                    if (rnd.Next(10) > 4)
                        g.FillRectangle(bDot, x, y, 7, 7);
                }
            }

            using var bCenter = new SolidBrush(Color.FromArgb(2, 132, 199));
            g.FillRectangle(bCenter, 90, 105, 80, 42);
            using var fLogo = new Font("Segoe UI", 11F, FontStyle.Bold);
            using var bWhite = new SolidBrush(Color.White);
            g.DrawString("VIETQR", fLogo, bWhite, new PointF(98, 115));

            return bmp;
        }

        private static void DrawCorner(Graphics g, int x, int y)
        {
            using var b = new SolidBrush(Color.FromArgb(15, 23, 42));
            g.FillRectangle(b, x, y, 48, 48);
            using var bWhite = new SolidBrush(Color.White);
            g.FillRectangle(bWhite, x + 8, y + 8, 32, 32);
            g.FillRectangle(b, x + 16, y + 16, 16, 16);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLL.DTOs;
using BLL.DTOs.Invoice;
using BLL.Services.Interfaces;
using GUI.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.Forms.Sales
{
    public partial class POSControl : UserControl
    {
        private const int MinCardWidth = 260;
        private const int CardGap = 16;
        private const int GridPad = 16; // lề trên và dưới của lưới
        private static readonly CultureInfo Vi = new("vi-VN");
        private static readonly Color Primary = Color.FromArgb(0, 112, 243);
        private static readonly Color TextDark = Color.FromArgb(15, 15, 60);

        // ĐỔI 1: thêm ImageFile (tên file ảnh trong Images\Products)
        private record ProductItem(int Id, string Name, decimal Price, int Stock, int CategoryId, string Barcode, string? ImageFile);

        private readonly List<ProductItem> _allProducts = new();
        private readonly List<CartItemDTO> _cart = new();
        private readonly List<(Button Btn, int CategoryId)> _chips = new();
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };
        private Panel pnlGrid = null!;       // thay cho flpProducts, xếp thẻ bằng tọa độ
        private int _selectedCategoryId = 0; // 0 = Tất cả
        private decimal _discount = 0;       // Tiền giảm giá thực tế (VNĐ)
        private decimal _discountPercent = 0; // % giảm giá nếu áp dụng theo %
        private bool _isDiscountPercent = false; // Đang giảm theo % hay VNĐ
        private string _discountReason = ""; // Ghi chú / lý do giảm giá
        private decimal _tax = 0;            // sau này tính theo cấu hình VAT

        public POSControl()
        {
            InitializeComponent();

            SetupGridPanel();
            LoadProducts();
            SetupCategoryChips();
            SetupSearch();
            SetupDiscountInteractions();

            flpCart.Resize += (s, e) => ResizeCartItems();
            btnPay.Click += BtnPay_Click;

            ApplyFilter();
            RefreshCart();

            // Timer WinForms không nằm trong components nên phải tự dispose
            Disposed += (s, e) =>
            {
                _searchTimer.Stop();
                _searchTimer.Dispose();
            };
        }

        private static string Money(decimal v) => v.ToString("N0", Vi) + "đ";

        // ĐỔI 2: ghép tên file thành đường dẫn đầy đủ, null nếu không có
        private static string? ImagePathOf(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return Path.Combine(Application.StartupPath, "Images", "Products", fileName);
        }

        // Bỏ dấu tiếng Việt để tìm "chuot" ra "Chuột"
        private static string Normalize(string s)
        {
            var d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in d)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        }

        // ---------- Panel lưới sản phẩm (thay flpProducts) ----------
        private void SetupGridPanel()
        {
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = flpProducts.BackColor
            };

            var parent = flpProducts.Parent!;
            parent.Controls.Add(pnlGrid);
            parent.Controls.SetChildIndex(pnlGrid, 0); // đứng đầu để Dock Fill chừa chỗ cho thanh tìm kiếm và chip

            flpProducts.Visible = false;
            flpProducts.Dock = DockStyle.None;

            pnlGrid.Resize += (s, e) => LayoutCards();
        }

        // ---------- Dữ liệu sản phẩm ----------
        // Dữ liệu giả theo SQL. Sau này thay bằng IProductService.
        private void LoadProducts()
        {
            LoadFakeProducts();

            _ = Task.Run(async () =>
            {
                try
                {
                    if (Program.ServiceProvider != null)
                    {
                        using var scope = Program.ServiceProvider.CreateScope();
                        var productService = scope.ServiceProvider.GetService<IProductService>();
                        if (productService != null)
                        {
                            var dbProducts = await productService.GetAllAsync();
                            if (dbProducts != null && dbProducts.Count > 0)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    _allProducts.Clear();
                                    foreach (var p in dbProducts)
                                    {
                                        _allProducts.Add(new ProductItem(
                                            p.Id,
                                            p.Name,
                                            p.Price,
                                            p.StockQuantity,
                                            p.CategoryId,
                                            p.Barcode ?? "",
                                            p.ImageUrl
                                        ));
                                    }
                                    ApplyFilter();
                                }));
                            }
                        }
                    }
                }
                catch { }
            });
        }

        private void LoadFakeProducts()
        {
            // ĐỔI 3: thêm tên file ảnh ở cuối mỗi dòng
            _allProducts.AddRange(new[]
            {
                new ProductItem(1,  "Chuột Logitech G102", 5000, 50, 1, "880123456789", "g102.jpg"),
                new ProductItem(2,  "Chuột Razer DeathAdder Essential", 550000, 30, 1, "880123456790", "deathadder.jpg"),
                new ProductItem(3,  "Bàn phím cơ Akko 3087", 1200000, 20, 2, "880123456791", "akko3087.jpg"),
                new ProductItem(4,  "Bàn phím DareU EK810", 600000, 45, 2, "880123456792", "dareu-ek810.jpg"),
                new ProductItem(5,  "Tai nghe HyperX Cloud II", 1800000, 15, 3, "880123456793", "hyperx-cloud2.jpg"),
                new ProductItem(6,  "Tai nghe Sony WH-1000XM4", 6500000, 10, 3, "880123456794", "sony-xm4.jpg"),
                new ProductItem(7,  "Màn hình LG 24MP60G", 3200000, 25, 4, "880123456795", "lg-24mp60g.jpg"),
                new ProductItem(8,  "Lót chuột Corsair MM300", 350000, 100, 5, "880123456796", "corsair-mm300.jpg"),
                new ProductItem(9,  "Chuột không dây Logitech MX Master 3S", 2500000, 15, 1, "880123456801", "mx-master3s.jpg"),
                new ProductItem(10, "RAM Corsair Vengeance RGB Pro 16GB (2x8GB)", 1450000, 50, 6, "880123456809", "corsair-vengeance.jpg"),
                new ProductItem(11, "Ổ cứng SSD Samsung 980 PRO 1TB PCIe 4.0", 2300000, 40, 6, "880123456810", "samsung-980pro.jpg"),
                new ProductItem(12, "Balo Laptop Targus 15.6 inch", 750000, 60, 8, "880123456815", "targus-156.jpg"),
                new ProductItem(13, "Ghế Gaming DXRacer Master Series", 8500000, 5, 10, "880123456818", "dxracer.jpg"),
            });
        }

        // ---------- Chip danh mục ----------
        private void SetupCategoryChips()
        {
            _chips.AddRange(new (Button, int)[]
            {
                (btnCatAll, 0),
                (btnCatMouse, 1),
                (btnCatKeyboard, 2),
                (btnCatHeadset, 3),
                (btnCatMonitor, 4),
                (btnCatPad, 5),
                (btnCatParts, 6),
                (btnCatBag, 8),
                (btnCatChair, 10),
            });

            foreach (var (btn, categoryId) in _chips)
                btn.Click += (s, e) => SelectCategory(categoryId);

            HighlightChips();
        }

        private void SelectCategory(int categoryId)
        {
            _selectedCategoryId = categoryId;
            HighlightChips();
            ApplyFilter();
        }

        private void HighlightChips()
        {
            foreach (var (btn, categoryId) in _chips)
            {
                bool on = categoryId == _selectedCategoryId;
                btn.BackColor = on ? Primary : Color.White;
                btn.ForeColor = on ? Color.White : TextDark;
            }
        }

        // ---------- Tìm kiếm ----------
        private void SetupSearch()
        {
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                if (IsDisposed) return;
                ApplyFilter();
            };
            txtSearch.TextChanged += (s, e) => { _searchTimer.Stop(); _searchTimer.Start(); };
            txtSearch.KeyDown += TxtSearch_KeyDown;
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true; // tránh tiếng "ding"

            string text = txtSearch.Text.Trim();
            if (text.Length == 0) return;

            // Máy quét mã vạch gõ mã rồi gửi Enter: khớp mã thì thêm thẳng vào giỏ
            var hit = _allProducts.FirstOrDefault(p => p.Barcode == text);
            if (hit != null)
            {
                AddToCart(hit.Id, hit.Name, hit.Price, hit.Stock, ImagePathOf(hit.ImageFile));
                txtSearch.Clear();
            }
            else
            {
                _searchTimer.Stop();
                ApplyFilter();
            }
        }

        // ---------- Lọc và vẽ lưới sản phẩm ----------
        private void ApplyFilter()
        {
            string keyword = Normalize(txtSearch.Text.Trim());

            var result = _allProducts.Where(p =>
                (_selectedCategoryId == 0 || p.CategoryId == _selectedCategoryId) &&
                (keyword.Length == 0 ||
                 Normalize(p.Name).Contains(keyword) ||
                 p.Barcode.Contains(keyword))).ToList();

            RenderProducts(result);
        }

        private void RenderProducts(List<ProductItem> products)
        {
            pnlGrid.SuspendLayout();

            // Về đầu danh sách trước khi xóa thẻ cũ
            pnlGrid.AutoScrollPosition = new Point(0, 0);

            foreach (Control c in pnlGrid.Controls.Cast<Control>().ToList())
            {
                pnlGrid.Controls.Remove(c);
                c.Dispose();
            }

            if (products.Count == 0)
            {
                pnlGrid.Controls.Add(new Label
                {
                    Text = "Không tìm thấy sản phẩm.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.FromArgb(160, 164, 174),
                    Location = new Point(8, GridPad)
                });
            }

            foreach (var p in products)
            {
                var card = new ucProductCard();
                string? img = ImagePathOf(p.ImageFile);   // ĐỔI 4: truyền đường dẫn ảnh thay vì null
                card.SetData(p.Id, p.Name, p.Price, p.Stock, img);
                card.ProductClicked += (s, id) => AddToCart(p.Id, p.Name, p.Price, p.Stock, img);
                pnlGrid.Controls.Add(card);
            }

            pnlGrid.ResumeLayout(false);
            LayoutCards();
        }

        // Tự xếp thẻ theo lưới và đặt đúng chiều cao vùng cuộn
        private void LayoutCards()
        {
            if (pnlGrid == null) return;

            // Trừ sẵn chỗ thanh cuộn dọc để bố cục không nhảy khi nó hiện/ẩn
            int available = pnlGrid.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            if (available <= 0) return;

            var cards = pnlGrid.Controls.OfType<ucProductCard>().ToList();

            int cols = 3;
            int cardWidth = (available - (cols - 1) * CardGap) / cols;
            int cardHeight = (int)(cardWidth * 1.26);

            pnlGrid.SuspendLayout();

            // Đặt về đầu rồi mới xếp, để tọa độ không bị lệch theo vị trí cuộn
            pnlGrid.AutoScrollPosition = new Point(0, 0);

            for (int i = 0; i < cards.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                cards[i].Size = new Size(cardWidth, cardHeight);
                cards[i].Location = new Point(col * (cardWidth + CardGap), GridPad + row * (cardHeight + CardGap));
            }

            int rows = cards.Count == 0 ? 0 : (cards.Count + cols - 1) / cols;
            int contentHeight = GridPad + rows * (cardHeight + CardGap) + GridPad;
            pnlGrid.AutoScrollMinSize = new Size(0, contentHeight);

            pnlGrid.ResumeLayout(true);
        }

        // ---------- Giỏ hàng ----------
        private void AddToCart(int id, string name, decimal price, int stock, string? imagePath = null)
        {
            var item = _cart.FirstOrDefault(x => x.ProductId == id);
            int newQty = (item?.Quantity ?? 0) + 1;

            if (newQty > stock)
            {
                MessageBox.Show($"\"{name}\" chỉ còn {stock} sản phẩm trong kho.",
                    "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (item == null)
                _cart.Add(new CartItemDTO { ProductId = id, Name = name, UnitPrice = price, Quantity = 1, Stock = stock, ImagePath = imagePath });
            else
                item.Quantity = newQty;

            RefreshCart();
        }

        private void ChangeQuantity(int productId, int delta)
        {
            var item = _cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            int newQty = item.Quantity + delta;
            if (newQty <= 0) { _cart.Remove(item); }
            else if (newQty > item.Stock)
            {
                MessageBox.Show($"Kho chỉ còn {item.Stock} sản phẩm.", "Hết hàng",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else item.Quantity = newQty;

            RefreshCart();
        }

        private void RemoveFromCart(int productId)
        {
            _cart.RemoveAll(x => x.ProductId == productId);
            RefreshCart();
        }
        private decimal CalcTotal()
        {
            decimal subtotal = _cart.Sum(x => x.LineTotal);
            return Math.Max(0, subtotal - _discount + _tax);
        }
        // Vẽ lại danh sách giỏ hàng và tính lại tổng tiền
        private void RefreshCart()
        {
            flpCart.SuspendLayout();

            foreach (Control c in flpCart.Controls.Cast<Control>().ToList())
            {
                flpCart.Controls.Remove(c);
                c.Dispose();
            }

            foreach (var item in _cart)
            {
                var ctl = new ucCartItem { Margin = new Padding(0, 0, 0, 8) };
                ctl.SetData(item.ProductId, item.Name, item.UnitPrice, item.Quantity, item.ImagePath);
                ctl.PlusClicked += (s, id) => ChangeQuantity(id, +1);
                ctl.MinusClicked += (s, id) => ChangeQuantity(id, -1);
                ctl.RemoveClicked += (s, id) => RemoveFromCart(id);
                flpCart.Controls.Add(ctl);
            }
            ResizeCartItems();
            flpCart.ResumeLayout();

            decimal subtotal = _cart.Sum(x => x.LineTotal);
            // Tự động tính lại tiền giảm theo subtotal mới nếu giảm theo %
            if (_isDiscountPercent && _discountPercent > 0)
            {
                _discount = Math.Round(subtotal * (_discountPercent / 100m), 0);
            }
            else
            {
                _discount = Math.Min(_discount, subtotal);
            }

            decimal total = Math.Max(0, subtotal - _discount + _tax);
            lblSubtotal.Text = Money(subtotal);
            if (_discount > 0)
            {
                lblDiscount.Text = _isDiscountPercent
                    ? $"-{Money(_discount)} ({_discountPercent:0.#}%) ✎"
                    : $"-{Money(_discount)} ✎";
                lblDiscount.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                lblDiscount.Text = "0đ ✎";
                lblDiscount.ForeColor = Color.FromArgb(0, 112, 243);
            }

            lblTax.Text = Money(_tax);
            lblTotal.Text = Money(total);
        }


        // ---------- Xử lý giảm giá ----------
        private void SetupDiscountInteractions()
        {
            lblDiscountText.Cursor = Cursors.Hand;
            lblDiscount.Cursor = Cursors.Hand;

            var tip = new ToolTip();
            tip.SetToolTip(lblDiscountText, "Nhấp vào để nhập hoặc thay đổi mức giảm giá (% hoặc VNĐ)");
            tip.SetToolTip(lblDiscount, "Nhấp vào để nhập hoặc thay đổi mức giảm giá (% hoặc VNĐ)");

            lblDiscountText.Click += (s, e) => OpenDiscountDialog();
            lblDiscount.Click += (s, e) => OpenDiscountDialog();
        }

        private void OpenDiscountDialog()
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm sản phẩm vào giỏ hàng trước khi áp dụng giảm giá.",
                    "Giảm giá", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal subtotal = _cart.Sum(x => x.LineTotal);
            using var dlg = new DiscountDialog(subtotal, _discount, _discountPercent, _isDiscountPercent, _discountReason);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _isDiscountPercent = dlg.IsPercent;
                _discountPercent = dlg.DiscountPercent;
                _discount = dlg.DiscountAmount;
                _discountReason = dlg.Reason;
                RefreshCart();
            }
        }

        private void ResizeCartItems()
        {
            int w = flpCart.ClientSize.Width - flpCart.Padding.Horizontal;
            if (w <= 0) return;
            foreach (Control c in flpCart.Controls)
                c.Width = w;
        }

        private async void BtnPay_Click(object? sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống.", "Thanh toán",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal total = CalcTotal();

            using var frm = new PaymentForm(total, _cart, _discount, _tax);
            if (frm.ShowDialog(this) != DialogResult.OK) return;

            string invoiceNo = "";
            try
            {
                if (Program.ServiceProvider != null)
                {
                    using var scope = Program.ServiceProvider.CreateScope();
                    var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

                    decimal subtotal = _cart.Sum(x => x.LineTotal);

                    // Phân bổ giảm giá xuống từng dòng theo tỷ lệ LineTotal / SubTotal
                    var invoiceItems = new List<CreateSaleInvoiceItemDto>();
                    decimal remainingDiscount = _discount;

                    for (int itemIdx = 0; itemIdx < _cart.Count; itemIdx++)
                    {
                        var item = _cart[itemIdx];
                        decimal itemSubtotal = item.LineTotal;
                        decimal alloc = 0;

                        if (subtotal > 0 && _discount > 0)
                        {
                            if (itemIdx == _cart.Count - 1)
                            {
                                alloc = Math.Max(0, remainingDiscount);
                            }
                            else
                            {
                                alloc = Math.Round(_discount * (itemSubtotal / subtotal), 0);
                                alloc = Math.Min(alloc, remainingDiscount);
                                remainingDiscount -= alloc;
                            }
                        }

                        decimal lineTotal = Math.Max(0, itemSubtotal - alloc);

                        invoiceItems.Add(new CreateSaleInvoiceItemDto(
                            ProductId: item.ProductId,
                            ProductName: item.Name,
                            Qty: item.Quantity,
                            UnitPrice: item.UnitPrice,
                            DiscountAlloc: alloc,
                            LineTotal: lineTotal
                        ));
                    }

                    var dto = new CreateSaleInvoiceDto(
                        CustomerId: 1, // Khách hàng mặc định trong CSDL (Nguyễn Văn A)
                        SubTotal: subtotal,
                        DiscountAmount: _discount,
                        TotalAmount: total,
                        PaymentMethod: frm.SelectedMethod,
                        Items: invoiceItems
                    );

invoiceNo = await invoiceService.CreateSaleInvoiceAsync(dto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu hóa đơn: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(
                $"Thanh toán thành công!\n" +
                $"Mã hóa đơn: {invoiceNo}\n" +
                $"Tổng tiền: {Money(total)}\n" +
                $"Tiền thừa: {Money(frm.Change)}\n\n" +
                "Hóa đơn đã được lưu vào hệ thống để thử nghiệm đổi trả.",
                "Thanh toán hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _cart.Clear();
            _discount = 0;
            _discountPercent = 0;
            _isDiscountPercent = false;
            _discountReason = "";
            RefreshCart();
        }

        // Các hàm dưới do Designer tạo, giữ lại để khỏi lỗi build.
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void btnSearch_Click(object sender, EventArgs e) { ApplyFilter(); }
        private void pnlSummary_Paint(object sender, PaintEventArgs e) { }
        private void pnlTotal_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}

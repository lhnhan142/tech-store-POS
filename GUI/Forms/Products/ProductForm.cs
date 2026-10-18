using BLL.DTOs;
using BLL.Exceptions;
using BLL.Services;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace GUI.Forms.Products
{
    /// <summary>
    /// Quản lý sản phẩm: xem, tìm kiếm, lọc theo danh mục / thương hiệu, thêm, sửa, xóa,
    /// chọn ảnh, nhập từ Excel, xuất ra Excel.
    /// </summary>
    [DesignerCategory("Code")]
    public class ProductForm : Form
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ProductImageSettings _imageSettings;

        // --- Danh sách
        private readonly TextBox _txtSearch = new();
        private readonly ComboBox _cboFilterCategory = new FlatComboBox();
        private readonly ComboBox _cboFilterBrand = new FlatComboBox();
        private readonly DataGridView _grid = new();
        private readonly Label _lblCount = new();

        // --- Form nhập
        private readonly TextBox _txtName = new();
        private readonly TextBox _txtBarcode = new();
        private readonly TextBox _txtPrice = new();
        private readonly ComboBox _cboCategory = new FlatComboBox();
        private readonly ComboBox _cboBrand = new FlatComboBox();
        private readonly PictureBox _picImage = new();
        private readonly Label _lblNoImage = new();

        private readonly HopeRoundButton _btnAdd = new();
        private readonly HopeRoundButton _btnUpdate = new();
        private readonly HopeRoundButton _btnDelete = new();
        private readonly HopeRoundButton _btnClear = new();
        private readonly HopeRoundButton _btnPickImage = new();
        private readonly HopeRoundButton _btnRemoveImage = new();
        private readonly HopeRoundButton _btnImport = new();
        private readonly HopeRoundButton _btnExport = new();
        private readonly HopeRoundButton _btnTemplate = new();

        private List<ProductListItemDTO> _all = new();
        private IReadOnlyList<CatalogItemDTO> _categories = Array.Empty<CatalogItemDTO>();
        private IReadOnlyList<CatalogItemDTO> _brands = Array.Empty<CatalogItemDTO>();
        private readonly Dictionary<string, Image?> _thumbs = new();
        private bool _binding;

        // Ảnh của sản phẩm đang soạn: tên file đã lưu / đường dẫn ảnh vừa chọn (chưa chép vào thư mục ảnh)
        private string? _imageName;
        private string? _pendingImagePath;

        public ProductForm(IServiceScopeFactory scopeFactory, ProductImageSettings imageSettings)
        {
            _scopeFactory = scopeFactory;
            _imageSettings = imageSettings;

            Text = "Quản lý sản phẩm";
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(this.S(1200), this.S(720));
            MinimumSize = new Size(this.S(1000), this.S(600));
            BackColor = AppTheme.Background;

            BuildUi();
            Load += async (_, _) => await RunAsync(async () =>
            {
                await LoadLookupsAsync();
                await ReloadAsync();
            });
            Disposed += (_, _) =>
            {
                foreach (var img in _thumbs.Values) img?.Dispose();
                _picImage.Image?.Dispose();
            };
        }

        // ================================================================== UI
        private void BuildUi()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(this.S(24), this.S(16), this.S(24), this.S(22)),
                BackColor = AppTheme.Background
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(78)));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ----- Hàng tiêu đề: bên trái là tiêu đề, bên phải là 3 nút Excel
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.S(430)));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titles = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            titles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            titles.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(46)));
            titles.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(26)));
            titles.Controls.Add(UiKit.MakeLabel("Quản lý sản phẩm", new Font("Segoe UI Semibold", 17F),
                AppTheme.TextDark, ContentAlignment.BottomLeft), 0, 0);
            titles.Controls.Add(UiKit.MakeLabel("Thêm, sửa, tra cứu sản phẩm; nhập / xuất danh sách bằng Excel",
                new Font("Segoe UI", 9.5F), AppTheme.TextMuted, ContentAlignment.TopLeft), 0, 1);

            var excelButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty,
                Padding = new Padding(0, this.S(26), 0, this.S(4))
            };
            for (int i = 0; i < 3; i++) excelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            excelButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            UiKit.ConfigButton(_btnImport, "Nhập Excel", HopeButtonType.Primary,
                async (_, _) => await RunAsync(ImportAsync), this);
            UiKit.ConfigButton(_btnExport, "Xuất Excel", HopeButtonType.Primary,
                async (_, _) => await RunAsync(ExportAsync), this);
            UiKit.ConfigButton(_btnTemplate, "Tải file mẫu", HopeButtonType.Primary,
                async (_, _) => await RunAsync(TemplateAsync), this);
            excelButtons.Controls.Add(_btnImport, 0, 0);
            excelButtons.Controls.Add(_btnExport, 1, 0);
            excelButtons.Controls.Add(_btnTemplate, 2, 0);

            header.Controls.Add(titles, 0, 0);
            header.Controls.Add(excelButtons, 1, 0);

            // ----- Nội dung: bảng bên trái, form bên phải
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = Padding.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.S(390)));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            content.Controls.Add(BuildLeftCard(), 0, 0);
            content.Controls.Add(BuildRightCard(), 1, 0);

            page.Controls.Add(header, 0, 0);
            page.Controls.Add(content, 0, 1);
            Controls.Add(page);
        }

        private Control BuildLeftCard()
        {
            var card = new RoundedPanel
            {
                Radius = this.S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(this.S(18)),
                Margin = new Padding(0, 0, this.S(18), 0)
            };
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(58)));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(30)));

            // Hàng bộ lọc: tìm kiếm | danh mục | thương hiệu
            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = AppTheme.Surface,
                Margin = new Padding(0, 0, 0, this.S(12))
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.S(210)));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.S(220)));
            filters.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtSearch.PlaceholderText = "Tìm theo tên hoặc mã vạch...";
            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            var searchBox = UiKit.WrapInput(_txtSearch, this);
            var catBox = UiKit.WrapInput(_cboFilterCategory, this);
            var brandBox = UiKit.WrapInput(_cboFilterBrand, this);
            catBox.Margin = new Padding(this.S(8), 0, 0, 0);
            brandBox.Margin = new Padding(this.S(8), 0, 0, 0);
            _cboFilterCategory.SelectedIndexChanged += (_, _) => ApplyFilter();
            _cboFilterBrand.SelectedIndexChanged += (_, _) => ApplyFilter();
            filters.Controls.Add(searchBox, 0, 0);
            filters.Controls.Add(catBox, 1, 0);
            filters.Controls.Add(brandBox, 2, 0);

            // Bảng sản phẩm
            UiKit.StyleGrid(_grid, this, rowHeight: 52);
            _grid.DataError += (_, e) => e.ThrowException = false;
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.SelectionChanged += (_, _) => OnSelectionChanged();

            _grid.Columns.Add(new DataGridViewImageColumn
            {
                HeaderText = "Ảnh",
                DataPropertyName = nameof(ProductListItemDTO.ImageUrl),
                Width = this.S(64),
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                DefaultCellStyle = new DataGridViewCellStyle { NullValue = null, Padding = new Padding(this.S(4)) }
            });
            AddColumn("Tên sản phẩm", nameof(ProductListItemDTO.Name), fillWeight: 38);
            AddColumn("Mã vạch", nameof(ProductListItemDTO.Barcode), width: 130);
            AddColumn("Danh mục", nameof(ProductListItemDTO.CategoryName), fillWeight: 20);
            AddColumn("Thương hiệu", nameof(ProductListItemDTO.BrandName), width: 140);
            var priceCol = AddColumn("Giá bán", nameof(ProductListItemDTO.Price), width: 120);
            priceCol.DefaultCellStyle.Format = "N0";
            priceCol.DefaultCellStyle.FormatProvider = Vi;
            priceCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            var stockCol = AddColumn("Tồn", nameof(ProductListItemDTO.StockQuantity), width: 70);
            stockCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            _lblCount.Dock = DockStyle.Fill;
            _lblCount.Margin = Padding.Empty;
            _lblCount.AutoSize = false;
            _lblCount.TextAlign = ContentAlignment.MiddleLeft;
            _lblCount.ForeColor = AppTheme.TextMuted;
            _lblCount.Font = new Font("Segoe UI", 9F);

            left.Controls.Add(filters, 0, 0);
            left.Controls.Add(_grid, 0, 1);
            left.Controls.Add(_lblCount, 0, 2);
            card.Controls.Add(left);
            return card;
        }

        private DataGridViewTextBoxColumn AddColumn(string header, string property, int? width = null, float fillWeight = 0)
        {
            var col = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property };
            if (width is int w) col.Width = this.S(w);
            else
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.FillWeight = fillWeight;
            }
            _grid.Columns.Add(col);
            return col;
        }

        private Control BuildRightCard()
        {
            var card = new RoundedPanel
            {
                Radius = this.S(14),
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(this.S(22), this.S(18), this.S(14), this.S(18)),
                Margin = Padding.Empty
            };

            // Cuộn được khi màn hình thấp
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.Surface };

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = AppTheme.Surface,
                Padding = new Padding(0, 0, this.S(8), 0)
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(Control c, int height)
            {
                c.Dock = DockStyle.Fill;
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, this.S(height)));
                right.Controls.Add(c, 0, right.RowStyles.Count - 1);
            }

            var fieldFont = new Font("Segoe UI", 9.5F);

            AddRow(UiKit.MakeLabel("Thông tin sản phẩm", new Font("Segoe UI Semibold", 12.5F),
                AppTheme.TextDark, ContentAlignment.MiddleLeft), 32);
            AddRow(UiKit.MakeLabel("Chọn một dòng trong bảng để sửa hoặc xóa", new Font("Segoe UI", 9F),
                AppTheme.TextMuted, ContentAlignment.TopLeft), 24);

            // ----- Ảnh
            var picHost = new RoundedPanel
            {
                Radius = this.S(12),
                BackColor = AppTheme.ImageBg,
                Margin = new Padding(0, 0, 0, this.S(4))
            };
            _picImage.Dock = DockStyle.Fill;
            _picImage.SizeMode = PictureBoxSizeMode.Zoom;
            _picImage.BackColor = AppTheme.ImageBg;
            _lblNoImage.Text = "Chưa có ảnh";
            _lblNoImage.Dock = DockStyle.Fill;
            _lblNoImage.TextAlign = ContentAlignment.MiddleCenter;
            _lblNoImage.ForeColor = AppTheme.TextMuted;
            _lblNoImage.BackColor = AppTheme.ImageBg;
            picHost.Controls.Add(_picImage);
            picHost.Controls.Add(_lblNoImage);
            _lblNoImage.BringToFront();
            AddRow(picHost, 160);

            var imgButtons = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Surface,
                Margin = Padding.Empty,
                Padding = new Padding(0, this.S(2), 0, this.S(2))
            };
            imgButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            imgButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            imgButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            UiKit.ConfigButton(_btnPickImage, "Chọn ảnh...", HopeButtonType.Primary, (_, _) => PickImage(), this);
            UiKit.ConfigButton(_btnRemoveImage, "Bỏ ảnh", HopeButtonType.Warning, (_, _) => RemoveImage(), this);
            imgButtons.Controls.Add(_btnPickImage, 0, 0);
            imgButtons.Controls.Add(_btnRemoveImage, 1, 0);
            AddRow(imgButtons, 46);

            // ----- Các ô nhập
            _txtName.MaxLength = ProductServiceLimits.NameMax;
            _txtName.KeyPress += (_, e) => UiKit.FilterKey(e, "&(),.-/+'\"_:");
            _txtBarcode.MaxLength = ProductServiceLimits.BarcodeMax;
            _txtBarcode.KeyPress += (_, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '-') e.Handled = true;
            };
            _txtPrice.MaxLength = 17;
            _txtPrice.KeyPress += (_, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (!char.IsDigit(e.KeyChar)) e.Handled = true; // chỉ cho nhập số
            };
            _txtPrice.Leave += (_, _) => FormatPrice();
            _txtPrice.Enter += (_, _) => _txtPrice.Text = DigitsOnly(_txtPrice.Text);

            AddRow(UiKit.MakeLabel("Tên sản phẩm *", fieldFont, UiKit.LabelGray, ContentAlignment.BottomLeft), 30);
            AddRow(UiKit.WrapInput(_txtName, this), 44);
            AddRow(UiKit.MakeLabel("Mã vạch", fieldFont, UiKit.LabelGray, ContentAlignment.BottomLeft), 30);
            AddRow(UiKit.WrapInput(_txtBarcode, this), 44);
            AddRow(UiKit.MakeLabel("Giá bán (VNĐ) *", fieldFont, UiKit.LabelGray, ContentAlignment.BottomLeft), 30);
            AddRow(UiKit.WrapInput(_txtPrice, this), 44);
            AddRow(UiKit.MakeLabel("Danh mục *", fieldFont, UiKit.LabelGray, ContentAlignment.BottomLeft), 30);
            AddRow(UiKit.WrapInput(_cboCategory, this), 44);
            AddRow(UiKit.MakeLabel("Thương hiệu", fieldFont, UiKit.LabelGray, ContentAlignment.BottomLeft), 30);
            AddRow(UiKit.WrapInput(_cboBrand, this), 44);

            // ----- 4 nút chính (2 x 2)
            UiKit.ConfigButton(_btnAdd, "Thêm mới", HopeButtonType.Primary, async (_, _) => await RunAsync(AddAsync), this);
            UiKit.ConfigButton(_btnUpdate, "Cập nhật", HopeButtonType.Warning, async (_, _) => await RunAsync(UpdateAsync), this);
            UiKit.ConfigButton(_btnDelete, "Xóa", HopeButtonType.Danger, async (_, _) => await RunAsync(DeleteAsync), this);
            UiKit.ConfigButton(_btnClear, "Làm mới", HopeButtonType.Primary, (_, _) => ClearInput(), this);

            var buttons = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                BackColor = AppTheme.Surface,
                Padding = new Padding(0, this.S(16), 0, 0),
                Margin = Padding.Empty
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.Controls.Add(_btnAdd, 0, 0);
            buttons.Controls.Add(_btnUpdate, 1, 0);
            buttons.Controls.Add(_btnDelete, 0, 1);
            buttons.Controls.Add(_btnClear, 1, 1);
            AddRow(buttons, 124);

            right.RowCount = right.RowStyles.Count;
            scroll.Controls.Add(right);
            card.Controls.Add(scroll);
            return card;
        }

        // ================================================================== Dữ liệu
        private ProductListItemDTO? SelectedItem =>
            _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as ProductListItemDTO : null;

        private void OnSelectionChanged()
        {
            if (_binding) return;
            if (SelectedItem is not { } p) return;

            _txtName.Text = p.Name;
            _txtBarcode.Text = p.Barcode ?? string.Empty;
            _txtPrice.Text = p.Price.ToString("N0", Vi);
            _cboCategory.SelectedValue = p.CategoryId;
            _cboBrand.SelectedValue = p.BrandId ?? 0;

            _pendingImagePath = null;
            _imageName = p.ImageUrl;
            ShowPreview(ImagePath(p.ImageUrl));
        }

        private async Task LoadLookupsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            _categories = await scope.ServiceProvider.GetRequiredService<ICategoryService>().GetAllAsync();
            _brands = await scope.ServiceProvider.GetRequiredService<IBrandService>().GetAllAsync();

            _binding = true;
            try
            {
                Fill(_cboFilterCategory, _categories, new CatalogItemDTO(0, "Tất cả danh mục", 0));
                Fill(_cboFilterBrand, _brands, new CatalogItemDTO(0, "Tất cả thương hiệu", 0));
                Fill(_cboCategory, _categories, null);
                Fill(_cboBrand, _brands, new CatalogItemDTO(0, "(Không có)", 0));
                _cboCategory.SelectedIndex = -1;
                _cboBrand.SelectedIndex = 0;
            }
            finally { _binding = false; }
        }

        private static void Fill(ComboBox cbo, IEnumerable<CatalogItemDTO> items, CatalogItemDTO? first)
        {
            var list = new List<CatalogItemDTO>();
            if (first is not null) list.Add(first);
            list.AddRange(items);
            cbo.DataSource = null;
            cbo.DisplayMember = nameof(CatalogItemDTO.Name);
            cbo.ValueMember = nameof(CatalogItemDTO.Id);
            cbo.DataSource = list;
            if (list.Count > 0) cbo.SelectedIndex = 0;
        }

        private async Task ReloadAsync(int? selectId = null)
        {
            _all = (await Run<IProductService, IReadOnlyList<ProductListItemDTO>>(s => s.GetAllAsync())).ToList();
            ApplyFilter();

            if (selectId is int id)
            {
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.DataBoundItem is ProductListItemDTO p && p.Id == id)
                    {
                        row.Selected = true;
                        _grid.CurrentCell = row.Cells[1];
                        return;
                    }
                }
            }
            ClearFields();
        }

        private void ApplyFilter()
        {
            if (_binding) return;
            var keyword = UiKit.RemoveDiacritics(_txtSearch.Text.Trim());
            int catId = _cboFilterCategory.SelectedValue is int c ? c : 0;
            int brandId = _cboFilterBrand.SelectedValue is int b ? b : 0;

            _binding = true;
            try
            {
                var list = _all.Where(p =>
                        (catId == 0 || p.CategoryId == catId) &&
                        (brandId == 0 || p.BrandId == brandId) &&
                        (keyword.Length == 0
                         || UiKit.RemoveDiacritics(p.Name).Contains(keyword)
                         || (p.Barcode ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                _grid.DataSource = list;
                _lblCount.Text = $"Hiển thị {list.Count} / {_all.Count} sản phẩm";
                _grid.CurrentCell = null;
                _grid.ClearSelection();
            }
            finally { _binding = false; }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0) return;
            e.Value = GetThumb(e.Value as string);
            e.FormattingApplied = true;
        }

        // ================================================================== Ảnh
        private string? ImagePath(string? fileName) =>
            string.IsNullOrWhiteSpace(fileName) ? null : Path.Combine(_imageSettings.Folder, fileName);

        // Đọc ảnh qua stream rồi copy sang Bitmap để KHÔNG khóa file trên đĩa
        private static Image? LoadImage(string? path)
        {
            try
            {
                if (path is null || !File.Exists(path)) return null;
                using var fs = File.OpenRead(path);
                using var src = Image.FromStream(fs);
                return new Bitmap(src);
            }
            catch { return null; }
        }

        private Image? GetThumb(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            if (_thumbs.TryGetValue(fileName, out var cached)) return cached;

            Image? thumb = null;
            using (var full = LoadImage(ImagePath(fileName)))
            {
                if (full is not null)
                {
                    int box = this.S(44);
                    float ratio = Math.Min((float)box / full.Width, (float)box / full.Height);
                    int w = Math.Max(1, (int)(full.Width * ratio)), h = Math.Max(1, (int)(full.Height * ratio));
                    thumb = new Bitmap(w, h);
                    using var g = Graphics.FromImage(thumb);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(full, 0, 0, w, h);
                }
            }
            _thumbs[fileName] = thumb;
            return thumb;
        }

        private void ShowPreview(string? path)
        {
            var old = _picImage.Image;
            var img = LoadImage(path);
            _picImage.Image = img;
            old?.Dispose();
            _lblNoImage.Visible = img is null;
            _lblNoImage.Text = path is not null && img is null ? "Không tìm thấy file ảnh" : "Chưa có ảnh";
        }

        private void PickImage()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh sản phẩm",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            if (LoadImage(ofd.FileName) is not { } test)
            {
                MessageBox.Show("File này không phải ảnh hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            test.Dispose();
            _pendingImagePath = ofd.FileName;
            ShowPreview(ofd.FileName);
        }

        private void RemoveImage()
        {
            _pendingImagePath = null;
            _imageName = null;
            ShowPreview(null);
        }

        // ================================================================== Ô nhập
        private static string DigitsOnly(string s) => new(s.Where(char.IsDigit).ToArray());

        private void FormatPrice()
        {
            var digits = DigitsOnly(_txtPrice.Text);
            _txtPrice.Text = decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var v)
                ? v.ToString("N0", Vi) : string.Empty;
        }

        private decimal ReadPrice() =>
            decimal.TryParse(DigitsOnly(_txtPrice.Text), NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : 0;

        private void ClearFields()
        {
            _txtName.Clear();
            _txtBarcode.Clear();
            _txtPrice.Clear();
            _cboCategory.SelectedIndex = -1;
            _cboBrand.SelectedIndex = _cboBrand.Items.Count > 0 ? 0 : -1;
            _pendingImagePath = null;
            _imageName = null;
            ShowPreview(null);
        }

        private void ClearInput()
        {
            _txtSearch.Clear();
            _binding = true;
            if (_cboFilterCategory.Items.Count > 0) _cboFilterCategory.SelectedIndex = 0;
            if (_cboFilterBrand.Items.Count > 0) _cboFilterBrand.SelectedIndex = 0;
            _binding = false;
            ApplyFilter();
            ClearFields();
            _txtName.Focus();
        }

        // Gom dữ liệu trên form thành DTO; nếu có ảnh mới thì chép vào thư mục ảnh trước
        private async Task<ProductInputDTO> BuildInputAsync()
        {
            string? image = _imageName;
            if (_pendingImagePath is { } path)
            {
                image = await Run<IProductService, string>(s => Task.FromResult(s.SaveImage(path)));
                _imageName = image;
                _pendingImagePath = null;
            }

            int? brand = _cboBrand.SelectedValue is int b && b > 0 ? b : null;
            int category = _cboCategory.SelectedValue is int c ? c : 0;
            return new ProductInputDTO(_txtName.Text, _txtBarcode.Text, ReadPrice(), category, brand, image);
        }

        // ================================================================== Nút bấm
        private async Task AddAsync()
        {
            var input = await BuildInputAsync();
            int id = await Run<IProductService, int>(s => s.CreateAsync(input));
            await AfterChangeAsync(id);
            MessageBox.Show("Đã thêm sản phẩm. Tồn kho ban đầu là 0 - hãy lập phiếu nhập kho để nhập hàng.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UpdateAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trong bảng để sửa.", "Chưa chọn",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var input = await BuildInputAsync();
            await Run<IProductService>(s => s.UpdateAsync(item.Id, input));
            await AfterChangeAsync(item.Id);
            MessageBox.Show("Đã cập nhật sản phẩm.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is not { } item)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trong bảng để xóa.", "Chưa chọn",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm '{item.Name}'?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await Run<IProductService>(s => s.DeleteAsync(item.Id));
            await AfterChangeAsync(null);
        }

        private async Task AfterChangeAsync(int? selectId)
        {
            _thumbs.Clear(); // ảnh có thể đã đổi
            await ReloadAsync(selectId);
            StockEvents.RaiseChanged(); // sản phẩm mới / xóa làm thay đổi cảnh báo tồn kho
        }

        // ================================================================== Excel
        private async Task ImportAsync()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn file Excel sản phẩm",
                Filter = "Excel (*.xlsx)|*.xlsx"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            var r = await Run<IProductExcelService, ImportResultDTO>(s => s.ImportAsync(ofd.FileName));

            var sb = new StringBuilder();
            sb.AppendLine($"Tổng số dòng: {r.TotalRows}");
            sb.AppendLine($"Nhập thành công: {r.SuccessCount}");
            sb.AppendLine($"Bị lỗi (đã bỏ qua): {r.FailedCount}");
            if (r.Errors.Count > 0)
            {
                sb.AppendLine().AppendLine("Chi tiết lỗi:");
                foreach (var err in r.Errors.Take(12))
                    sb.AppendLine($"• Dòng {err.Row}: {err.Message}");
                if (r.Errors.Count > 12)
                    sb.AppendLine($"... và {r.Errors.Count - 12} dòng lỗi khác.");
            }
            MessageBox.Show(sb.ToString(), "Kết quả nhập Excel", MessageBoxButtons.OK,
                r.FailedCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (r.SuccessCount > 0) await AfterChangeAsync(null);

            if (r.Errors.Count > 12 &&
                MessageBox.Show("Lưu toàn bộ danh sách lỗi ra file văn bản?", "Lưu lỗi",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var sfd = new SaveFileDialog { Filter = "Văn bản (*.txt)|*.txt", FileName = "LoiNhapSanPham.txt" };
                if (sfd.ShowDialog(this) == DialogResult.OK)
                    await File.WriteAllLinesAsync(sfd.FileName,
                        r.Errors.Select(x => $"Dòng {x.Row}: {x.Message}"), Encoding.UTF8);
            }
        }

        private async Task ExportAsync()
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Xuất danh sách sản phẩm",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"SanPham_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            await Run<IProductExcelService>(s => s.ExportAsync(sfd.FileName));
            AskOpen(sfd.FileName, "Đã xuất danh sách sản phẩm.");
        }

        private async Task TemplateAsync()
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Lưu file mẫu",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = "Mau_Nhap_SanPham.xlsx"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            await Run<IProductExcelService>(s =>
            {
                s.CreateTemplate(sfd.FileName);
                return Task.CompletedTask;
            });
            AskOpen(sfd.FileName, "Đã tạo file mẫu. Điền dữ liệu rồi bấm \"Nhập Excel\".");
        }

        private void AskOpen(string file, string message)
        {
            if (MessageBox.Show(message + "\nMở file ngay?", "Hoàn tất", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) != DialogResult.Yes) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch { /* máy không có Excel: bỏ qua */ }
        }

        // ================================================================== Chạy thao tác
        private async Task RunAsync(Func<Task> action)
        {
            SetBusy(true);
            try
            {
                await action();
            }
            catch (BusinessException ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi hệ thống:\n{ex.GetBaseException().Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            foreach (var b in new[] { _btnAdd, _btnUpdate, _btnDelete, _btnClear, _btnPickImage,
                                      _btnRemoveImage, _btnImport, _btnExport, _btnTemplate })
                b.Enabled = !busy;
            UseWaitCursor = busy;
        }

        // Mỗi thao tác dùng một scope (một DbContext) riêng để dữ liệu luôn mới
        private async Task<TResult> Run<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<TService>());
        }

        private async Task Run<TService>(Func<TService, Task> action) where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<TService>());
        }

        // Giới hạn ký tự trên ô nhập (khớp ProductService)
        private static class ProductServiceLimits
        {
            public const int NameMax = BLL.Services.Implementations.ProductService.NameMaxLength;
            public const int BarcodeMax = BLL.Services.Implementations.ProductService.BarcodeMaxLength;
        }
    }
}

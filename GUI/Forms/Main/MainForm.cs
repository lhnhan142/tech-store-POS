using BLL.Services.Interfaces;
using GUI.Forms.Products;
using GUI.Forms.Sales;
using GUI.Forms.Warehouse;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.Forms.Main
{
    public partial class MainForm : Form
    {
        // Một "trang" trong hệ thống: thuộc nhóm menu nào, tên hiển thị, cách tạo nội dung
        private sealed record Page(string Group, string Text, Func<Control> Create);

        private static readonly Color SidebarBg = Color.FromArgb(15, 15, 60);
        private static readonly Color SidebarHover = Color.FromArgb(35, 35, 90);
        private static readonly Color Primary = Color.FromArgb(0, 112, 243);

        private readonly IServiceProvider? _sp;
        private readonly List<Page> _pages = new();
        private readonly Dictionary<Page, Button> _navButtons = new();
        private Page? _current;
        public bool LoggedOut { get; private set; }

        // Mục "Cảnh báo tồn kho" có huy hiệu hiển thị số sản phẩm đang bị cảnh báo
        private const string AlertPageText = "Cảnh báo tồn kho";
        private Page? _alertPage;

        // Constructor cho Visual Studio Designer (không có logic)
        public MainForm()
        {
            InitializeComponent();
        }

        // Constructor dùng khi chạy thật (DI tự chọn constructor này)
        public MainForm(IServiceProvider sp) : this()
        {
            _sp = sp;

            // ===== Danh sách màn hình (sửa/thêm ở đây) =====
            _pages.AddRange(new[]
            {
                new Page("Bán hàng", "Bán hàng (POS)",    () => new POSControl()),
                new Page("Kho",      "Sản phẩm",          () => AsChild(_sp!.GetRequiredService<ProductForm>())),
                new Page("Kho",      "Danh mục",          () => AsChild(_sp!.GetRequiredService<CategoryForm>())),
                new Page("Kho",      "Thương hiệu",       () => AsChild(_sp!.GetRequiredService<BrandForm>())),
                new Page("Kho",      "Nhà cung cấp",      () => AsChild(_sp!.GetRequiredService<SupplierForm>())),
                new Page("Kho",      "Phiếu nhập kho",    () => AsChild(_sp!.GetRequiredService<ReceiptForm>())),
                new Page("Kho",      "Phiếu xuất kho",    () => AsChild(_sp!.GetRequiredService<IssueForm>())),
                new Page("Kho",      "Kiểm kê kho",       () => AsChild(_sp!.GetRequiredService<StockTakeForm>())),
                new Page("Kho",      AlertPageText,       () => AsChild(_sp!.GetRequiredService<StockAlertForm>())),
                new Page("Nhân sự",  "Nhân viên",         () => Placeholder("Quản lý nhân viên")),
                new Page("Nhân sự",  "Chấm công",         () => Placeholder("Chấm công")),
                new Page("Báo cáo",  "Báo cáo doanh thu", () => Placeholder("Báo cáo doanh thu")),
            });

            _alertPage = _pages.First(p => p.Text == AlertPageText);

            BuildMenu();
            BuildSidebar();
            Load += (_, _) => NavigateTo(_pages[0]); // mặc định mở trang đầu tiên

            // Huy hiệu cảnh báo: cập nhật lúc mở app và mỗi khi tồn kho thay đổi (nhập/xuất/kiểm kê...)
            Load += async (_, _) => await RefreshAlertBadgeAsync();
            StockEvents.Changed += OnStockChanged;
            FormClosed += (_, _) => StockEvents.Changed -= OnStockChanged;
        }

        // ------------------------------------------------ Dựng phần động
        // Menu "Hệ thống" đã có trong Designer; các nhóm còn lại sinh từ _pages
        private void BuildMenu()
        {
            foreach (var group in _pages.GroupBy(p => p.Group))
            {
                var parent = new ToolStripMenuItem(group.Key);
                foreach (var p in group)
                    parent.DropDownItems.Add(p.Text, null, (_, _) => NavigateTo(p));
                menuStrip.Items.Add(parent);
            }
        }

        // Nút điều hướng bên trái, sinh từ _pages và thêm vào pnlNav
        private void BuildSidebar()
        {
            // Dock.Top: Add SAU nằm TRÊN => duyệt ngược để hiển thị đúng thứ tự
            foreach (var p in Enumerable.Reverse(_pages))
            {
                var btn = new Button
                {
                    Text = p.Text,
                    Dock = DockStyle.Top,
                    Height = 46,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    Font = new Font("Segoe UI", 10.5F),
                    ForeColor = Color.White,
                    BackColor = SidebarBg,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = SidebarHover;
                btn.Click += (_, _) => NavigateTo(p);

                _navButtons[p] = btn;
                pnlNav.Controls.Add(btn);
            }
        }

        // ------------------------------------------------- Điều hướng
        private void NavigateTo(Page page)
        {
            if (_current == page) return;

            var old = pnlContent.Controls.Cast<Control>().FirstOrDefault();
            pnlContent.Controls.Clear();
            old?.Dispose();

            var view = page.Create();
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
            if (view is Form f) f.Show(); // Form nhúng phải gọi Show()

            _current = page;
            lblTitle.Text = page.Text;
            HighlightNav(page);
        }

        // ------------------------------------------------- Huy hiệu cảnh báo tồn kho
        private void OnStockChanged() => _ = RefreshAlertBadgeAsync();

        private async Task RefreshAlertBadgeAsync()
        {
            if (_sp is null || _alertPage is null) return;
            try
            {
                using var scope = _sp.CreateScope();
                var alerts = await scope.ServiceProvider.GetRequiredService<IStockAlertService>().GetAlertsAsync();
                if (IsDisposed) return;

                var btn = _navButtons[_alertPage];
                btn.Text = alerts.Count > 0 ? $"{AlertPageText} ({alerts.Count})" : AlertPageText;
                btn.ForeColor = alerts.Count > 0 ? Color.FromArgb(255, 183, 77) : Color.White;
            }
            catch
            {
                // Huy hiệu chỉ là phần phụ: lỗi kết nối CSDL không được làm hỏng màn hình chính
            }
        }

        private void HighlightNav(Page active)
        {
            foreach (var (page, btn) in _navButtons)
                btn.BackColor = page == active ? Primary : SidebarBg;
        }

        // Biến một Form có sẵn thành "trang" nhúng được vào panel
        private static Control AsChild(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            return form;
        }

        private static Control Placeholder(string name) => new Label
        {
            Text = $"{name}\n(đang phát triển)",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F),
            ForeColor = Color.FromArgb(160, 164, 174)
        };

        // ------------------------------------------------- Sự kiện
        private void mnuLogout_Click(object? sender, EventArgs e) => Logout();
        private void mnuExit_Click(object? sender, EventArgs e) => Application.Exit();
        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var ask = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ask != DialogResult.Yes) return;

            AppSession.SignOut();
            LoggedOut = true;   // Program.cs thấy cờ này thì quay lại màn đăng nhập
            Close();
        }

        private void Logout()
        {
            var ask = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ask == DialogResult.Yes)
                Close(); // TODO: khi LoginForm dùng DI thì đóng form này và mở lại LoginForm
        }
    }
}
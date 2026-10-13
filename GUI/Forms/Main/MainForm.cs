// GUI/Forms/Main/MainForm.cs
using GUI.Forms.Sales;
using GUI.Forms.Warehouse;
using Microsoft.Extensions.DependencyInjection;
using ReaLTaiizor.Util;
using System.ComponentModel;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton; // chỉ lấy đúng 1 control, tránh trùng Button/Panel/Label với WinForms

namespace GUI.Forms.Main
{
    /// <summary>
    /// Form chính: thanh Menu (trên cùng) + thanh điều hướng bên trái + vùng nội dung dùng chung.
    /// Thêm màn hình mới chỉ cần thêm 1 dòng vào danh sách _pages.
    /// </summary>
    [DesignerCategory("Code")]
    public class MainForm : Form
    {
        // Một "trang" trong hệ thống: thuộc nhóm menu nào, tên hiển thị, cách tạo nội dung
        private sealed record Page(string Group, string Text, Func<Control> Create);

        private readonly IServiceProvider _sp;
        private readonly List<Page> _pages;
        private readonly Dictionary<Page, Button> _navButtons = new();

        private readonly Panel _pnlContent = new();
        private readonly Label _lblTitle = new();
        private Page? _current;

        private static readonly Color SidebarBg = AppTheme.TextDark;
        private static readonly Color SidebarHover = Color.FromArgb(35, 35, 90);

        public MainForm(IServiceProvider sp)
        {
            _sp = sp;

            // ===== Danh sách màn hình (sửa/thêm ở đây) =====
            _pages = new List<Page>
            {
                new Page("Bán hàng", "Bán hàng (POS)",  () => new POSControl()),
                new Page("Kho",      "Sản phẩm",        () => Placeholder("Quản lý sản phẩm")),
                new Page("Kho",      "Nhà cung cấp",    () => AsChild(_sp.GetRequiredService<SupplierForm>())),
                new Page("Nhân sự",  "Nhân viên",       () => Placeholder("Quản lý nhân viên")),
                new Page("Nhân sự",  "Chấm công",       () => Placeholder("Chấm công")),
                new Page("Báo cáo",  "Báo cáo doanh thu", () => Placeholder("Báo cáo doanh thu")),
            };

            Text = "Tech Store POS";
            Font = new Font("Segoe UI", 10F);
            BackColor = AppTheme.Background;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1280, 760);
            MinimumSize = new Size(1000, 600);

            BuildUi();
            Load += (_, _) => NavigateTo(_pages[0]); // mặc định mở trang đầu tiên
        }

        // ------------------------------------------------------------ UI
        private void BuildUi()
        {
            // Thứ tự Add quan trọng với Dock: control Add SAU được xếp trước (ngoài cùng).
            // => Fill trước, rồi Left, rồi header, cuối cùng là MenuStrip (nằm trên cùng).
            _pnlContent.Dock = DockStyle.Fill;
            _pnlContent.BackColor = AppTheme.Background;

            Controls.Add(_pnlContent);
            Controls.Add(BuildSidebar());
            Controls.Add(BuildHeader());
            Controls.Add(BuildMenu());
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip { Dock = DockStyle.Top };

            var system = new ToolStripMenuItem("Hệ thống");
            system.DropDownItems.Add("Đăng xuất", null, (_, _) => Logout());
            system.DropDownItems.Add(new ToolStripSeparator());
            system.DropDownItems.Add("Thoát", null, (_, _) => Application.Exit());
            menu.Items.Add(system);

            // Mỗi nhóm trong _pages thành một menu cha
            foreach (var group in _pages.GroupBy(p => p.Group))
            {
                var parent = new ToolStripMenuItem(group.Key);
                foreach (var p in group)
                    parent.DropDownItems.Add(p.Text, null, (_, _) => NavigateTo(p));
                menu.Items.Add(parent);
            }

            MainMenuStrip = menu;
            return menu;
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppTheme.Surface,
                Padding = new Padding(20, 10, 16, 10)
            };

            _lblTitle.Dock = DockStyle.Fill;
            _lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _lblTitle.ForeColor = AppTheme.TextDark;

            // HopeRoundButton của ReaLTaiizor (cùng loại với nút Đăng nhập ở LoginForm)
            var btnLogout = new HopeRoundButton
            {
                Text = "Đăng xuất",
                ButtonType = HopeButtonType.Primary,
                Dock = DockStyle.Right,
                Width = 120
            };
            btnLogout.Click += (_, _) => Logout();

            header.Controls.Add(_lblTitle);   // Fill trước
            header.Controls.Add(btnLogout);   // Right sau
            return header;
        }

        private Panel BuildSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = SidebarBg
            };

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
                sidebar.Controls.Add(btn);
            }

            var brand = new Label
            {
                Text = "TECH STORE POS",
                Dock = DockStyle.Top,
                Height = 64,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White
            };
            sidebar.Controls.Add(brand); // Add cuối => nằm trên cùng
            return sidebar;
        }

        // ------------------------------------------------------- Điều hướng
        private void NavigateTo(Page page)
        {
            if (_current == page) return;

            var old = _pnlContent.Controls.Cast<Control>().FirstOrDefault();
            _pnlContent.Controls.Clear();
            old?.Dispose();

            var view = page.Create();
            view.Dock = DockStyle.Fill;
            _pnlContent.Controls.Add(view);
            if (view is Form f) f.Show(); // Form nhúng phải gọi Show()

            _current = page;
            _lblTitle.Text = page.Text;
            HighlightNav(page);
        }

        private void HighlightNav(Page active)
        {
            foreach (var (page, btn) in _navButtons)
                btn.BackColor = page == active ? AppTheme.Primary : SidebarBg;
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
            ForeColor = AppTheme.TextMuted
        };

        private void Logout()
        {
            var ask = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ask == DialogResult.Yes)
                Close(); // TODO: khi LoginForm dùng DI thì đóng form này và mở lại LoginForm
        }
    }
}
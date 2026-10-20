using BLL.Exceptions;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.Forms.Auth
{
    public partial class LoginForm : Form
    {
        private readonly IServiceScopeFactory? _scopeFactory;
        private bool _busy;

        // Constructor cho Visual Studio Designer (không có logic)
        public LoginForm()
        {
            InitializeComponent();

            // Đảm bảo sự kiện luôn được nối dù Designer đã nối hay chưa:
            // gỡ trước rồi nối lại nên không bao giờ bị gọi hai lần.
            btnLogin.Click -= btnLogin_Click;
            btnLogin.Click += btnLogin_Click;
            KeyDown -= LoginForm_KeyDown;
            KeyDown += LoginForm_KeyDown;
            lklblFogotPw.Click -= lklblFogotPw_Click;
            lklblFogotPw.Click += lklblFogotPw_Click;
            KeyPreview = true;
        }

        // Constructor dùng khi chạy thật (DI tự chọn constructor này)
        public LoginForm(IServiceScopeFactory scopeFactory) : this()
        {
            _scopeFactory = scopeFactory;
        }

        // ------------------------------------------------ Sự kiện
        private async void btnLogin_Click(object? sender, EventArgs e) => await LoginAsync();

        // Nhấn Enter ở bất kỳ ô nào cũng đăng nhập (cần Form.KeyPreview = True)
        private async void LoginForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await LoginAsync();
        }

        private void lklblFogotPw_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ Quản trị viên để được cấp lại mật khẩu.",
                "Quên mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ------------------------------------------------ Xử lý đăng nhập
        private async Task LoginAsync()
        {
            if (_busy) return;

            if (_scopeFactory is null)
            {
                ShowError("Form đăng nhập đang được mở bằng new LoginForm() nên không có dịch vụ. Hãy mở qua DI (xem Program.cs).");
                return;
            }

            lblError.Text = string.Empty;
            SetBusy(true);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var user = await auth.LoginAsync(txtUsername.Text, txtPassword.Text);

                AppSession.SignIn(user.Id, user.FullName, user.RoleName);
                DialogResult = DialogResult.OK;   // đóng form, Program.cs sẽ mở MainForm
            }
            catch (BusinessException ex)
            {
                ShowError(ex.Message);
                txtPassword.Text = string.Empty;
                txtPassword.Focus();
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống (thường là không kết nối được SQL Server): hiện nguyên nhân thật để dễ sửa
                lblError.Text = "Không đăng nhập được do lỗi hệ thống.";
                lblError.Visible = true;
                MessageBox.Show(this, ex.GetBaseException().Message, "Lỗi hệ thống",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false);
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
            lblError.BringToFront();   // phòng khi nhãn bị control khác che

            // Hộp thoại đảm bảo luôn nhìn thấy lỗi. Khi giao diện ổn định, xóa dòng này nếu chỉ muốn dùng dòng đỏ.
            MessageBox.Show(this, message, "Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            txtUsername.Enabled = !busy;
            txtPassword.Enabled = !busy;
            btnLogin.Enabled = !busy;
            btnLogin.Text = busy ? "Đang đăng nhập..." : "Đăng nhập";
            UseWaitCursor = busy;
        }
    }
}
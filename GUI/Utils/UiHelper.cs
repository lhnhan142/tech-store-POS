using BLL.Exceptions;
using GUI.Themes;

namespace GUI.Utils
{
    /// <summary>
    /// Xử lý ngoại lệ tập trung cho GUI và áp dụng giao diện phẳng cho DataGridView.
    /// </summary>
    public static class UiHelper
    {
        /// <summary>
        /// Hiển thị thông báo lỗi thân thiện tiếng Việt theo BusinessErrorCode.
        /// </summary>
        public static void HandleBusinessException(BusinessException ex)
        {
            string title = ex.Code switch
            {
                BusinessErrorCode.Forbidden => "Không có quyền",
                BusinessErrorCode.ApprovalRequired => "Cần duyệt",
                BusinessErrorCode.ConcurrencyConflict => "Xung đột dữ liệu",
                BusinessErrorCode.ReturnWindowExpired => "Quá thời hạn",
                BusinessErrorCode.QtyExceedsReturnable => "Số lượng không hợp lệ",
                BusinessErrorCode.ItemNotReturnable => "Không thể đổi trả",
                BusinessErrorCode.NotFound => "Không tìm thấy",
                BusinessErrorCode.InvalidStatusTransition => "Thao tác không hợp lệ",
                _ => "Thông báo nghiệp vụ"
            };

            var icon = ex.Code switch
            {
                BusinessErrorCode.Forbidden => MessageBoxIcon.Stop,
                BusinessErrorCode.ConcurrencyConflict => MessageBoxIcon.Warning,
                _ => MessageBoxIcon.Warning
            };

            MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, icon);
        }

        /// <summary>
        /// Xử lý ngoại lệ chung: BusinessException hoặc Exception không lường trước.
        /// </summary>
        public static void HandleException(Exception ex)
        {
            if (ex is BusinessException bex)
            {
                HandleBusinessException(bex);
            }
            else
            {
                var baseEx = ex.GetBaseException();
                MessageBox.Show(
                    $"Đã xảy ra lỗi ngoài ý muốn:\n{baseEx.Message}",
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Áp dụng style phẳng, sạch sẽ chuẩn POS cho DataGridView.
        /// </summary>
        public static void ApplyFlatGridStyle(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(243, 244, 246);
            grid.BackgroundColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = false;
            grid.RowTemplate.Height = 44; // Chiều cao tối thiểu 44px theo yêu cầu

            // Header Style
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 44;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            grid.ColumnHeadersDefaultCellStyle.Font = InvoiceTheme.BodyBoldFont;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Row Style
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            grid.DefaultCellStyle.Font = InvoiceTheme.BodyFont;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 253, 245); // Màu xanh ngọc rất nhạt khi hover/select
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 95, 70);
            grid.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);

            // Alternating Row Style
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 253, 245);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 95, 70);
        }

        /// <summary>
        /// Vẽ chip trạng thái bo góc dạng viên thuốc cho cột trạng thái trong CellPainting.
        /// </summary>
        public static void PaintStatusCell(DataGridViewCellPaintingEventArgs e, string text, Color bg, Color textColor)
        {
            e.PaintBackground(e.ClipBounds, true);

            if (!string.IsNullOrEmpty(text) && e.Graphics != null)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var textSize = TextRenderer.MeasureText(text, InvoiceTheme.SmallFont);
                int chipWidth = textSize.Width + 18;
                int chipHeight = 26;
                int chipX = e.CellBounds.X + (e.CellBounds.Width - chipWidth) / 2;
                int chipY = e.CellBounds.Y + (e.CellBounds.Height - chipHeight) / 2;

                var rect = new Rectangle(chipX, chipY, chipWidth, chipHeight);
                using (var path = CreateRoundedRectanglePath(rect, 13))
                using (var brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                TextRenderer.DrawText(
                    g,
                    text,
                    InvoiceTheme.SmallFont,
                    rect,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            e.Handled = true;
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

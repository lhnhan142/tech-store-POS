using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace GUI.UserControls
{
    public partial class ucProductCard : UserControl
    {
        public event EventHandler<int>? ProductClicked;
        private int _productId;

        public ucProductCard()
        {
            InitializeComponent();
            // Ô ảnh co giãn theo chiều rộng thẻ
            Resize += (s, e) => picImage.Height = (int)((Width - Padding.Horizontal) * 0.8);


            // Click vào bất kỳ chỗ nào trên thẻ đều tính là chọn sản phẩm
            foreach (Control c in new Control[] { this, picImage, lblName, pnlBottom, lblPrice, lblStock })
            {
                c.Click += (s, e) => ProductClicked?.Invoke(this, _productId);
                c.Cursor = Cursors.Hand;
            }
        }

        public void SetData(int id, string name, decimal price, int stock, string? imagePath)
        {
            _productId = id;
            lblName.Text = name;
            lblPrice.Text = price.ToString("N0", new CultureInfo("vi-VN")) + "đ";
            lblStock.Text = $"Kho: {stock}";

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                picImage.Image = Image.FromStream(fs); // không khóa file ảnh
            }
        }

        // Hàm này do Designer tạo khi bạn nhấp đúp vào ô ảnh. Giữ lại để khỏi lỗi build.
        private void picImage_Click(object sender, EventArgs e)
        {
        }

        private void lblName_Click(object sender, EventArgs e)
        {

        }

        private void lblName_Click_1(object sender, EventArgs e)
        {

        }
    }
}
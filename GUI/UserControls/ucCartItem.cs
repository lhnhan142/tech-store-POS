using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GUI.UserControls
{
    public partial class ucCartItem : UserControl
    {
        public event EventHandler<int>? PlusClicked;
        public event EventHandler<int>? MinusClicked;
        public event EventHandler<int>? RemoveClicked;

        private int _productId;

        public ucCartItem()
        {
            InitializeComponent();
            btnPlus.Click += (s, e) => PlusClicked?.Invoke(this, _productId);
            btnMinus.Click += (s, e) => MinusClicked?.Invoke(this, _productId);
            btnRemove.Click += (s, e) => RemoveClicked?.Invoke(this, _productId);

            // PictureBox không tự dispose Image, tự giải phóng khi control bị hủy
            Disposed += (s, e) =>
            {
                picThumb.Image?.Dispose();
                picThumb.Image = null;
            };
        }

        public void SetData(int productId, string name, decimal unitPrice, int qty, string? imagePath)
        {
            _productId = productId;
            lblName.Text = name;
            lblQty.Text = qty.ToString();
            lblLineTotal.Text = (unitPrice * qty).ToString("N0", new CultureInfo("vi-VN")) + "đ";

            picThumb.Image?.Dispose();
            picThumb.Image = null;

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                // Copy ra Bitmap độc lập để không phụ thuộc stream/file (tránh lỗi GDI+)
                using var img = Image.FromFile(imagePath);
                picThumb.Image = new Bitmap(img);
            }
        }

        // Hàm này do Designer tạo khi bạn nhấp đúp vào lblName. Giữ lại để khỏi lỗi build.
        private void lblName_Click(object sender, EventArgs e)
        {
        }
    }
}
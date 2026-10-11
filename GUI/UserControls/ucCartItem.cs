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
        }

        public void SetData(int productId, string name, decimal unitPrice, int qty, string? imagePath)
        {
            _productId = productId;
            lblName.Text = name;
            lblQty.Text = qty.ToString();
            lblLineTotal.Text = (unitPrice * qty).ToString("N0", new CultureInfo("vi-VN")) + "đ";

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                picThumb.Image = Image.FromStream(fs);
            }
        }

        // Hàm này do Designer tạo khi bạn nhấp đúp vào lblName. Giữ lại để khỏi lỗi build.
        private void lblName_Click(object sender, EventArgs e)
        {
        }
    }
}
namespace GUI.Forms.Sales
{
    partial class POSControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlRight = new Panel();
            flpCart = new FlowLayoutPanel();
            pnlSummary = new Panel();
            tlpSummary = new TableLayoutPanel();
            lblTax = new Label();
            lblDiscount = new Label();
            lblTaxText = new Label();
            lblDiscountText = new Label();
            lblSubtotal = new Label();
            lblSubtotalText = new Label();
            pnlTotal = new Panel();
            lblTotal = new Label();
            lblTotalText = new Label();
            btnPay = new Button();
            lblTitle = new Label();
            pnlLeft = new Panel();
            flpProducts = new FlowLayoutPanel();
            flpCategory = new FlowLayoutPanel();
            btnCatAll = new Button();
            btnCatMouse = new Button();
            btnCatKeyboard = new Button();
            btnCatHeadset = new Button();
            btnCatMonitor = new Button();
            btnCatPad = new Button();
            btnCatParts = new Button();
            btnCatBag = new Button();
            btnCatChair = new Button();
            pnlTop = new Panel();
            pnlSearchBox = new Panel();
            txtSearch = new TextBox();
            btnSearch = new Button();
            pnlRight.SuspendLayout();
            pnlSummary.SuspendLayout();
            tlpSummary.SuspendLayout();
            pnlTotal.SuspendLayout();
            pnlLeft.SuspendLayout();
            flpCategory.SuspendLayout();
            pnlTop.SuspendLayout();
            pnlSearchBox.SuspendLayout();
            SuspendLayout();
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.White;
            pnlRight.Controls.Add(flpCart);
            pnlRight.Controls.Add(pnlSummary);
            pnlRight.Controls.Add(lblTitle);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(820, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(380, 750);
            pnlRight.TabIndex = 0;
            pnlRight.Paint += panel1_Paint;
            // 
            // flpCart
            // 
            flpCart.AutoScroll = true;
            flpCart.Dock = DockStyle.Fill;
            flpCart.FlowDirection = FlowDirection.TopDown;
            flpCart.Location = new Point(0, 60);
            flpCart.Name = "flpCart";
            flpCart.Padding = new Padding(12, 8, 12, 8);
            flpCart.Size = new Size(380, 440);
            flpCart.TabIndex = 2;
            flpCart.WrapContents = false;
            // 
            // pnlSummary
            // 
            pnlSummary.Controls.Add(tlpSummary);
            pnlSummary.Controls.Add(pnlTotal);
            pnlSummary.Controls.Add(btnPay);
            pnlSummary.Dock = DockStyle.Bottom;
            pnlSummary.Location = new Point(0, 500);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Size = new Size(380, 250);
            pnlSummary.TabIndex = 1;
            pnlSummary.Paint += pnlSummary_Paint;
            // 
            // tlpSummary
            // 
            tlpSummary.ColumnCount = 2;
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSummary.Controls.Add(lblTax, 1, 2);
            tlpSummary.Controls.Add(lblDiscount, 1, 1);
            tlpSummary.Controls.Add(lblTaxText, 0, 2);
            tlpSummary.Controls.Add(lblDiscountText, 0, 1);
            tlpSummary.Controls.Add(lblSubtotal, 1, 0);
            tlpSummary.Controls.Add(lblSubtotalText, 0, 0);
            tlpSummary.Dock = DockStyle.Fill;
            tlpSummary.Location = new Point(0, 0);
            tlpSummary.Name = "tlpSummary";
            tlpSummary.Padding = new Padding(16, 8, 16, 0);
            tlpSummary.RowCount = 3;
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tlpSummary.Size = new Size(380, 125);
            tlpSummary.TabIndex = 2;
            // 
            // lblTax
            // 
            lblTax.Dock = DockStyle.Fill;
            lblTax.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTax.ForeColor = Color.FromArgb(0, 112, 243);
            lblTax.Location = new Point(193, 84);
            lblTax.Name = "lblTax";
            lblTax.Size = new Size(168, 41);
            lblTax.TabIndex = 14;
            lblTax.Text = "\t0đ";
            lblTax.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblDiscount
            // 
            lblDiscount.Dock = DockStyle.Fill;
            lblDiscount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDiscount.ForeColor = Color.FromArgb(0, 112, 243);
            lblDiscount.Location = new Point(193, 46);
            lblDiscount.Name = "lblDiscount";
            lblDiscount.Size = new Size(168, 38);
            lblDiscount.TabIndex = 13;
            lblDiscount.Text = "\t0đ";
            lblDiscount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTaxText
            // 
            lblTaxText.Dock = DockStyle.Fill;
            lblTaxText.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTaxText.ForeColor = Color.FromArgb(160, 164, 174);
            lblTaxText.Location = new Point(19, 84);
            lblTaxText.Name = "lblTaxText";
            lblTaxText.Size = new Size(168, 41);
            lblTaxText.TabIndex = 12;
            lblTaxText.Text = "\tThuế VAT";
            lblTaxText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDiscountText
            // 
            lblDiscountText.Dock = DockStyle.Fill;
            lblDiscountText.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDiscountText.ForeColor = Color.FromArgb(160, 164, 174);
            lblDiscountText.Location = new Point(19, 46);
            lblDiscountText.Name = "lblDiscountText";
            lblDiscountText.Size = new Size(168, 38);
            lblDiscountText.TabIndex = 11;
            lblDiscountText.Text = "\tGiảm giá";
            lblDiscountText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtotal
            // 
            lblSubtotal.Dock = DockStyle.Fill;
            lblSubtotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSubtotal.ForeColor = Color.FromArgb(0, 112, 243);
            lblSubtotal.Location = new Point(193, 8);
            lblSubtotal.Name = "lblSubtotal";
            lblSubtotal.Size = new Size(168, 38);
            lblSubtotal.TabIndex = 8;
            lblSubtotal.Text = "\t0đ";
            lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblSubtotalText
            // 
            lblSubtotalText.Dock = DockStyle.Fill;
            lblSubtotalText.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSubtotalText.ForeColor = Color.FromArgb(160, 164, 174);
            lblSubtotalText.Location = new Point(19, 8);
            lblSubtotalText.Name = "lblSubtotalText";
            lblSubtotalText.Size = new Size(168, 38);
            lblSubtotalText.TabIndex = 0;
            lblSubtotalText.Text = "\tTạm tính";
            lblSubtotalText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlTotal
            // 
            pnlTotal.Controls.Add(lblTotal);
            pnlTotal.Controls.Add(lblTotalText);
            pnlTotal.Dock = DockStyle.Bottom;
            pnlTotal.Location = new Point(0, 125);
            pnlTotal.Name = "pnlTotal";
            pnlTotal.Size = new Size(380, 69);
            pnlTotal.TabIndex = 1;
            pnlTotal.Paint += pnlTotal_Paint;
            // 
            // lblTotal
            // 
            lblTotal.AllowDrop = true;
            lblTotal.Dock = DockStyle.Right;
            lblTotal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(0, 112, 243);
            lblTotal.Location = new Point(160, 0);
            lblTotal.Name = "lblTotal";
            lblTotal.Padding = new Padding(0, 0, 16, 0);
            lblTotal.Size = new Size(220, 69);
            lblTotal.TabIndex = 1;
            lblTotal.Text = "0đ";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            lblTotal.Click += label1_Click;
            // 
            // lblTotalText
            // 
            lblTotalText.Dock = DockStyle.Left;
            lblTotalText.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalText.ForeColor = Color.FromArgb(160, 164, 174);
            lblTotalText.Location = new Point(0, 0);
            lblTotalText.Name = "lblTotalText";
            lblTotalText.Size = new Size(150, 69);
            lblTotalText.TabIndex = 0;
            lblTotalText.Text = "Tổng cộng";
            lblTotalText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnPay
            // 
            btnPay.BackColor = Color.FromArgb(0, 112, 243);
            btnPay.Cursor = Cursors.Hand;
            btnPay.Dock = DockStyle.Bottom;
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.FlatStyle = FlatStyle.Flat;
            btnPay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPay.ForeColor = Color.White;
            btnPay.Location = new Point(0, 194);
            btnPay.Name = "btnPay";
            btnPay.Size = new Size(380, 56);
            btnPay.TabIndex = 0;
            btnPay.Text = "Thanh toán";
            btnPay.UseVisualStyleBackColor = false;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 15, 60);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(16, 0, 0, 0);
            lblTitle.Size = new Size(380, 60);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "\tChi tiết đơn hàng";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.WhiteSmoke;
            pnlLeft.Controls.Add(flpProducts);
            pnlLeft.Controls.Add(flpCategory);
            pnlLeft.Controls.Add(pnlTop);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(24);
            pnlLeft.Size = new Size(820, 750);
            pnlLeft.TabIndex = 1;
            // 
            // flpProducts
            // 
            flpProducts.AutoScroll = true;
            flpProducts.Dock = DockStyle.Fill;
            flpProducts.Location = new Point(24, 172);
            flpProducts.Name = "flpProducts";
            flpProducts.Padding = new Padding(0, 16, 0, 16);
            flpProducts.Size = new Size(772, 554);
            flpProducts.TabIndex = 2;
            // 
            // flpCategory
            // 
            flpCategory.AutoScroll = true;
            flpCategory.BackColor = Color.Transparent;
            flpCategory.Controls.Add(btnCatAll);
            flpCategory.Controls.Add(btnCatMouse);
            flpCategory.Controls.Add(btnCatKeyboard);
            flpCategory.Controls.Add(btnCatHeadset);
            flpCategory.Controls.Add(btnCatMonitor);
            flpCategory.Controls.Add(btnCatPad);
            flpCategory.Controls.Add(btnCatParts);
            flpCategory.Controls.Add(btnCatBag);
            flpCategory.Controls.Add(btnCatChair);
            flpCategory.Dock = DockStyle.Top;
            flpCategory.Location = new Point(24, 86);
            flpCategory.Margin = new Padding(0, 0, 14, 0);
            flpCategory.Name = "flpCategory";
            flpCategory.Padding = new Padding(0, 16, 0, 0);
            flpCategory.Size = new Size(772, 86);
            flpCategory.TabIndex = 1;
            flpCategory.WrapContents = false;
            // 
            // btnCatAll
            // 
            btnCatAll.BackColor = Color.FromArgb(0, 112, 243);
            btnCatAll.FlatAppearance.BorderSize = 0;
            btnCatAll.FlatStyle = FlatStyle.Flat;
            btnCatAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatAll.ForeColor = Color.White;
            btnCatAll.Location = new Point(3, 19);
            btnCatAll.Name = "btnCatAll";
            btnCatAll.Size = new Size(110, 42);
            btnCatAll.TabIndex = 0;
            btnCatAll.Text = "Tất cả";
            btnCatAll.UseVisualStyleBackColor = false;
            // 
            // btnCatMouse
            // 
            btnCatMouse.BackColor = Color.White;
            btnCatMouse.FlatAppearance.BorderSize = 0;
            btnCatMouse.FlatStyle = FlatStyle.Flat;
            btnCatMouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatMouse.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatMouse.Location = new Point(119, 19);
            btnCatMouse.Name = "btnCatMouse";
            btnCatMouse.Size = new Size(110, 42);
            btnCatMouse.TabIndex = 1;
            btnCatMouse.Text = "Chuột";
            btnCatMouse.UseVisualStyleBackColor = false;
            // 
            // btnCatKeyboard
            // 
            btnCatKeyboard.BackColor = Color.White;
            btnCatKeyboard.FlatAppearance.BorderSize = 0;
            btnCatKeyboard.FlatStyle = FlatStyle.Flat;
            btnCatKeyboard.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatKeyboard.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatKeyboard.Location = new Point(235, 19);
            btnCatKeyboard.Name = "btnCatKeyboard";
            btnCatKeyboard.Size = new Size(110, 42);
            btnCatKeyboard.TabIndex = 2;
            btnCatKeyboard.Text = "Bàn phím";
            btnCatKeyboard.UseVisualStyleBackColor = false;
            // 
            // btnCatHeadset
            // 
            btnCatHeadset.BackColor = Color.White;
            btnCatHeadset.FlatAppearance.BorderSize = 0;
            btnCatHeadset.FlatStyle = FlatStyle.Flat;
            btnCatHeadset.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatHeadset.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatHeadset.Location = new Point(351, 19);
            btnCatHeadset.Name = "btnCatHeadset";
            btnCatHeadset.Size = new Size(110, 42);
            btnCatHeadset.TabIndex = 3;
            btnCatHeadset.Text = "\tTai nghe";
            btnCatHeadset.UseVisualStyleBackColor = false;
            // 
            // btnCatMonitor
            // 
            btnCatMonitor.BackColor = Color.White;
            btnCatMonitor.FlatAppearance.BorderSize = 0;
            btnCatMonitor.FlatStyle = FlatStyle.Flat;
            btnCatMonitor.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatMonitor.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatMonitor.Location = new Point(467, 19);
            btnCatMonitor.Name = "btnCatMonitor";
            btnCatMonitor.Size = new Size(110, 42);
            btnCatMonitor.TabIndex = 4;
            btnCatMonitor.Text = "\tMàn hình";
            btnCatMonitor.UseVisualStyleBackColor = false;
            // 
            // btnCatPad
            // 
            btnCatPad.BackColor = Color.White;
            btnCatPad.FlatAppearance.BorderSize = 0;
            btnCatPad.FlatStyle = FlatStyle.Flat;
            btnCatPad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatPad.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatPad.Location = new Point(583, 19);
            btnCatPad.Name = "btnCatPad";
            btnCatPad.Size = new Size(110, 42);
            btnCatPad.TabIndex = 5;
            btnCatPad.Text = "Lót chuột";
            btnCatPad.UseVisualStyleBackColor = false;
            // 
            // btnCatParts
            // 
            btnCatParts.BackColor = Color.White;
            btnCatParts.FlatAppearance.BorderSize = 0;
            btnCatParts.FlatStyle = FlatStyle.Flat;
            btnCatParts.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatParts.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatParts.Location = new Point(699, 19);
            btnCatParts.Name = "btnCatParts";
            btnCatParts.Size = new Size(143, 42);
            btnCatParts.TabIndex = 6;
            btnCatParts.Text = "\tLinh kiện PC";
            btnCatParts.UseVisualStyleBackColor = false;
            // 
            // btnCatBag
            // 
            btnCatBag.BackColor = Color.White;
            btnCatBag.FlatAppearance.BorderSize = 0;
            btnCatBag.FlatStyle = FlatStyle.Flat;
            btnCatBag.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatBag.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatBag.Location = new Point(848, 19);
            btnCatBag.Name = "btnCatBag";
            btnCatBag.Size = new Size(127, 42);
            btnCatBag.TabIndex = 7;
            btnCatBag.Text = "\tBalo &&Túi";
            btnCatBag.UseVisualStyleBackColor = false;
            // 
            // btnCatChair
            // 
            btnCatChair.BackColor = Color.White;
            btnCatChair.FlatAppearance.BorderSize = 0;
            btnCatChair.FlatStyle = FlatStyle.Flat;
            btnCatChair.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCatChair.ForeColor = Color.FromArgb(15, 15, 60);
            btnCatChair.Location = new Point(981, 19);
            btnCatChair.Name = "btnCatChair";
            btnCatChair.Size = new Size(157, 42);
            btnCatChair.TabIndex = 8;
            btnCatChair.Text = "Ghế gaming";
            btnCatChair.UseVisualStyleBackColor = false;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(pnlSearchBox);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(24, 24);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(772, 62);
            pnlTop.TabIndex = 0;
            // 
            // pnlSearchBox
            // 
            pnlSearchBox.BackColor = Color.White;
            pnlSearchBox.Controls.Add(txtSearch);
            pnlSearchBox.Controls.Add(btnSearch);
            pnlSearchBox.Dock = DockStyle.Left;
            pnlSearchBox.Location = new Point(0, 0);
            pnlSearchBox.Name = "pnlSearchBox";
            pnlSearchBox.Padding = new Padding(0, 8, 8, 8);
            pnlSearchBox.Size = new Size(650, 62);
            pnlSearchBox.TabIndex = 0;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 12F);
            txtSearch.Location = new Point(26, 14);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "\tTìm sản phẩm hoặc quét mã vạch...";
            txtSearch.Size = new Size(472, 32);
            txtSearch.TabIndex = 2;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.FromArgb(0, 112, 243);
            btnSearch.Dock = DockStyle.Right;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(515, 8);
            btnSearch.Name = "btnSearch";
            btnSearch.Padding = new Padding(8, 8, 8, 0);
            btnSearch.Size = new Size(127, 46);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Tìm kiếm";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // POSControl
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);
            Name = "POSControl";
            Size = new Size(1200, 750);
            pnlRight.ResumeLayout(false);
            pnlSummary.ResumeLayout(false);
            tlpSummary.ResumeLayout(false);
            pnlTotal.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            flpCategory.ResumeLayout(false);
            pnlTop.ResumeLayout(false);
            pnlSearchBox.ResumeLayout(false);
            pnlSearchBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRight;
        private Panel pnlLeft;
        private Panel pnlTop;
        private Panel pnlSearchBox;
        private Button btnSearch;
        private TextBox txtSearch;
        private FlowLayoutPanel flpCategory;
        private Button btnCatAll;
        private Button btnCatMouse;
        private Button btnCatKeyboard;
        private Button btnCatHeadset;
        private Button btnCatMonitor;
        private Button btnCatPad;
        private Button btnCatParts;
        private Button btnCatBag;
        private Button btnCatChair;
        private FlowLayoutPanel flpProducts;
        private Panel pnlSummary;
        private Label lblTitle;
        private Panel pnlTotal;
        private Button btnPay;
        private Label lblTotalText;
        private Label lblTotal;
        private TableLayoutPanel tlpSummary;
        private Label lblSubtotalText;
        private Label lblTaxText;
        private Label lblDiscountText;
        private Label lblSubtotal;
        private Label lblTax;
        private Label lblDiscount;
        private FlowLayoutPanel flpCart;
    }
}

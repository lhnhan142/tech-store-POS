namespace GUI.UserControls
{
    partial class ucProductCard
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
            picImage = new PictureBox();
            pnlBottom = new Panel();
            lblPrice = new Label();
            lblStock = new Label();
            lblName = new Label();
            ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // picImage
            // 
            picImage.BackColor = Color.FromArgb(235, 235, 235);
            picImage.Dock = DockStyle.Top;
            picImage.Location = new Point(8, 8);
            picImage.Name = "picImage";
            picImage.Size = new Size(214, 170);
            picImage.SizeMode = PictureBoxSizeMode.Zoom;
            picImage.TabIndex = 0;
            picImage.TabStop = false;
            picImage.Click += picImage_Click;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(lblPrice);
            pnlBottom.Controls.Add(lblStock);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(8, 246);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(8, 0, 8, 0);
            pnlBottom.Size = new Size(214, 36);
            pnlBottom.TabIndex = 1;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Dock = DockStyle.Fill;
            lblPrice.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPrice.ForeColor = Color.FromArgb(0, 112, 243);
            lblPrice.Location = new Point(8, 0);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(120, 32);
            lblPrice.TabIndex = 1;
            lblPrice.Text = "\t390.000đ";
            lblPrice.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStock
            // 
            lblStock.AutoSize = true;
            lblStock.Dock = DockStyle.Right;
            lblStock.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStock.ForeColor = Color.FromArgb(160, 164, 174);
            lblStock.Location = new Point(130, 0);
            lblStock.Name = "lblStock";
            lblStock.Size = new Size(76, 25);
            lblStock.TabIndex = 0;
            lblStock.Text = "\tKho: 50";
            lblStock.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblName
            // 
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(15, 15, 60);
            lblName.Location = new Point(8, 178);
            lblName.Name = "lblName";
            lblName.Padding = new Padding(4, 8, 4, 0);
            lblName.Size = new Size(214, 68);
            lblName.TabIndex = 2;
            lblName.Text = "\tChuột Logitech G102";
            lblName.Click += lblName_Click_1;
            // 
            // ucProductCard
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(lblName);
            Controls.Add(pnlBottom);
            Controls.Add(picImage);
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, 16, 16);
            Name = "ucProductCard";
            Padding = new Padding(8);
            Size = new Size(230, 290);
            ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picImage;
        private Panel pnlBottom;
        private Label lblStock;
        private Label lblPrice;
        private Label lblName;
    }
}

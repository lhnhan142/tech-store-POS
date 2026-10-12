namespace GUI.Forms.Sales
{
    partial class PaymentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            lblTotalCaption = new Label();
            lblTotal = new Label();
            lblMethodCaption = new Label();
            cboMethod = new ComboBox();
            lblPaidCaption = new Label();
            numPaid = new NumericUpDown();
            btn50k = new Button();
            btn100k = new Button();
            btn200k = new Button();
            btn500k = new Button();
            lblChangeCaption = new Label();
            lblChange = new Label();
            btnCancel = new Button();
            btnConfirm = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPaid).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(0, 102, 255);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(4, 5, 4, 5);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(571, 100);
            pnlHeader.TabIndex = 14;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(29, 23);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(190, 45);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Thanh toán";
            // 
            // lblTotalCaption
            // 
            lblTotalCaption.AutoSize = true;
            lblTotalCaption.Font = new Font("Segoe UI", 10F);
            lblTotalCaption.ForeColor = Color.FromArgb(110, 120, 140);
            lblTotalCaption.Location = new Point(34, 130);
            lblTotalCaption.Margin = new Padding(4, 0, 4, 0);
            lblTotalCaption.Name = "lblTotalCaption";
            lblTotalCaption.Size = new Size(145, 28);
            lblTotalCaption.TabIndex = 13;
            lblTotalCaption.Text = "Cần thanh toán";
            // 
            // lblTotal
            // 
            lblTotal.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(0, 102, 255);
            lblTotal.Location = new Point(34, 167);
            lblTotal.Margin = new Padding(4, 0, 4, 0);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(503, 90);
            lblTotal.TabIndex = 12;
            lblTotal.Text = "0đ";
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMethodCaption
            // 
            lblMethodCaption.AutoSize = true;
            lblMethodCaption.Font = new Font("Segoe UI", 10F);
            lblMethodCaption.ForeColor = Color.FromArgb(110, 120, 140);
            lblMethodCaption.Location = new Point(34, 280);
            lblMethodCaption.Margin = new Padding(4, 0, 4, 0);
            lblMethodCaption.Name = "lblMethodCaption";
            lblMethodCaption.Size = new Size(225, 28);
            lblMethodCaption.TabIndex = 11;
            lblMethodCaption.Text = "Phương thức thanh toán";
            // 
            // cboMethod
            // 
            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMethod.Font = new Font("Segoe UI", 12F);
            cboMethod.Location = new Point(37, 313);
            cboMethod.Margin = new Padding(4, 5, 4, 5);
            cboMethod.Name = "cboMethod";
            cboMethod.Size = new Size(501, 40);
            cboMethod.TabIndex = 10;
            // 
            // lblPaidCaption
            // 
            lblPaidCaption.AutoSize = true;
            lblPaidCaption.Font = new Font("Segoe UI", 10F);
            lblPaidCaption.ForeColor = Color.FromArgb(110, 120, 140);
            lblPaidCaption.Location = new Point(34, 397);
            lblPaidCaption.Margin = new Padding(4, 0, 4, 0);
            lblPaidCaption.Name = "lblPaidCaption";
            lblPaidCaption.Size = new Size(143, 28);
            lblPaidCaption.TabIndex = 9;
            lblPaidCaption.Text = "Tiền khách đưa";
            // 
            // numPaid
            // 
            numPaid.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            numPaid.Increment = new decimal(new int[] { 10000, 0, 0, 0 });
            numPaid.Location = new Point(34, 437);
            numPaid.Margin = new Padding(4, 5, 4, 5);
            numPaid.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            numPaid.Name = "numPaid";
            numPaid.Size = new Size(503, 45);
            numPaid.TabIndex = 8;
            numPaid.TextAlign = HorizontalAlignment.Right;
            numPaid.ThousandsSeparator = true;
            // 
            // btn50k
            // 
            btn50k.BackColor = Color.FromArgb(235, 242, 255);
            btn50k.Cursor = Cursors.Hand;
            btn50k.FlatAppearance.BorderSize = 0;
            btn50k.FlatStyle = FlatStyle.Flat;
            btn50k.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn50k.ForeColor = Color.FromArgb(0, 102, 255);
            btn50k.Location = new Point(34, 520);
            btn50k.Margin = new Padding(4, 5, 4, 5);
            btn50k.Name = "btn50k";
            btn50k.Size = new Size(113, 63);
            btn50k.TabIndex = 7;
            btn50k.Text = "50.000";
            btn50k.UseVisualStyleBackColor = false;
            // 
            // btn100k
            // 
            btn100k.BackColor = Color.FromArgb(235, 242, 255);
            btn100k.Cursor = Cursors.Hand;
            btn100k.FlatAppearance.BorderSize = 0;
            btn100k.FlatStyle = FlatStyle.Flat;
            btn100k.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn100k.ForeColor = Color.FromArgb(0, 102, 255);
            btn100k.Location = new Point(164, 520);
            btn100k.Margin = new Padding(4, 5, 4, 5);
            btn100k.Name = "btn100k";
            btn100k.Size = new Size(113, 63);
            btn100k.TabIndex = 6;
            btn100k.Text = "100.000";
            btn100k.UseVisualStyleBackColor = false;
            // 
            // btn200k
            // 
            btn200k.BackColor = Color.FromArgb(235, 242, 255);
            btn200k.Cursor = Cursors.Hand;
            btn200k.FlatAppearance.BorderSize = 0;
            btn200k.FlatStyle = FlatStyle.Flat;
            btn200k.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn200k.ForeColor = Color.FromArgb(0, 102, 255);
            btn200k.Location = new Point(294, 520);
            btn200k.Margin = new Padding(4, 5, 4, 5);
            btn200k.Name = "btn200k";
            btn200k.Size = new Size(113, 63);
            btn200k.TabIndex = 5;
            btn200k.Text = "200.000";
            btn200k.UseVisualStyleBackColor = false;
            // 
            // btn500k
            // 
            btn500k.BackColor = Color.FromArgb(235, 242, 255);
            btn500k.Cursor = Cursors.Hand;
            btn500k.FlatAppearance.BorderSize = 0;
            btn500k.FlatStyle = FlatStyle.Flat;
            btn500k.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn500k.ForeColor = Color.FromArgb(0, 102, 255);
            btn500k.Location = new Point(424, 520);
            btn500k.Margin = new Padding(4, 5, 4, 5);
            btn500k.Name = "btn500k";
            btn500k.Size = new Size(113, 63);
            btn500k.TabIndex = 4;
            btn500k.Text = "500.000";
            btn500k.UseVisualStyleBackColor = false;
            // 
            // lblChangeCaption
            // 
            lblChangeCaption.AutoSize = true;
            lblChangeCaption.Font = new Font("Segoe UI", 10F);
            lblChangeCaption.ForeColor = Color.FromArgb(110, 120, 140);
            lblChangeCaption.Location = new Point(34, 620);
            lblChangeCaption.Margin = new Padding(4, 0, 4, 0);
            lblChangeCaption.Name = "lblChangeCaption";
            lblChangeCaption.Size = new Size(178, 28);
            lblChangeCaption.TabIndex = 3;
            lblChangeCaption.Text = "Tiền thừa trả khách";
            // 
            // lblChange
            // 
            lblChange.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblChange.ForeColor = Color.FromArgb(0, 150, 80);
            lblChange.Location = new Point(34, 657);
            lblChange.Margin = new Padding(4, 0, 4, 0);
            lblChange.Name = "lblChange";
            lblChange.Size = new Size(503, 67);
            lblChange.TabIndex = 2;
            lblChange.Text = "0đ";
            lblChange.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(240, 240, 240);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCancel.Location = new Point(34, 750);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(157, 77);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Hủy";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = Color.FromArgb(0, 102, 255);
            btnConfirm.Cursor = Cursors.Hand;
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnConfirm.ForeColor = Color.White;
            btnConfirm.Location = new Point(209, 750);
            btnConfirm.Margin = new Padding(4, 5, 4, 5);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(329, 77);
            btnConfirm.TabIndex = 0;
            btnConfirm.Text = "Xác nhận thanh toán";
            btnConfirm.UseVisualStyleBackColor = false;
            // 
            // PaymentForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancel;
            ClientSize = new Size(571, 860);
            Controls.Add(btnConfirm);
            Controls.Add(btnCancel);
            Controls.Add(lblChange);
            Controls.Add(lblChangeCaption);
            Controls.Add(btn500k);
            Controls.Add(btn200k);
            Controls.Add(btn100k);
            Controls.Add(btn50k);
            Controls.Add(numPaid);
            Controls.Add(lblPaidCaption);
            Controls.Add(cboMethod);
            Controls.Add(lblMethodCaption);
            Controls.Add(lblTotal);
            Controls.Add(lblTotalCaption);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PaymentForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thanh toán";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPaid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblMethodCaption;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.Label lblPaidCaption;
        private System.Windows.Forms.NumericUpDown numPaid;
        private System.Windows.Forms.Button btn50k;
        private System.Windows.Forms.Button btn100k;
        private System.Windows.Forms.Button btn200k;
        private System.Windows.Forms.Button btn500k;
        private System.Windows.Forms.Label lblChangeCaption;
        private System.Windows.Forms.Label lblChange;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;
    }
}
using System.Globalization;
using Common.Enums;   // enum PaymentMethod

namespace GUI.Forms.Sales
{
    public partial class PaymentForm : Form
    {
        private readonly decimal _total;
        private static readonly CultureInfo VN = new("vi-VN");

        public PaymentMethod SelectedMethod { get; private set; }
        public decimal AmountPaid { get; private set; }
        public decimal Change => Math.Max(0, AmountPaid - _total);

        public PaymentForm(decimal total)
        {
            InitializeComponent();
            _total = total;

            lblTotal.Text = total.ToString("N0", VN) + "đ";

            cboMethod.Items.Clear();
            cboMethod.Items.Add("Tiền mặt");
            cboMethod.Items.Add("Chuyển khoản");
            cboMethod.SelectedIndex = 0;
            cboMethod.SelectedIndexChanged += (s, e) => OnMethodChanged();

            numPaid.Value = Math.Min(total, numPaid.Maximum);
            numPaid.ValueChanged += (s, e) => UpdateChange();

            btn50k.Click += (s, e) => numPaid.Value = 50_000;
            btn100k.Click += (s, e) => numPaid.Value = 100_000;
            btn200k.Click += (s, e) => numPaid.Value = 200_000;
            btn500k.Click += (s, e) => numPaid.Value = 500_000;

            btnConfirm.Click += BtnConfirm_Click;
            // btnCancel đã có DialogResult = Cancel trong Designer, không cần gắn Click

            UpdateChange();
        }

        private bool IsCash => cboMethod.SelectedIndex == 0;

        private void OnMethodChanged()
        {
            // Chuyển khoản / Thẻ: khách trả đúng số tiền, không cần nhập
            bool cash = IsCash;
            numPaid.Enabled = cash;
            btn50k.Enabled = btn100k.Enabled = btn200k.Enabled = btn500k.Enabled = cash;
            if (!cash) numPaid.Value = Math.Min(_total, numPaid.Maximum);
            UpdateChange();
        }

        private void UpdateChange()
        {
            decimal change = numPaid.Value - _total;
            lblChange.Text = change >= 0
                ? change.ToString("N0", VN) + "đ"
                : "Chưa đủ";
            lblChange.ForeColor = change >= 0
                ? Color.FromArgb(0, 150, 80)
                : Color.Crimson;
            btnConfirm.Enabled = !IsCash || numPaid.Value >= _total;
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            SelectedMethod = cboMethod.SelectedIndex == 0
                ? PaymentMethod.Cash
                : PaymentMethod.Transfer;
            AmountPaid = numPaid.Value;
            DialogResult = DialogResult.OK;
        }

        private void numPaid_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
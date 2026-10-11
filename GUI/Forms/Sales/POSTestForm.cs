using System;
using System.Windows.Forms;

namespace GUI.Forms.Sales
{
    // Form chạy thử riêng màn hình POS. Khi có MainForm thì mở POSControl từ menu, không cần form này.
    public class POSTestForm : Form
    {
        public POSTestForm()
        {
            Text = "POS test";
            Width = 1440;
            Height = 900;
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new POSControl { Dock = DockStyle.Fill });
        }
    }
}
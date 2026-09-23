using GUI.Auth;
using System;
using System.Windows.Forms;

namespace GUI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Khởi chạy LoginForm đầu tiên. 
            // Nếu file LoginForm nằm trong thư mục Forms/Auth, hãy gõ đúng đường dẫn:
            Application.Run(new LoginForm());
        }
    }
}
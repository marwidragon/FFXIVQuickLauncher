using System;
using System.Net;
using System.Windows.Forms;
using XIVLauncher.WPF.Views;

namespace XIVLauncher
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // enabled TLS1.2
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;

            if (XIVLauncher.Properties.Settings.Default.useWPF)
            {
                var app = new System.Windows.Application()
                {
                    ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose
                };

                app.Run(new MainView());
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}

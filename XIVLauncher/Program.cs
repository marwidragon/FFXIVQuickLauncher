using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using XIVLauncher.WPF.Models;
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
            Settings.Instance.Load();

            // enabled TLS1.2
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;

            if (Settings.Instance.UseWPF)
            {
                var app = new System.Windows.Application()
                {
                    ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose
                };

                app.DispatcherUnhandledException += (_, e) =>
                {
                    Settings.Instance.Save();

                    var message = string.Empty;
                    message += "予期しない例外が発生しました。アプリケーションを終了します。\n\n";
                    message += e.Exception.ToString() + "\n";

                    File.WriteAllText(
                        "error.log",
                        message,
                        new UTF8Encoding(false));

                    MessageBox.Show(message, "Fatal");
                };

                app.Exit += (x, y) => Settings.Instance.Save();

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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using FolderSelect;
using Prism.Commands;
using XIVLauncher.WPF.Models;

namespace XIVLauncher.WPF.Views
{
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView :
        Window
    {
        public MainView()
        {
            this.InitializeComponent();

            this.Loaded += this.MainView_Loaded;
        }

        public SettingsModel Config => SettingsModel.Instance;

        private async void MainView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Config.SavedID))
            {
                this.IDTextBox.Focusable = true;
                this.IDTextBox.Focus();
            }
            else
            {
                this.OTPTextBox.Focusable = true;
                this.OTPTextBox.Focus();
            }

            if (!this.Config.ExistGame)
            {
                MessageBox.Show(
                    @"You will now be asked to select the path your game is installed in." + Environment.NewLine +
                    @"It should contain the folders ""game"" and ""boot"".",
                    "Select Game Path",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var fsd = new FolderSelectDialog();
                fsd.Title = "Choose your game path";

                if (fsd.ShowDialog(new WindowInteropHelper(this).Handle))
                {
                    this.Config.GamePath = fsd.FileName;
                }
            }

            if (this.Config.AutoLogin &&
                !Settings.IsAdministrator())
            {
                var stat = await Task.Run(() => XIVGame.GetGateStatus());

                if (!stat)
                {
                    MessageBox.Show(
                        "SQUARE ENIX seems to be running maintenance work right now.\nThe game shouldn't be launched.",
                        "Login failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.Config.AutoLogin = false;
                    XIVLauncher.Properties.Settings.Default.Save();

                    return;
                }

                try
                {
                    await Task.Run(() =>
                        XIVGame.LaunchGame(
                            XIVGame.GetRealSid(
                                this.Config.SavedID,
                                this.Config.SavedPW,
                                this.Config.OnetimePassword),
                            (int)this.Config.Language,
                            this.Config.IsDXII,
                            (int)this.Config.ExpansionLevel));

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Logging in failed, check your login information or try again.\n\n" + ex,
                        "Login failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private ICommand loginCommand;

        public ICommand LoginCommand =>
            this.loginCommand ?? (this.loginCommand = new DelegateCommand(async () =>
            {
                XIVLauncher.Properties.Settings.Default.Save();

                if (!this.Config.ExistGame)
                {
                    MessageBox.Show(
                        "FFXIV not found.\nPlease setup options. [Options] -> [Game Path]",
                        "Not Avalable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                var stat = await Task.Run(() => XIVGame.GetGateStatus());

                if (!stat)
                {
                    MessageBox.Show(
                        "SQUARE ENIX seems to be running maintenance work right now.\nThe game shouldn't be launched.",
                        "Login failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                try
                {
                    await Task.Run(() =>
                        XIVGame.LaunchGame(
                            XIVGame.GetRealSid(
                                this.Config.SavedID,
                                this.Config.SavedPW,
                                this.Config.OnetimePassword),
                            (int)this.Config.Language,
                            this.Config.IsDXII,
                            (int)this.Config.ExpansionLevel));

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Logging in failed, check your login information or try again.\n\n" + ex,
                        "Login failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }));

        private ICommand queueMaintenanceCommand;

        public ICommand QueueMaintenanceCommand =>
            this.queueMaintenanceCommand ?? (this.queueMaintenanceCommand = new DelegateCommand(async () =>
            {
                if (!this.Config.ExistGame)
                {
                    MessageBox.Show(
                        "FFXIV not found.\nPlease setup options. [Options] -> [Game Path]",
                        "Not Avalable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                var result = MessageBox.Show(
                    "This will be querying the maintenance status server, until the maintenance is over and then launch the game. Make sure the login information you entered is correct." +
                    "\n\n!!!The application will be unresponsive!!!\n\nDo you want to continue?",
                    "Maintenance Queue",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (XIVGame.GetGateStatus())
                        {
                            break;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                });

                Console.Beep(529, 130);
                Thread.Sleep(200);
                Console.Beep(529, 100);
                Thread.Sleep(30);
                Console.Beep(529, 100);
                Thread.Sleep(300);
                Console.Beep(420, 140);
                Thread.Sleep(300);
                Console.Beep(466, 100);
                Thread.Sleep(300);
                Console.Beep(529, 160);
                Thread.Sleep(200);
                Console.Beep(466, 100);
                Thread.Sleep(30);
                Console.Beep(529, 900);

                this.LoginCommand.Execute(null);
            }));

        private ICommand optionCommand;

        public ICommand OptionCommand =>
            this.optionCommand ?? (this.optionCommand = new DelegateCommand(() =>
            {
                var view = new OptionView()
                {
                    Owner = this
                };

                view.Show();
            }));
    }
}

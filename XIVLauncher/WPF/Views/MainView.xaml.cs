using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        Window,
        INotifyPropertyChanged
    {
        public MainView()
        {
            this.InitializeComponent();

            this.Loaded += this.MainView_Loaded;

            this.PreviewKeyDown += (x, y) =>
            {
                if (y.Key == Key.Enter)
                {
                    this.LoginButton.Focus();
                }
            };
        }

        public Settings Config => Models.Settings.Instance;

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
                !SettingsHelper.IsAdministrator() &&
                this.CanExecute())
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
                    Settings.Instance.Save();

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
                            this.Config.IsDX11,
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

        private bool CanExecute() =>
            !string.IsNullOrEmpty(this.Config.SavedID) &&
            !string.IsNullOrEmpty(this.Config.SavedPW);

        private ICommand loginCommand;

        public ICommand LoginCommand =>
            this.loginCommand ?? (this.loginCommand = new DelegateCommand(async () =>
            {
                if (!this.CanExecute())
                {
                    return;
                }

                Settings.Instance.Save();

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
                    // ツールを起動する
                    await Task.Run(() =>
                    {
                        var kicked = false;
                        foreach (var tool in Models.Settings.Instance.ToolSettings.OrderBy(x => x.Priority))
                        {
                            if (tool.Run())
                            {
                                kicked = true;
                                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                            }
                        }

                        if (kicked)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(2));
                        }
                    });

                    // FFXIVを起動する
                    await Task.Run(() =>
                        XIVGame.LaunchGame(
                            XIVGame.GetRealSid(
                                this.Config.SavedID,
                                this.Config.SavedPW,
                                this.Config.OnetimePassword),
                            (int)this.Config.Language,
                            this.Config.IsDX11,
                            (int)this.Config.ExpansionLevel));

                    // 起動したら終わる
                    await Task.Delay(TimeSpan.FromSeconds(1));
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

        private string waitingMessage = string.Empty;

        public string WaitingMessage
        {
            get => this.waitingMessage;
            set => this.SetProperty(ref this.waitingMessage, value);
        }

        private ICommand queueMaintenanceCommand;

        public ICommand QueueMaintenanceCommand =>
            this.queueMaintenanceCommand ?? (this.queueMaintenanceCommand = new DelegateCommand(async () =>
            {
                if (!this.CanExecute())
                {
                    return;
                }

#if !DEBUG
                if (!this.Config.ExistGame)
                {
                    MessageBox.Show(
                        "FFXIV not found.\nPlease setup options. [Options] -> [Game Path]",
                        "Not Avalable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }
#endif

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

                var startTime = DateTime.Now;

                await Task.Run(async () =>
                {
                    var i = 0;

                    while (true)
                    {
                        if (i == 0)
                        {
                            if (XIVGame.GetGateStatus())
                            {
                                break;
                            }
                        }

                        var span = DateTime.Now - startTime;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            this.WaitingMessage = $" {span.ToString(@"mm\:ss")}  Wating{new string('.', i)} ";
                        });

                        i++;
                        if (i > 4)
                        {
                            i = 0;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1.0));
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

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }
}

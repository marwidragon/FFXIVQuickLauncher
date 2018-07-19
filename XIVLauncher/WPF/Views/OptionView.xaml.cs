using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using FolderSelect;
using Prism.Commands;
using XIVLauncher.WPF.Models;

namespace XIVLauncher.WPF.Views
{
    /// <summary>
    /// OptionView.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionView :
        Window
    {
        public OptionView()
        {
            this.InitializeComponent();

            this.Closed += (x, y) =>
            {
                Settings.Instance.Save();
            };
        }

        public Settings Config => Models.Settings.Instance;

        public IEnumerable<FFXIVLanguages> Languages =>
            Enum.GetValues(typeof(FFXIVLanguages)).Cast<FFXIVLanguages>();

        public IEnumerable<FFXIVExpantions> Expantions =>
            Enum.GetValues(typeof(FFXIVExpantions)).Cast<FFXIVExpantions>();

        private ICommand browseGameCommand;

        public ICommand BrowseGameCommand =>
            this.browseGameCommand ?? (this.browseGameCommand = new DelegateCommand(() =>
            {
                var fsd = new FolderSelectDialog();
                fsd.Title = "Choose your game path";

                if (fsd.ShowDialog(new WindowInteropHelper(this).Handle))
                {
                    this.Config.GamePath = fsd.FileName;
                }
            }));

        private ICommand launchBackupToolCommand;

        public ICommand LaunchBackupToolCommand =>
            this.launchBackupToolCommand ?? (this.launchBackupToolCommand = new DelegateCommand(() =>
            {
                try
                {
                    var tool = Path.Combine(
                        this.Config.GamePath,
                        @"boot\ffxivconfig.exe");

                    if (File.Exists(tool))
                    {
                        Process.Start(tool);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(
                        "Could not launch ffxivconfig. Is your game path correct?\n\n" + exc,
                        "Launch failed",
                        MessageBoxButton.OK);
                }
            }));
    }
}

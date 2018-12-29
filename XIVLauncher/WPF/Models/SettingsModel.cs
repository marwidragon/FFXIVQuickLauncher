using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using Prism.Commands;
using Prism.Mvvm;
using XIVLauncher.Common;

namespace XIVLauncher.WPF.Models
{
    public enum FFXIVLanguages
    {
        Japanese = 0,
        English,
        German,
        French,
    }

    public enum FFXIVExpantions
    {
        RealmReborn = 0,
        Heavensward,
        Stormblood,
    }

    [Serializable]
    public class Settings :
        BindableBase
    {
        #region Singleton

        private static Settings instance;

        public static Settings Instance =>
            instance ?? (instance = new Settings());

        private Settings()
        {
        }

        #endregion Singleton

        #region I/O

        public static string FileName =>
            Assembly.GetEntryAssembly().Location.Replace(".exe", ".Settings.config");

        public void Load()
        {
            if (!File.Exists(FileName))
            {
                return;
            }

            using (var sr = new StreamReader(FileName, new UTF8Encoding(false)))
            {
                if (sr.BaseStream.Length > 0)
                {
                    var xs = new XmlSerializer(this.GetType());
                    var data = xs.Deserialize(sr) as Settings;
                    if (data != null)
                    {
                        instance = data;
                    }
                }
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var xs = new XmlSerializer(this.GetType());
                xs.Serialize(sw, this);
            }

            sb.Replace("utf-16", "utf-8");

            File.WriteAllText(
                FileName,
                sb.ToString(),
                new UTF8Encoding(false));
        }

        #endregion I/O

        private bool useWPF = true;

        public bool UseWPF
        {
            get => this.useWPF;
            set => this.SetProperty(ref this.useWPF, value);
        }

        private bool isDX11 = true;

        public bool IsDX11
        {
            get => this.isDX11;
            set => this.SetProperty(ref this.isDX11, value);
        }

        [XmlIgnore]
        public bool ExistGame =>
            !string.IsNullOrEmpty(this.GamePath) &&
            Directory.Exists(this.GamePath);

        private string gamePath;

        public string GamePath
        {
            get => this.gamePath;
            set => this.SetProperty(ref this.gamePath, value);
        }

        private FFXIVLanguages language = FFXIVLanguages.English;

        public FFXIVLanguages Language
        {
            get => this.language;
            set => this.SetProperty(ref this.language, value);
        }

        private string savedID;

        [XmlIgnore]
        public string SavedID
        {
            get => this.savedID;
            set => this.SetProperty(ref this.savedID, value);
        }

        [XmlElement]
        public string SavedIDEncrypted
        {
            get => Cryptor.EncryptString(this.SavedID);
            set => this.SavedID = string.IsNullOrEmpty(value) ?
                string.Empty :
                Cryptor.DecryptString(value);
        }

        private string savedPW;

        [XmlIgnore]
        public string SavedPW
        {
            get => this.savedPW;
            set => this.SetProperty(ref this.savedPW, value);
        }

        [XmlElement]
        public string SavedPWEncrypted
        {
            get => Cryptor.EncryptString(this.SavedPW);
            set => this.SavedPW = string.IsNullOrEmpty(value) ?
                string.Empty :
                Cryptor.DecryptString(value);
        }

        private string onetimePassword = string.Empty;

        [XmlIgnore]
        public string OnetimePassword
        {
            get => this.onetimePassword;
            set => this.SetProperty(ref this.onetimePassword, value);
        }

        private bool autoLogin;

        public bool AutoLogin
        {
            get => this.autoLogin;
            set => this.SetProperty(ref this.autoLogin, value);
        }

        private bool setupComplete = false;

        public bool SetupComplete
        {
            get => this.setupComplete;
            set => this.SetProperty(ref this.setupComplete, value);
        }

        private FFXIVExpantions expansionLevel = FFXIVExpantions.Stormblood;

        public FFXIVExpantions ExpansionLevel
        {
            get => this.expansionLevel;
            set => this.SetProperty(ref this.expansionLevel, value);
        }

        private double delayLaunchFFXIV = 3.0;

        public double DelayLaunchFFXIV
        {
            get => this.delayLaunchFFXIV;
            set => this.SetProperty(ref this.delayLaunchFFXIV, value);
        }

        #region Tools

        private ObservableCollection<ToolSetting> toolSettings = new ObservableCollection<ToolSetting>();

        public ObservableCollection<ToolSetting> ToolSettings => this.toolSettings;

        private CollectionViewSource toolSettingsSource;

        [XmlIgnore]
        private CollectionViewSource ToolSettingsSource =>
            this.toolSettingsSource ?? (this.toolSettingsSource = this.CreateToolSettingsSource());

        [XmlIgnore]
        public ICollectionView ToolSettingsView => this.ToolSettingsSource.View;

        private CollectionViewSource CreateToolSettingsSource()
        {
            var src = new CollectionViewSource()
            {
                Source = this.ToolSettings,
                IsLiveSortingRequested = true,
            };

            src.SortDescriptions.Add(new SortDescription(nameof(ToolSetting.IsPostProcess), ListSortDirection.Ascending));
            src.SortDescriptions.Add(new SortDescription(nameof(ToolSetting.Delay), ListSortDirection.Ascending));
            src.SortDescriptions.Add(new SortDescription(nameof(ToolSetting.Priority), ListSortDirection.Ascending));

#if DEBUG
            this.AddToolCommand.Execute(null);
#endif

            return src;
        }

        private ICommand addToolCommand;

        public ICommand AddToolCommand =>
            this.addToolCommand ?? (this.addToolCommand = new DelegateCommand(() =>
            {
                Settings.Instance.ToolSettings.Add(new ToolSetting()
                {
                    Priority = Settings.Instance.ToolSettings.Any() ?
                        Settings.Instance.ToolSettings.Max(x => x.Priority) + 1 :
                        1
                });
            }));

        [Serializable]
        public class ToolSetting :
            BindableBase
        {
            private string path;
            private bool isEnabled;
            private bool isRunAs;
            private int priority;
            private bool isPostProcess;
            private double delay = 0;

            public string Path
            {
                get => this.path;
                set => this.SetProperty(ref this.path, value);
            }

            public bool IsEnabled
            {
                get => this.isEnabled;
                set => this.SetProperty(ref this.isEnabled, value);
            }

            public bool IsRunAs
            {
                get => this.isRunAs;
                set => this.SetProperty(ref this.isRunAs, value);
            }

            public int Priority
            {
                get => this.priority;
                set => this.SetProperty(ref this.priority, value);
            }

            public bool IsPostProcess
            {
                get => this.isPostProcess;
                set => this.SetProperty(ref this.isPostProcess, value);
            }

            public double Delay
            {
                get => this.delay;
                set => this.SetProperty(ref this.delay, value);
            }

            public bool Run()
            {
                if (!this.isEnabled)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(this.path) ||
                    !File.Exists(this.path))
                {
                    return false;
                }

                // プロセスがすでに起動しているか？
                var procName = System.IO.Path.GetFileNameWithoutExtension(this.path);
                if (Process.GetProcessesByName(procName).Length > 0)
                {
                    return false;
                }

                try
                {
                    var proc = new Process();

                    proc.StartInfo.FileName = this.path;
                    proc.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(this.path);

                    if (this.isRunAs)
                    {
                        proc.StartInfo.Verb = "RunAs";
                    }

                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();

                    proc.WaitForInputIdle(10 * 1000);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private ICommand deleteCommand;

            public ICommand DeleteCommand =>
                this.deleteCommand ?? (this.deleteCommand = new DelegateCommand<ToolSetting>((tool) =>
                {
                    if (tool == null)
                    {
                        return;
                    }

                    if (Settings.Instance.ToolSettings.Contains(tool))
                    {
                        Settings.Instance.ToolSettings.Remove(tool);
                    }
                }));
        }

        #endregion Tools
    }
}

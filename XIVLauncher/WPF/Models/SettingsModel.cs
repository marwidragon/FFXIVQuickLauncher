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
    public class SettingsModel :
        BindableBase
    {
        #region Singleton

        private static SettingsModel instance;

        public static SettingsModel Instance =>
            instance ?? (instance = new SettingsModel());

        private SettingsModel()
        {
        }

        #endregion Singleton

        #region I/O

        public static string FileName =>
            Assembly.GetEntryAssembly().Location.Replace(".exe", ".config");

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
                    var data = xs.Deserialize(sr) as SettingsModel;
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

        [XmlIgnore]
        public bool IsDXII
        {
            get => XIVLauncher.Properties.Settings.Default.isdx11;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.isdx11 != value)
                {
                    XIVLauncher.Properties.Settings.Default.isdx11 = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public bool ExistGame =>
            !string.IsNullOrEmpty(this.GamePath) &&
            Directory.Exists(this.GamePath);

        [XmlIgnore]
        public string GamePath
        {
            get => XIVLauncher.Properties.Settings.Default.gamepath;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.gamepath != value)
                {
                    XIVLauncher.Properties.Settings.Default.gamepath = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(this.ExistGame));
                }
            }
        }

        [XmlIgnore]
        public FFXIVLanguages Language
        {
            get => (FFXIVLanguages)Enum.ToObject(typeof(FFXIVLanguages), XIVLauncher.Properties.Settings.Default.language);
            set
            {
                if (XIVLauncher.Properties.Settings.Default.language != (int)value)
                {
                    XIVLauncher.Properties.Settings.Default.language = (int)value;
                    this.RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public string SavedID
        {
            get => XIVLauncher.Properties.Settings.Default.savedid;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.savedid != value)
                {
                    XIVLauncher.Properties.Settings.Default.savedid = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public string SavedPW
        {
            get => XIVLauncher.Properties.Settings.Default.savedpw;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.savedpw != value)
                {
                    XIVLauncher.Properties.Settings.Default.savedpw = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string onetimePassword = string.Empty;

        [XmlIgnore]
        public string OnetimePassword
        {
            get => this.onetimePassword;
            set => this.SetProperty(ref this.onetimePassword, value);
        }

        [XmlIgnore]
        public bool AutoLogin
        {
            get => XIVLauncher.Properties.Settings.Default.autologin;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.autologin != value)
                {
                    XIVLauncher.Properties.Settings.Default.autologin = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public bool SetupComplete
        {
            get => XIVLauncher.Properties.Settings.Default.setupcomplete;
            set
            {
                if (XIVLauncher.Properties.Settings.Default.setupcomplete != value)
                {
                    XIVLauncher.Properties.Settings.Default.setupcomplete = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public FFXIVExpantions ExpansionLevel
        {
            get => (FFXIVExpantions)Enum.ToObject(typeof(FFXIVExpantions), XIVLauncher.Properties.Settings.Default.expansionlevel);
            set
            {
                if (XIVLauncher.Properties.Settings.Default.expansionlevel != (int)value)
                {
                    XIVLauncher.Properties.Settings.Default.expansionlevel = (int)value;
                    this.RaisePropertyChanged();
                }
            }
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
                SettingsModel.Instance.ToolSettings.Add(new ToolSetting()
                {
                    Priority = SettingsModel.Instance.ToolSettings.Any() ?
                        SettingsModel.Instance.ToolSettings.Max(x => x.Priority) + 1 :
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

                    if (SettingsModel.Instance.ToolSettings.Contains(tool))
                    {
                        SettingsModel.Instance.ToolSettings.Remove(tool);
                    }
                }));
        }

        #endregion Tools
    }
}

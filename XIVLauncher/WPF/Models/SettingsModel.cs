using System;
using System.IO;
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

        public bool ExistGame =>
            !string.IsNullOrEmpty(this.GamePath) &&
            Directory.Exists(this.GamePath);

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

        public string OnetimePassword
        {
            get => this.onetimePassword;
            set => this.SetProperty(ref this.onetimePassword, value);
        }

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
    }
}

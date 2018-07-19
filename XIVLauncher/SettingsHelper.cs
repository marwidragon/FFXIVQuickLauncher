using System;
using System.Security.Principal;
using Prism.Mvvm;
using XIVLauncher.WPF.Models;

namespace XIVLauncher
{
    [Serializable]
    public class SettingsHelper :
        BindableBase
    {
        public static string GetGamePath() =>
            Settings.Instance.GamePath;

        public static int GetLanguage() =>
            Convert.ToInt32(Settings.Instance.Language);

        public static bool IsDX11() =>
            Settings.Instance.IsDX11;

        public static int GetExpansionLevel() =>
            Convert.ToInt32(Settings.Instance.ExpansionLevel);

        public static bool IsAdministrator() =>
            (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
            .IsInRole(WindowsBuiltInRole.Administrator);
    }
}

using System;
using System.Security.Principal;
using Prism.Mvvm;

namespace XIVLauncher
{
    [Serializable]
    public class Settings :
        BindableBase
    {
        public static string GetGamePath() =>
            Properties.Settings.Default.gamepath;

        public static int GetLanguage() =>
            System.Convert.ToInt32(Properties.Settings.Default.language);

        public static bool IsDX11() =>
            System.Convert.ToBoolean(Properties.Settings.Default.isdx11);

        public static int GetExpansionLevel() =>
            Properties.Settings.Default.expansionlevel;

        public static bool IsAdministrator() =>
            (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
            .IsInRole(WindowsBuiltInRole.Administrator);
    }
}

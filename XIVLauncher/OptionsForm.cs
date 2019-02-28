using System;
using System.Diagnostics;
using System.Windows.Forms;
using XIVLauncher.WPF.Models;

namespace XIVLauncher
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            LanguageSelector.SelectedIndex = System.Convert.ToInt32(Settings.Instance.Language);
            dxCheckBox.Checked = Settings.Instance.IsDX11;
			steamCheckBox.Checked = Settings.Instance.UseSteam;
            comboBox1.SelectedIndex = (int)Settings.Instance.ExpansionLevel;
            pathLabel.Text = "Current Game Path:\n" + Settings.Instance.GamePath;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Settings.Instance.Language = (FFXIVLanguages)Enum.ToObject(typeof(FFXIVLanguages), LanguageSelector.SelectedIndex);
            Settings.Instance.ExpansionLevel = (FFXIVExpantions)Enum.ToObject(typeof(FFXIVExpantions), comboBox1.SelectedIndex);
            if (dxCheckBox.Checked) { Settings.Instance.IsDX11 = true; } else { Settings.Instance.IsDX11 = false; }
			if (steamCheckBox.Checked) { Settings.Instance.UseSteam = true; } else { Settings.Instance.UseSteam = false; }
            Settings.Instance.Save();
            this.Close();
        }

        private void LaunchBackupTool(object sender, EventArgs e)
        {
            try
            {
                Process backuptool = new Process();
                backuptool.StartInfo.FileName = SettingsHelper.GetGamePath() + "/boot/ffxivconfig.exe";
                backuptool.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Could not launch ffxivconfig. Is your game path correct?\n\n" + exc, "Launch failed", MessageBoxButtons.OK);
            }
        }

        private void ChangeGamePath(object sender, EventArgs e)
        {
            MessageBox.Show(@"You will now be asked to select the path your game is installed in.
It should contain the folders ""game"" and ""boot"".", "Select Game Path", MessageBoxButtons.OK);

            if (GamePathDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Instance.GamePath = GamePathDialog.SelectedPath;
                Settings.Instance.Save();
                pathLabel.Text = "Current Game Path:\n" + Settings.Instance.GamePath;
            }
        }
    }
}

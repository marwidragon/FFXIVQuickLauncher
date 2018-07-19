using System;
using System.Windows.Forms;
using XIVLauncher.WPF.Models;

namespace XIVLauncher
{
    public partial class ExpansionSelector : Form
    {
        public ExpansionSelector()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 2;
            BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Instance.ExpansionLevel = (FFXIVExpantions)Enum.ToObject(typeof(FFXIVExpantions), comboBox1.SelectedIndex);
            Settings.Instance.Save();
            Close();
        }
    }
}

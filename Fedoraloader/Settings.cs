using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fedoraloader
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.steamArgs = txtLaunchArgs.Text;
            Properties.Settings.Default.Save();
            Close();
        }

        private void Settings_Load_1(object sender, EventArgs e)
        {
            txtLaunchArgs.Text = Properties.Settings.Default.steamArgs;
        }
    }
}

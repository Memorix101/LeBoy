using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeBoy
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (leBoyForm1.emulatorThread != null && leBoyForm1.emulatorThread.IsAlive)
                leBoyForm1.emulatorThread.Abort();

            leBoyForm1.loadROM(); 
        }

        private void MainForm_ClientSizeChanged(object sender, EventArgs e)
        {
            Console.WriteLine("moo");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (leBoyForm1.emulatorThread != null && leBoyForm1.emulatorThread.IsAlive)
                leBoyForm1.emulatorThread.Abort();

            System.Environment.Exit(0);
        }
    }
}

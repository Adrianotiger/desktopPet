using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DesktopPet
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
                // ok pressed
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
                // link pressed
            Process.Start("http://esheep.petrucci.ch");
        }

        private void button2_Click(object sender, EventArgs e)
        {
                // cancel pressed (easter egg: synchronize all sheeps)
            desktopPet.Program.Mainthread.SyncSheeps();
        }
    }
}

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

            string version = "";
            version = Application.ProductVersion;
            Text = Text.Replace("XXX", version);
        }

        public void FillData(string author, string title, string version, string info)
        {
            while (info.IndexOf("[br]") > 0)
            {
                info = info.Replace("[br]", "\n");
            }
            while(info.IndexOf("[link:")>0)
            {
                int iPos = info.IndexOf("[link:");
                string link = info.Substring(iPos + 6, info.IndexOf("]", iPos + 5) - iPos - 6);
                info = info.Substring(0, iPos) + link + info.Substring(info.IndexOf("]", iPos+5) + 1);
            }

            label_author.Text = author;
            label_title.Text = title;
            label_version.Text = version;
            richTextBox1.Text = info;
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
            Close();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // link pressed
            Process.Start("https://github.com/Adrianotiger/desktopPet");
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}

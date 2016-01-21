using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DesktopPet
{
        /// <summary>
        /// Information about application and current XML animation file
        /// </summary>
    public partial class AboutBox : Form
    {
            /// <summary>
            /// Initialize form and get application version
            /// </summary>
        public AboutBox()
        {
            InitializeComponent();

            string version = "";
            version = Application.ProductVersion;
            Text = Text.Replace("XXX", version);
        }

            /// <summary>
            /// Called from parent to fill all labels on the form
            /// </summary>
            /// <param name="author">Author of the XML animation</param>
            /// <param name="title">Title of the animation (got from XML file)</param>
            /// <param name="version">Animation version (got from XML file)</param>
            /// <param name="info">Animation infos (got from XML file). Contains author and copyright information.</param>
            /// <remarks>In the info, you can't use HTML tags. But you can use:
            /// [br] to add a line break 
            /// [link:http:/...] to add a line break
            /// </remarks>
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

            /// <summary>
            /// OK was pressed. Close About dialog.
            /// </summary>
            /// <param name="sender">Caller object</param>
            /// <param name="e">Events</param>
        private void button_ok_Click(object sender, EventArgs e)
        {
            Close();
        }

            /// <summary>
            /// http://esheep.petrucci.ch was pressed, a webpage with this link will be opened
            /// </summary>
            /// <param name="sender">Caller object</param>
            /// <param name="e">Information about the link click event</param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://esheep.petrucci.ch");
        }

            /// <summary>
            /// Cancel was pressed. Synchronize pets and close about dialog.
            /// </summary>
            /// <param name="sender">Caller object</param>
            /// <param name="e">Click events</param>
        private void button2_Click(object sender, EventArgs e)
        {
            Program.Mainthread.SyncSheeps();
            Close();
        }

            /// <summary>
            /// https://github.com/Adrianotiger/desktopPet was pressed, a webpage with this link will be opened
            /// </summary>
            /// <param name="sender">Caller object</param>
            /// <param name="e">Information about the link click event</param>
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Adrianotiger/desktopPet");
        }

            /// <summary>
            /// Link on the richTextbox was pressed. Open it in the browser.
            /// </summary>
            /// <param name="sender">Caller as object</param>
            /// <param name="e">Information about the link click event</param>
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}

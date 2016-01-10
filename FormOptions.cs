using desktopPet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DesktopPet
{
    public partial class FormOptions : Form
    {
        public FormOptions()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            Close();
        }

        private void FormOptions_Load(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser web = (WebBrowser)sender;
            string s = web.DocumentText;
            if(s.Substring(0, 5) == "-XML-")
            {
                Program.Mainthread.LoadNewXMLFromString(s.Substring(5));
                this.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetEditor
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            listView1.Columns[2].Width = Width - 300;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void AddErrorLog(string text, string action, Control emitter)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.BackColor = Color.LightPink;
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
            li.Tag = emitter;
        }

        public void AddWarningLog(string text, string action, Control emitter)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.BackColor = Color.Orange;
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
            li.Tag = emitter;
        }

        public void AddLog(string text, string action)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag != null)
                {
                    if(listView1.SelectedItems[0].Tag is Control)
                    {
                        SetVisible((listView1.SelectedItems[0].Tag as Control));
                        (listView1.SelectedItems[0].Tag as Control).Focus();
                        (listView1.SelectedItems[0].Tag as Control).Select();
                    }
                }
            }
        }

        private void SetVisible(Control c)
        {
            if(c.Visible == false)
            {
                if (c.Parent != null) SetVisible(c.Parent);
                c.Visible = true;
            }
        }
    }
}

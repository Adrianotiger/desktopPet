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

        public void AddErrorLog(string text, string action)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.BackColor = Color.LightPink;
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
        }

        public void AddWarningLog(string text, string action)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.BackColor = Color.Orange;
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
        }

        public void AddLog(string text, string action)
        {
            var li = listView1.Items.Add(DateTime.Now.ToLongTimeString());
            li.SubItems.Add(action);
            li.SubItems.Add(text);
            li.EnsureVisible();
        }
    }
}

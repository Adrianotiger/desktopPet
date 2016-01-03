using desktopPet;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopPet
{
    public partial class FormDebug : Form
    {
        public FormDebug()
        {
            InitializeComponent();
        }

        private void FormDebug_Load(object sender, EventArgs e)
        {

        }

        public void AddDebugInfo(StartUp.DEBUG_TYPE type, string text)
        {
            ListViewItem item = new ListViewItem(DateTime.Now.ToLongTimeString());
            if(type == StartUp.DEBUG_TYPE.info)
            {
                item.ForeColor = Color.White;
                if (checkBox1.Checked) listView1.Items.Add(item);
            }
            else if(type == StartUp.DEBUG_TYPE.warning)
            {
                item.ForeColor = Color.Yellow;
                if (checkBox2.Checked) listView1.Items.Add(item);
            }
            else if(type == StartUp.DEBUG_TYPE.error)
            {
                item.ForeColor = Color.Salmon;
                if (checkBox3.Checked) listView1.Items.Add(item);
            }
            item.SubItems.Add(text);
            item.EnsureVisible();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

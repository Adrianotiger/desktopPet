using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopPet
{ 
        /// <summary>
        /// Debug form. If you start the application pressing the SHIFT-key a debug window will be started.<br />
        /// With this window, you can see what is happening to your pet.
        /// </summary>
    public partial class FormDebug : Form
    {
            /// <summary>
            /// Constructor of this form.
            /// </summary>
        public FormDebug()
        {
            InitializeComponent();
        }

            /// <summary>
            /// Add a debug information line to the window.
            /// </summary>
            /// <param name="type">Line type: info, warning or error.</param>
            /// <param name="text">Text to display in the window.</param>
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
    }
}

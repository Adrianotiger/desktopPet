using DesktopPet.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopPet
{
	/// <summary>
	/// Debug form. If you start the application pressing the SHIFT-key a debug window will be started.<br />
	/// With this window, you can see what is happening to your pet.
	/// </summary>
	public partial class FormDebug : Form
    {
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

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
		
		private void convertoToDOTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo("notepad");
			startInfo.UseShellExecute = false;

			Process notepad = Process.Start(startInfo);
			notepad.WaitForInputIdle();

			IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), null, null);
			SendMessage(child, 0x000c, 0, XmlToDot.ProcessXml(Animations.Xml.AnimationXML));
		}

		private void openXMLToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo("notepad");
			startInfo.UseShellExecute = false;

			Process notepad = Process.Start(startInfo);
			notepad.WaitForInputIdle();

			IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), null, null);
			SendMessage(child, 0x000c, 0, Animations.Xml.AnimationXMLString);
		}

		private void clearWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			listView1.Items.Clear();
		}

		private void removeInfosToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach(ListViewItem lv in listView1.Items)
			{
				if (lv.ForeColor == Color.White) listView1.Items.Remove(lv);
			}
		}
	}
}

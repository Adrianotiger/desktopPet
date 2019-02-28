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
        /// <summary>
        /// FindWindowEx is used to open another application
        /// </summary>
        /// <param name="hwndParent">hwnd of the parent (this application)</param>
        /// <param name="hwndChildAfter">hwnd of the next application (0)</param>
        /// <param name="lpszClass">null</param>
        /// <param name="lpszWindow">null</param>
        /// <returns>A pointer to the opened application</returns>
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// Send a message to the opened application (<see cref="FindWindowEx(IntPtr, IntPtr, string, string)"/>
        /// </summary>
        /// <param name="hWnd">hWnd of the created application pointer.</param>
        /// <param name="uMsg">Message type</param>
        /// <param name="wParam">wParam is 0</param>
        /// <param name="lParam">lParam is the text to show in the application</param>
        /// <returns></returns>
		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

		private static bool addingAnimationsLog = false;
		private static bool addingSpawnLog = false;
		private static bool addingChildLog = false;
		private static bool playingNewAnimation = false;

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
			if (addingAnimationsLog && text.StartsWith("adding animation") ||
				addingSpawnLog && text.StartsWith("adding spawn") ||
				addingChildLog && text.StartsWith("adding child"))
			{
				ListViewItem itemUpdate = listView1.Items[listView1.Items.Count-1];
				if (itemUpdate.SubItems[1].Text.Length > 64)
				{
					ListViewItem item2 = new ListViewItem(DateTime.Now.ToLongTimeString());
					item2.ForeColor = Color.White;
					if (checkBox1.Checked) listView1.Items.Add(item2);
					item2.SubItems.Add(text);
				}
				else
				{
					itemUpdate.SubItems[1].Text += "," + text.Substring(text.IndexOf(":") + 1);
				}
				return;
			}
			else
			{
				addingAnimationsLog = false;
			}

			if(playingNewAnimation)
			{
				playingNewAnimation = false;
				ListViewItem itemUpdate = listView1.Items[listView1.Items.Count - 1];
				itemUpdate.SubItems[1].Text += " - " + text;
				return;
			}

			ListViewItem item = new ListViewItem(DateTime.Now.ToLongTimeString());
			if (type == StartUp.DEBUG_TYPE.info)
			{
				item.ForeColor = Color.White;
				if (checkBox1.Checked) listView1.Items.Add(item);
			}
			else if (type == StartUp.DEBUG_TYPE.warning)
			{
				item.ForeColor = Color.Yellow;
				if (checkBox2.Checked) listView1.Items.Add(item);
			}
			else if (type == StartUp.DEBUG_TYPE.error)
			{
				item.ForeColor = Color.Salmon;
				if (checkBox3.Checked) listView1.Items.Add(item);
			}
			if (text.StartsWith("adding animation")) addingAnimationsLog = true;
			if (text.StartsWith("adding spawn")) addingSpawnLog = true;
			if (text.StartsWith("adding child")) addingChildLog = true;
			if (text.StartsWith("new animation")) playingNewAnimation = true;
			item.SubItems.Add(text);
			if(checkBox4.Checked)	item.EnsureVisible();
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

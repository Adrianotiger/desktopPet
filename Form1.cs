using DesktopPet;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace desktopPet
{
    public partial class Form1 : Form
    {
        public const int MAX_SHEEPS = 64;
        public enum DEBUG_TYPE
        {
            info    = 1,
            warning = 2,
            error   = 3,
        }

        Form2[] sheeps = new Form2[MAX_SHEEPS];
        static FormDebug debug = null;
        int iSheeps = 0;
        Xml xml;
        Animations animations;

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            xml = new Xml();
            animations = new Animations(xml);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Keys ks = ModifierKeys;
            if (ks == Keys.Shift)
            {
                debug = new FormDebug();
                debug.Show();
                AddDebugInfo(DEBUG_TYPE.info, "debug window started");
            }
            
            xml.readXML();

            timer1.Tag = "A";
            timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (iSheeps < MAX_SHEEPS)
            {
                int iXSize = pictureBox1.Image.Width / xml.images.xImages;
                int iYSize = pictureBox1.Image.Height / xml.images.yImages;
                Bitmap bmpOriginal = new Bitmap(xml.images.bitmapImages);
                sheeps[iSheeps] = new Form2(animations, xml);
                sheeps[iSheeps].Show(iXSize, iYSize);

                /*
                Width = 900;
                Top = 0;
                Left = 0;
                Height = 600;
                Visible = true;
                pictureBox1.Width = 640;
                pictureBox1.Height = 440;
                FormBorderStyle = FormBorderStyle.Sizable;
                BackColor = Color.White;
                Thread.Sleep(2000);
                */

                AddDebugInfo(DEBUG_TYPE.info, "new pet...");

                for (int i = 0; i < xml.images.xImages * xml.images.yImages; i++)
                {
                    Rectangle cropArea = new Rectangle(0 + iXSize * (i % xml.images.xImages), 0 + iYSize * (i / xml.images.xImages), iXSize, iYSize);
                    Bitmap bmpImage = new Bitmap(iXSize, iYSize, bmpOriginal.PixelFormat);
                    using (Graphics graphics = Graphics.FromImage(bmpImage))
                    {
                        Rectangle destRectangle = new Rectangle(0, 0, iXSize, iYSize);
                        Rectangle sourceRectangle = new Rectangle(0 + iXSize * (i % xml.images.xImages), 0 + iYSize * (i / xml.images.xImages), iXSize, iYSize);
                        graphics.DrawImage(bmpOriginal, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    sheeps[iSheeps].addImage(bmpImage);
                }

                AddDebugInfo(DEBUG_TYPE.info, (xml.images.xImages * xml.images.yImages).ToString() + " frames added");

                sheeps[iSheeps].Play(true);
                iSheeps++;
            }
            else
            {
                AddDebugInfo(DEBUG_TYPE.warning, "max PETs reached");
            }
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Y < Height / 3)
            {
                DialogResult res;
                AddDebugInfo(DEBUG_TYPE.info, "Info pressed");
                do
                {
                    res = MessageBox.Show("Application Version 1.0 (beta 0.1)\nAdriano Petrucci\n____________________________\nAnimation pack from: " + xml.headerInfo.Author + "\nInfo: " + xml.headerInfo.Info + "\nVersion: " + xml.headerInfo.Version, xml.headerInfo.Title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);
                    if (res == DialogResult.Retry)
                    {
                        button1_Click(sender, e);
                    }
                    else if (res == DialogResult.Abort)
                    {
                        for (int i = 0; i < iSheeps; i++)
                        {
                            sheeps[i].Whats();
                        }
                    }
                } while (res == DialogResult.Retry);
            }
            else if (e.X < Width / 2)
            {
                AddDebugInfo(DEBUG_TYPE.info, "new pet pressed");
                button1_Click(sender, e);
            }
            else
            {
                AddDebugInfo(DEBUG_TYPE.info, "exit application pressed");
                timer1.Tag = "0";
                timer1.Enabled = true;
                Opacity = 0.0;
                for (int i = 0; i < iSheeps; i++)
                {
                    sheeps[i].Kill();
                }
            }
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1.0;
            button2.Visible = true;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            Opacity = 0.8;
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1200);
                Invoke(new Action(() => { if (Opacity < 1.0) button2.Visible = false; }));
            })).Start();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            string sDesc;
            string sTitle;
            if (e.Y < Height / 3)
            {
                sTitle = "Info";
                toolTip1.ToolTipIcon = ToolTipIcon.Info;
                sDesc = "Info about this application";
            }
            else if (e.X < Width / 2)
            {
                sTitle = "More";
                toolTip1.ToolTipIcon = ToolTipIcon.None;
                sDesc = "Add another one to \n your desktop";
            }
            else
            {
                sTitle = "Kill all";
                toolTip1.ToolTipIcon = ToolTipIcon.Warning;
                sDesc = "Close application and \n clean desktop";
            }

            if (toolTip1.ToolTipTitle != sTitle)
            {
                toolTip1.ToolTipTitle = sTitle;
                toolTip1.SetToolTip(pictureBox2, sDesc);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Tag.ToString() == "A")
            {
                timer1.Enabled = false;
                timer1.Tag = "B";

                AddDebugInfo(DEBUG_TYPE.info, "init application...");

                // Set images
                pictureBox2.Image = new Bitmap(xml.bitmapHome);
                pictureBox2.Width = pictureBox2.Image.Width;
                pictureBox2.Height = pictureBox2.Image.Height;
                Width = pictureBox2.Width;
                Height = pictureBox2.Height;

                pictureBox1.Image = new Bitmap(xml.images.bitmapImages);

                Icon = new Icon(xml.bitmapIcon);

                // Set info texts
                toolTip1.SetToolTip(pictureBox2, xml.headerInfo.Title);

                // set home position
                Top = Screen.PrimaryScreen.WorkingArea.Height - Height;
                Left = Screen.PrimaryScreen.WorkingArea.Width - Width - 50;

                xml.loadAnimations(animations);

                button2.Visible = false;
                Opacity = 1.0;

                button1_Click(sender, e);
            }
            else if (timer1.Tag.ToString() == "0")
            {
                timer1.Tag = "1";
            }
            else
            {
                Close();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            AddDebugInfo(DEBUG_TYPE.info, "dragging file...");
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            AddDebugInfo(DEBUG_TYPE.info, "files dragged:");
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                AddDebugInfo(DEBUG_TYPE.info, file); 
            }
        }

        public static void AddDebugInfo(DEBUG_TYPE type, string text)
        {
            if(debug != null)
            {
                debug.AddDebugInfo(type, text);
            }
        }
    }
}

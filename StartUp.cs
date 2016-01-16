using DesktopPet;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace desktopPet
{
    public sealed class StartUp : IDisposable
    {
            // Maximal sheeps (too much sheeps will cover too much the screen and would not be nice to see)
        public const int MAX_SHEEPS = 16;

            // DEBUG TYPE. If you press "SHIFT" by starting the application, a debug Window will appear
        public enum DEBUG_TYPE
        {
            info    = 1,
            warning = 2,
            error   = 3,
        }

            // A timer to allow some times to the sheeps to die, before the application will close definitively
        static public System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();

            // Each sheep is in a different form
        Form2[] sheeps = new Form2[MAX_SHEEPS];
            // Debug window
        static FormDebug debug = null;
            // Number of active sheeps
        int iSheeps = 0;
            // The XML file where all animations are defined
        Xml xml;
            // Class of the animations
        Animations animations;
            // Process Icon
        ProcessIcon pi;

        public StartUp(ProcessIcon processIcon)
        {
            pi = processIcon;

                // Init XML class
            xml = new Xml();
                // Init Animations class
            animations = new Animations(xml);

                // If SHIFT key was pressed, open Debug window
            Keys ks = Control.ModifierKeys;
            if (ks == Keys.Shift)
            {
                debug = new FormDebug();
                debug.Show();
                AddDebugInfo(DEBUG_TYPE.info, "debug window started");
            }
            
                // Read XML file and start new sheep in 1 second
            if(!xml.readXML())
            {
                DesktopPet.Properties.Settings.Default.xml = DesktopPet.Properties.Resources.animations;
                DesktopPet.Properties.Settings.Default.Save();
                xml.readXML();
            }

                // Set animation icon
            pi.SetIcon(xml.bitmapIcon, xml.headerInfo.PetName, xml.headerInfo.Author, xml.headerInfo.Title, xml.headerInfo.Version, xml.headerInfo.Info);

                // Wait 1 second, before starting first animation
            timer1.Tag = "A";
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000;
            timer1.Enabled = true;
        }

        public void Dispose()
        {
            xml.Dispose();
        }
        
        public void AddSheep()
        {
            if (iSheeps < MAX_SHEEPS)
            {
                    // Get the image from XML file
                    // An imagelist will be filled with single images picked from sprite sheet.
                    // Each image frame is stored and id begins from 0 (top left).
                    // ToDo: maybe a better way would be store the imagelist in this class and each form can access the
                    //       images from this class. This will save some memory in the RAM.
                Bitmap bmpOriginal = new Bitmap(xml.images.bitmapImages);
                int iXSize = bmpOriginal.Width / xml.images.xImages;
                int iYSize = bmpOriginal.Height / xml.images.yImages;
                sheeps[iSheeps] = new Form2(animations, xml);
                sheeps[iSheeps].Show(iXSize, iYSize);
                
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

        /// <summary>
        /// Close all sheeps on the desktop and eventually closes the application
        /// </summary>
        /// <param name="exit">If true, the application will close after 1 second (leaving time to the sheeps to die).</param>
        public void KillSheeps(bool exit)
        {
            AddDebugInfo(DEBUG_TYPE.info, "Killing all sheeps");
            timer1.Tag = "0";
            pi.Dispose();

            Random rand = new Random();
            for (int i = 0; i < iSheeps; i++)
            {
                Thread.Sleep(rand.Next(100, 200));
                sheeps[i].Kill();
                Application.DoEvents();
            }
            iSheeps = 0;

            if (exit)
            {
                timer1.Interval = 1100;
                timer1.Enabled = true;
            }
        }
        
        // Timer used to init and close the application 
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Tag.ToString() == "A")
            {
                timer1.Enabled = false;
                timer1.Tag = "B";

                AddDebugInfo(DEBUG_TYPE.info, "init application...");
                
                xml.loadAnimations(animations);

                AddSheep();
            }
            else if (timer1.Tag.ToString() == "0")
            {
                timer1.Tag = "1";
            }
            else
            {
                Application.Exit();
            }
        }
        
        /// <summary>
        /// Load new XML (from XML string)
        /// </summary>
        /// <param name="strXml">A string with the xml content.</param>
        public void LoadNewXMLFromString(string strXml)
        {
            AddDebugInfo(DEBUG_TYPE.info, "load new XML string");

            // Close all sheeps
            for (int i = 0; i < iSheeps; i++)
            {
                sheeps[i].Close();
                sheeps[i].Dispose();
            }
            iSheeps = 0;

            // reload XML and Animations
            xml = new Xml();
            animations = new Animations(xml);

            DesktopPet.Properties.Settings.Default.xml = strXml;
            DesktopPet.Properties.Settings.Default.Save();

            if (!xml.readXML())
            {
                DesktopPet.Properties.Settings.Default.xml = DesktopPet.Properties.Resources.animations;
                DesktopPet.Properties.Settings.Default.Save();
                xml.readXML();
            }

            pi.SetIcon(xml.bitmapIcon, xml.headerInfo.PetName, xml.headerInfo.Author, xml.headerInfo.Title, xml.headerInfo.Version, xml.headerInfo.Info);

            timer1.Tag = "A";
            timer1.Enabled = true;
        }

        /// <summary>
        /// If the application is started with the SHIFT key pressed, warnings and errors are reported on a window
        /// </summary>
        /// <param name="type">See <see cref="StartUp.DEBUG_TYPE"/> for the possible values. </param>
        /// <param name="text">Text to show in the dialog window.</param>
        public static void AddDebugInfo(DEBUG_TYPE type, string text)
        {
            if(debug != null)
            {
                debug.AddDebugInfo(type, text);
            }
        }

        /// <summary>
        /// Calling this function, all sheeps will execute the same animation (if the sync-word is present in the XML)
        /// </summary>
        public void SyncSheeps()
        {
            AddDebugInfo(DEBUG_TYPE.info, "synchronize Sheeps");
            for (int i = 0; i < iSheeps; i++)
            {
                sheeps[i].Sync();
            }
        }

        /// <summary>
        /// Open the option dialog, to show some options like reset XML animation or load animation from the webpage.
        /// </summary>
        public void OpenOptionDialog()
        {
            FormOptions formoptions = new FormOptions();
            switch (formoptions.ShowDialog())
            {
                case DialogResult.Retry:
                    AddDebugInfo(DEBUG_TYPE.warning, "restoring default XML");

                    DesktopPet.Properties.Settings.Default.Icon = "";
                    DesktopPet.Properties.Settings.Default.Images = "";

                    LoadNewXMLFromString("");
                    break;
            }
        }
    }
}

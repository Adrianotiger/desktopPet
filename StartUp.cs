using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DesktopPet
{
        /// <summary>
        /// StartUp class. This class will initialize the entire application and define some constants.
        /// </summary>
    public sealed class StartUp : IDisposable
    {
            /// <summary>
            /// Maximal sheeps (too much sheeps will cover too much the screen and would not be nice to see).
            /// </summary>
        public const int MAX_SHEEPS = 16;

            /// <summary>
            /// DEBUG TYPE. If you press "SHIFT" by starting the application, a debug Window will appear.
            /// </summary>
        public enum DEBUG_TYPE
        {
                /// <summary>
                /// Only info, to show what is happening.
                /// </summary>
            info    = 1,
                /// <summary>
                /// Something important happened or something that was not expected.
                /// </summary>
            warning = 2,
                /// <summary>
                /// An error is occurred. The application need to do something that was not expected.
                /// </summary>
            error   = 3,
        }

            /// <summary>
            /// A timer to allow some times to the sheeps to die, before the application will close definitively.
            /// </summary>
        static public System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();

            /// <summary>
            /// Each sheep is in a different form.
            /// </summary>
        Form2[] sheeps = new Form2[MAX_SHEEPS];

            /// <summary>
            /// Debug window, used only if SHIFT was pressed by starting the application.
            /// </summary>
        static FormDebug debug = null;

            /// <summary>
            /// Number of currently active sheeps.
            /// </summary>
        int iSheeps = 0;

            /// <summary>
            /// The XML file where all animations are defined.
            /// </summary>
        Xml xml;

            /// <summary>
            /// Class of the animations. All animations are stored there.
            /// </summary>
        Animations animations;

            /// <summary>
            /// Process Icon. The tray icon on the taskbar.
            /// </summary>
        ProcessIcon pi;

            /// <summary>
            /// Constructor. Called when application is started.
            /// </summary>
            /// <param name="processIcon">ProcessIcon class, to change icon when a new pet is selected.</param>
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
                Properties.Settings.Default.xml = Properties.Resources.animations;
                Properties.Settings.Default.Save();
                xml.readXML();
            }

                // Set animation icon
            pi.SetIcon(xml.bitmapIcon, 
                        xml.AnimationXML.Header.Petname, 
                        xml.AnimationXML.Header.Author, 
                        xml.AnimationXML.Header.Title, 
                        xml.AnimationXML.Header.Version, 
                        xml.AnimationXML.Header.Info
                        );

                // Wait 1 second, before starting first animation
            timer1.Tag = "A";
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000;
            timer1.Enabled = true;

            if(Program.ArgumentInstall == "yes")
            {
                Install install = new Install();
                install.ShowInstallation();
            }
        }

            /// <summary>
            /// Dispose class -> used to dispose xml class
            /// </summary>
        public void Dispose()
        {
            xml.Dispose();
            pi.Dispose();
        }
        
            /// <summary>
            /// Calling this function will add another sheep on the desktop, if MAX_SHEEP was not reached.
            /// </summary>
        public void AddSheep()
        {
            if (iSheeps < MAX_SHEEPS)
            {
                    // Get the image from XML file
                    // An imageList will be filled with single images picked from sprite sheet.
                    // Each image frame is stored and id begins from 0 (top left).
                    // ToDo: maybe a better way would be store the imageList in this class and each form can access the
                    //       images from this class. This will save some memory in the RAM.
                Bitmap bmpOriginal = new Bitmap(xml.images.bitmapImages);
                int iXSize = bmpOriginal.Width / xml.AnimationXML.Image.TilesX;
                int iYSize = bmpOriginal.Height / xml.AnimationXML.Image.TilesY;
                sheeps[iSheeps] = new Form2(animations, xml);
                sheeps[iSheeps].Show(iXSize, iYSize);
                
                AddDebugInfo(DEBUG_TYPE.info, "new pet...");

                for (int i = 0; i < xml.AnimationXML.Image.TilesX * xml.AnimationXML.Image.TilesY; i++)
                {
                    Rectangle cropArea = new Rectangle(0 + iXSize * (i % xml.AnimationXML.Image.TilesX), 0 + iYSize * (i / xml.AnimationXML.Image.TilesX), iXSize, iYSize);
                    Bitmap bmpImage = new Bitmap(iXSize, iYSize, bmpOriginal.PixelFormat);
                    using (Graphics graphics = Graphics.FromImage(bmpImage))
                    {
                        Rectangle destRectangle = new Rectangle(0, 0, iXSize, iYSize);
                        Rectangle sourceRectangle = new Rectangle(0 + iXSize * (i % xml.AnimationXML.Image.TilesX), 0 + iYSize * (i / xml.AnimationXML.Image.TilesX), iXSize, iYSize);
                        graphics.DrawImage(bmpOriginal, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    sheeps[iSheeps].addImage(bmpImage);
                }

                AddDebugInfo(DEBUG_TYPE.info, (xml.AnimationXML.Image.TilesX * xml.AnimationXML.Image.TilesY).ToString() + " frames added");

                    // Start the animation of the pet
                sheeps[iSheeps].Play(true);
                iSheeps++;
            }
            else
            {
                AddDebugInfo(DEBUG_TYPE.warning, "max PETs reached");
            }
        }

            /// <summary>
            /// Close all sheeps on the desktop and eventually closes the application.
            /// </summary>
            /// <param name="exit">If true, the application will close after 1 second (leaving time to the sheeps to die).</param>
        public void KillSheeps(bool exit)
        {
            AddDebugInfo(DEBUG_TYPE.info, "Killing all sheeps");
            timer1.Tag = "0";
            pi.Dispose();

                // Leave open application to show some kill animations. 
                // Only if there is a pet on the desktop.
            if (iSheeps > 0)
            {
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
            else
            {
                timer1.Interval = 100;
                timer1.Enabled = true;
            }
        }

            /// <summary>
            /// Close a single sheep on the desktop.
            /// </summary>
            /// <param name="sheep">The sheep-form to close.</param>
        public bool KillSheep(Form2 sheep)
        {
            bool bSheepRemoved = false;

            AddDebugInfo(DEBUG_TYPE.info, "Kill one sheep");
            
            for (int i = 0; i < iSheeps; i++)
            {
                if(sheeps[i] == sheep)
                {
                    sheeps[i].Kill();
                    for (int j = i; j < iSheeps - 1; j++) sheeps[j] = sheeps[j + 1];
                    iSheeps--;
                    Application.DoEvents();
                    bSheepRemoved = true;
                    break;
                }
            }

            /*
             * This will close application if all Sheeps are removed. But Maybe the user want see the try icon to add a sheep later.
             * Maybe in future you can choose 0 to 10 sheeps at startup, so this is commented out for the moment.
            if (iSheeps <= 0)
            {
                timer1.Tag = "0";
                pi.Dispose();

                timer1.Interval = 1100;
                timer1.Enabled = true;
            }
            */

            return bSheepRemoved;
        }

            /// <summary>
            /// Timer used to init and close the application.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Timer event values.</param>
        private void timer1_Tick(object sender, EventArgs e)
        {
                // "A" when application starts. Add a sheep.
            if (timer1.Tag.ToString() == "A")
            {
                timer1.Enabled = false;
                timer1.Tag = "B";

                AddDebugInfo(DEBUG_TYPE.info, "init application...");
                
                xml.loadAnimations(animations);

                AddSheep();
            }
                // "0" when application should be stopped.
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
            /// Load new XML (from XML string).
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

            Properties.Settings.Default.xml = strXml;
            Properties.Settings.Default.Save();

            if (!xml.readXML())
            {
                Properties.Settings.Default.xml = Properties.Resources.animations;
                Properties.Settings.Default.Save();
                xml.readXML();
            }

            pi.SetIcon(
                xml.bitmapIcon, 
                xml.AnimationXML.Header.Petname, 
                xml.AnimationXML.Header.Author, 
                xml.AnimationXML.Header.Title, 
                xml.AnimationXML.Header.Version, 
                xml.AnimationXML.Header.Info);

                // start animation in 1 second.
            timer1.Tag = "A";
            timer1.Enabled = true;
        }

            /// <summary>
            /// If the application is started with the SHIFT key pressed, warnings and errors are reported on a window.
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
            /// Calling this function, all sheeps will execute the same animation (if the sync-word is present in the XML).
            /// </summary>
        public void SyncSheeps()
        {
            AddDebugInfo(DEBUG_TYPE.info, "synchronize sheeps");
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

                    Properties.Settings.Default.Icon = "";
                    Properties.Settings.Default.Images = "";

                    LoadNewXMLFromString("");
                    break;
            }
        }
    }
}

using System;
using System.Windows.Forms;
using DesktopPet.Properties;
using System.Drawing;
using System.IO;
using Windows.System;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopPet
{
        /// <summary>
        /// The only way to interact with the application (not the pet itself) is over the context menu.<br />
        /// This menu is available once you press on the tray icon (near the windows clock).
        /// </summary>
    class ContextMenus : IDisposable
    {
            /// <summary>
            /// Sheep Menu Item: if another pet was downloaded, the icon and text of this item will change.
            /// </summary>
        static ToolStripMenuItem newSheepMenuItem;
            /// <summary>
            /// Close Menu Item: if another pet was downloaded, the text of this item will change.
            /// </summary>
        static ToolStripMenuItem closeSheepMenuItem;
        
        /// <summary>
        /// A value to set in the About dialog: author.
        /// </summary>
        static string author;
            /// <summary>
            /// A value to set in the About dialog: animation version.
            /// </summary>
        static string version;
            /// <summary>
            /// A value to set in the About dialog: animation title.
            /// </summary>
        static string title;
            /// <summary>
            /// A value to set in the About dialog: description and information about the animation.
            /// </summary>
        static string info;

        LocalData.LocalData MyData = new LocalData.LocalData();

        /// <summary>
        /// Creates this instance for the tray icon.
        /// </summary>
        /// <returns>ContextMenuStrip to add in the tray icon.</returns>
        public ContextMenuStrip Create()
        {
                // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;
            ToolStripSeparator sep;
                        
            // Item: New Sheep.
            newSheepMenuItem = new ToolStripMenuItem();
            newSheepMenuItem.Text = "&Add new Sheep";
            newSheepMenuItem.Click += new EventHandler(AddNewSheep_Click);
            newSheepMenuItem.Image = Resources.icon.ToBitmap();
            newSheepMenuItem.Font = new Font(newSheepMenuItem.Font, newSheepMenuItem.Font.Style | FontStyle.Bold);
            menu.Items.Add(newSheepMenuItem);

                // Item: Options.
            item = new ToolStripMenuItem();
            item.Text = "&Options";
            item.Click += new EventHandler(Options_Click);
            item.Image = Resources.option;
            menu.Items.Add(item);

                // Item: Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);
                        
            // Item: About.
            item = new ToolStripMenuItem();
            item.Text = "A&bout";
            item.Click += new EventHandler(About_Click);
            item.Image = Resources.about;
            menu.Items.Add(item);

            // Item: Help.
            item = new ToolStripMenuItem();
            item.Text = "&Help";
            item.Click += new EventHandler(Help_Click);
            item.Image = Resources.help;
            menu.Items.Add(item);

            // Item: Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

                // Item: Close application.
            closeSheepMenuItem = new ToolStripMenuItem();
            closeSheepMenuItem.Text = "&Remove Sheep and Close";
            closeSheepMenuItem.Click += new EventHandler(Exit_Click);
            closeSheepMenuItem.Image = Resources.exit;
            menu.Items.Add(closeSheepMenuItem);

            if(Program.MyData.IsFirstBoot())
            {
                OpenOptionWindow("xamlesheep://options");
            }
            
            return menu;
        }

        private async void OpenOptionWindow(string url)
        {
            Uri uri = new Uri(url);
            await Launcher.LaunchUriAsync(uri);
        }

            /// <summary>
            /// Set a new icon in the context menu with the new pet and updated the info to show in the about dialog.<br />
            /// This function is called every time a new pet is loaded.
            /// </summary>
            /// <param name="newIcon">Icon with the new image.</param>
            /// <param name="petName">Name of the pet, to show in the context menu.</param>
            /// <param name="aboutAuthor">Name of the author.</param>
            /// <param name="aboutTitle">Title of the animation.</param>
            /// <param name="aboutVersion">Version of the animation.</param>
            /// <param name="aboutInfo">About the animation (copyright and author information)</param>
        static public void UpdateIcon(Icon newIcon, string petName, string aboutAuthor, string aboutTitle, string aboutVersion, string aboutInfo)
        {
            newSheepMenuItem.Text = "&Add new " + petName;
            newSheepMenuItem.Image = newIcon.ToBitmap();
            closeSheepMenuItem.Text = "&Remove " + petName + " and Close";

            author = aboutAuthor;
            title = aboutTitle;
            version = aboutVersion;
            info = aboutInfo;
        }

            /// <summary>
            /// Handles the Click event of the Explorer control: add another pet in the desktop.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void AddNewSheep_Click(object sender, EventArgs e)
        {
            //Process.Start("explorer", null);
            Program.Mainthread.AddSheep();
        }
        
        /// <summary>
        /// Handles the Click event of the About control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void About_Click(object sender, EventArgs e)
        {
            OpenOptionWindow("xamlesheep://about");
        }

        /// <summary>
        /// Handles the Click event of the Help control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Help_Click(object sender, EventArgs e)
        {
            //FormHelp help = new FormHelp();
            //help.Show();
            OpenOptionWindow("xamlesheep://help");
        }

        /// <summary>
        /// Handles the Click event of the Option control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Options_Click(object sender, EventArgs e)
        {
            OpenOptionWindow("xamlesheep://options");
        }

            /// <summary>
            /// Processes a menu item. Will close the application after closing all pets.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            //Application.Exit();
            Program.Mainthread.KillSheeps(true);
        }


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
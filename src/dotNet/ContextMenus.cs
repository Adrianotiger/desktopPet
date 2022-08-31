using System;
using System.Windows.Forms;
using DesktopPet.Properties;
using System.Drawing;
using System.IO;
#if !PORTABLE
using Windows.System;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
#endif
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

#if PORTABLE
        /// <summary>
        /// Install functions and form.
        /// </summary>
        Install installForm;
#endif

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

#if PORTABLE
        LocalData MyData = new LocalData();
        bool isAboutLoaded = false;
        bool isOptionLoaded = false;
#else
        LocalData.LocalData MyData = new LocalData.LocalData(Windows.Storage.ApplicationData.Current.LocalFolder.Path, Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\eSheep.exe");
#endif

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

#if PORTABLE
            // Create install class
            installForm = new Install();

            // Item: Install.
            item = new ToolStripMenuItem();
            if (!Program.IsApplicationInstalled())
                item.Text = "&Install application...";
            else
                item.Text = "Repair/&Uninstall...";
            item.Click += new EventHandler(InstallApplication);
            item.Image = installForm.Icon.ToBitmap();
            menu.Items.Add(item);
#endif

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
#if !PORTABLE
        private async void OpenOptionWindow(string url)
        {

            Uri uri = new Uri(url);
            await Launcher.LaunchUriAsync(uri);

        }
#else
        private void OpenOptionWindow(string url) { }
#endif

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


#if PORTABLE
        /// <summary>
        /// Open the window form to install this application on the computer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void InstallApplication(object sender, EventArgs e)
        {
            installForm.ShowInstallation();
        }
#endif

        /// <summary>
        /// Handles the Click event of the About control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void About_Click(object sender, EventArgs e)
        {
#if PORTABLE
            if (isOptionLoaded)
            {

            }
            else if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                AboutBox box = new AboutBox();
                box.FillData(author, title, version, info);
                box.ShowDialog();
                isAboutLoaded = false;
            }
#else
            OpenOptionWindow("xamlesheep://about");
#endif
        }

        /// <summary>
        /// Handles the Click event of the Help control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Help_Click(object sender, EventArgs e)
        {
#if PORTABLE
            FormHelp help = new FormHelp();
            help.Show();
#else
            OpenOptionWindow("xamlesheep://help");
#endif
        }

        /// <summary>
        /// Handles the Click event of the Option control. Open a dialog if no other dialog is still opened.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Options_Click(object sender, EventArgs e)
        {
#if PORTABLE
            if (isAboutLoaded)
            {

            }
            else if (!isOptionLoaded)
            {
                isOptionLoaded = true;
                Program.OpenOptionDialog();
                isOptionLoaded = false;
            }
#else
            OpenOptionWindow("xamlesheep://options");
#endif
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
#if PORTABLE
            if (installForm != null)
            {
                installForm.Dispose();
            }
#endif
        }
    }
}
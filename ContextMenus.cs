using System;
using System.Windows.Forms;
using DesktopPet.Properties;
using System.Drawing;

namespace DesktopPet
{
    /// <summary>
    /// 
    /// </summary>
    class ContextMenus
    {
        /// <summary>
        /// Is the About box displayed?
        /// </summary>
        bool isAboutLoaded = false;
        bool isOptionLoaded = false;

        static ToolStripMenuItem newSheepMenuItem;
        static ToolStripMenuItem closeSheepMenuItem;

        static string author;
        static string version;
        static string title;
        static string info;

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip Create()
        {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;
            ToolStripSeparator sep;
            
            // New Sheep.
            newSheepMenuItem = new ToolStripMenuItem();
            newSheepMenuItem.Text = "&Add new Sheep";
            newSheepMenuItem.Click += new EventHandler(AddNewSheep_Click);
            newSheepMenuItem.Image = Resources.icon.ToBitmap();
            newSheepMenuItem.Font = new Font(newSheepMenuItem.Font, newSheepMenuItem.Font.Style | FontStyle.Bold);
            menu.Items.Add(newSheepMenuItem);

            // Options.
            item = new ToolStripMenuItem();
            item.Text = "&Options";
            item.Click += new EventHandler(Options_Click);
            item.Image = Resources.option;
            menu.Items.Add(item);

            // About.
            item = new ToolStripMenuItem();
            item.Text = "A&bout";
            item.Click += new EventHandler(About_Click);
            item.Image = Resources.about;
            menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            closeSheepMenuItem = new ToolStripMenuItem();
            closeSheepMenuItem.Text = "&Remove Sheep and Close";
            closeSheepMenuItem.Click += new EventHandler(Exit_Click);
            closeSheepMenuItem.Image = Resources.exit;
            menu.Items.Add(closeSheepMenuItem);

            return menu;
        }
        /// <summary>
        /// Set a new icon in the context menu with the new pet
        /// </summary>
        /// <param name="newIcon">Icon with the new image.</param>
        /// <param name="petName">Name of the pet, to show in the contextmenu.</param>
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
        /// Handles the Click event of the Explorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void AddNewSheep_Click(object sender, EventArgs e)
        {
            //Process.Start("explorer", null);
            Program.Mainthread.AddSheep();
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void About_Click(object sender, EventArgs e)
        {
            if(isOptionLoaded)
            {

            }
            else if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                DesktopPet.AboutBox box = new DesktopPet.AboutBox();
                box.FillData(author, title, version, info);
                box.ShowDialog();
                isAboutLoaded = false;
            }
        }

        void Options_Click(object sender, EventArgs e)
        {
            if(isAboutLoaded)
            {

            }
            else if (!isOptionLoaded)
            {
                isOptionLoaded = true;
                Program.Mainthread.OpenOptionDialog();
                isOptionLoaded = false;
            }
        }

        /// <summary>
        /// Processes a menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            //Application.Exit();
            Program.Mainthread.KillSheeps(true);
        }
    }
}
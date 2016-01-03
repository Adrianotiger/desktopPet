using System;
using System.Windows.Forms;
using DesktopPet.Properties;
using System.Drawing;

namespace desktopPet
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

        static ToolStripMenuItem newSheepMenuItem;

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
            item = new ToolStripMenuItem();
            item.Text = "&Remove Sheep and Close";
            item.Click += new EventHandler(Exit_Click);
            item.Image = Resources.exit;
            menu.Items.Add(item);

            return menu;
        }
        /// <summary>
        /// Set a new icon in the context menu with the new pet
        /// </summary>
        /// <param name="newIcon">Icon with the new image.</param>
        static public void UpdateIcon(Icon newIcon)
        {
            newSheepMenuItem.Image = newIcon.ToBitmap();
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
            if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                new DesktopPet.AboutBox().ShowDialog();
                isAboutLoaded = false;
            }
        }

        void Options_Click(object sender, EventArgs e)
        {
            Program.Mainthread.OpenOptionDialog();
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
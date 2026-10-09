using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using static DesktopPet.FormOptions2SurfaceMenu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
#if !PORTABLE
using Windows.ApplicationModel;
#endif

namespace DesktopPet
{
	/// <summary>
	/// This class is a menu item for the FormOptions2SurfaceMenu. It is used to create a menu item with a text, a rectangle, and an offset. 
	/// It can also have a submenu and can be selectable if Surface is set.
	/// </summary>
	public class TMenuItem : Panel
	{
		/// <summary>
		/// If set, this menu is a drop down menu.
		/// </summary>
		public FormOptions2SurfaceSubMenu Surface { get; set; } = null;
		private FormOptions2SurfaceMenu _menu = null;
		/// <summary>
		/// Main Menu. If set, this Panel (text with highlight functionality) will be added automatically to the menu.
		/// </summary>
		public FormOptions2SurfaceMenu Menu { get { return _menu; } set { _menu = value;  value.Controls.Add(this); } }
		/// <summary>
		/// Rect to use for rendering the text and draw the Windows95-like highlight.
		/// </summary>
		public Rectangle Rect { get; private set; }
		string text;
		/// <summary>
		/// First letter of the text. This letter will be underlined to indicate a keyboard shortcut.
		/// </summary>
		public string First { get; private set; }
		/// <summary>
		/// Last String of the text. This is the text that will be displayed in the menu item.
		/// </summary>
		public string Last { get; set; }
		/// <summary>
		/// Offset between the first letter and the last string. This is used to align the text in the menu item.
		/// </summary>
		public int Offset { get; private set; }
		/// <summary>
		/// If the menu is selected. If true, a check mark will be displayed in front of the text. This is used for selectable menu items.
		/// </summary>
		public bool Selected { get; set; } = false;
		/// <summary>
		/// If the menu is selectable. If true, a check mark will be displayed in front of the text when selected. This is used for selectable menu items.
		/// </summary>
		public bool IsSelectable { get; set; } = false;
		/// <summary>
		/// If Highlighted, the menu item will be drawn with a blue background and white text. This is used for mouse hover and keyboard navigation.
		/// </summary>
		public bool Highlighted { get; set; } = false;
		/// <summary>
		/// If not Enabled, the menu item will be drawn with a gray background and dark gray text and does not accept mouse clicks. This is used for disabled menu items.
		/// </summary>
		public bool IsEnabled { get; set; } = true;
		/// <summary>
		/// Menu Index to set, if you want to use the MenuClicked event in FormOptions2. This is used for menu items that are not selectable and do not have a submenu.
		/// </summary>
		public MenuItemIndex MenuIndex { get; set; } = MenuItemIndex.None;
		/// <summary>
		/// If the menu item has a submenu, this list will contain the submenu items. This is used for menu items that have a submenu.
		/// Don't forget to set the Surface property of the submenu items to a new FormOptions2SurfaceSubMenu object, otherwise the submenu will not be displayed.
		/// </summary>
		public List<TMenuItem> Submenu = new List<TMenuItem>();
		/// <summary>
		/// Parent Menu is used to reset the Selected property from other items, once this item is selected. 
		/// </summary>
		public TMenuItem ParentMenu = null;

		/// <summary>
		/// Creates a new menu item with the specified text, rectangle, and offset. The menu item will be added to the specified menu and will be a child of the specified parent menu item.
		/// </summary>
		/// <param name="menu">Parent menu to which the item will be added</param>
		/// <param name="parent">Parent menu item</param>
		/// <param name="Text">Text to display in the menu item</param>
		/// <param name="Rect">Rectangle defining the position and size of the menu item (all items should have the same width)</param>
		/// <param name="OffsetFirst">Offset between the first letter and the last string</param>
		public TMenuItem(FormOptions2SurfaceMenu menu, TMenuItem parent, string Text, Rectangle Rect, int OffsetFirst)
		{
			Menu = menu;
			Dock = DockStyle.None;
			DoubleBuffered = true;

			text = Text;
			First = text.Substring(0, 1);
			Last = text.Substring(1);
			this.Rect = Rect;
			Offset = OffsetFirst;
			Submenu = new List<TMenuItem>();

			Location = new Point(Rect.Left, Rect.Top);
			Size = new Size(Rect.Width, Rect.Height);

			ParentMenu = parent;
			ParentMenu?.Surface?.Controls.Add(this);
		}

		/// <summary>
		/// Mouse entered in the menu item (text). If the menu is activated, the item will be highlighted and the submenu will be displayed if it has one.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (Menu.MenuActivated)
			{
				if (ParentMenu != null)
				{
					// Unhighlight all other items in this submenu and highlight this item
					Menu.UnHighlight(ParentMenu.Surface);
					Highlighted = true;
				}
				else
				{
					// Unhighlight all other items and if a submenu is present, display it
					Menu.UnHighlight();
					Highlighted = true;
					if(Submenu.Count > 0)
					{
						Surface.Visible = true;
						Submenu.ForEach(sm =>
						{
							sm.Highlighted = false;
						});
					}
				}
				Invalidate();
			}
		}

		/// <summary>
		/// Mouse left the menu item (text). If the menu is activated, the item will be unhighlighted and the submenu will be hidden if it has one.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
		}

		/// <summary>
		/// Mouse Click event. If the menu is activated, the item will be highlighted and the submenu will be displayed if it has one. If the item is selectable, it will be selected and the menu will be deactivated.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			// Activate the menu only after the first click.
			if (!Menu.MenuActivated)
			{
				Menu.MenuActivated = true;
				Highlighted = true;
				OnMouseEnter(new EventArgs());
			}

			if(MenuIndex != MenuItemIndex.None)
			{
				if(IsEnabled) Menu.Form.MenuClicked(MenuIndex);

				if(IsSelectable && ParentMenu != null)
				{
					ParentMenu.Submenu.ForEach(sm =>
					{
						sm.Selected = false;
						if (sm == this) sm.Selected = true;
					});
					Menu.UnHighlight();
					Menu.MenuActivated = false;
					Menu.Invalidate();
				}
			}
		}

		/// <summary>
		/// Paint a single text of the menu and a background if higlighted.
		/// If the menu is selectable, a check mark will be displayed in front of the text if it is selected. 
		/// If the menu is not enabled, it will be drawn with a gray background and dark gray text.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			Brush color = Brushes.Black;
			if (Highlighted && Menu.MenuActivated && IsEnabled)
			{
				g.Clear(Menu.Blue.Color);
				color = Brushes.White;

			}
			else if(!IsEnabled)
			{
				g.Clear(Menu.Gray.Color);
				color = Brushes.DarkGray;
			}
			else
			{
				g.Clear(Menu.Gray.Color);
			}
						
			int left = 2;

			if(IsSelectable)
			{
				if (Selected)
					g.DrawString("✓", Menu.FontText, Brushes.Black, 0, 1);
				left += 15;
			}

			g.DrawString(
				First,
				new Font(Menu.FontText, FontStyle.Underline),
				color,
				left,
				1);
			g.DrawString(
				Last,
				Menu.FontText,
				color,
				Offset + left,
				1);
		}
	}

	/// <summary>
	/// This is the Surface for the entire Menu. It is a Panel that contains the menu items and handles the mouse events to highlight the items and display the submenus.
	/// </summary>
	public class FormOptions2SurfaceMenu : Panel
	{
		/// <summary>
		/// The Option2 Form, to forward callbacks and events.
		/// </summary>
		public FormOptions2 Form { get; private set; }

		/// <summary>
		/// Font used fot all menu items.
		/// </summary>
		public Font FontText { get; private set; }

		/// <summary>
		/// Windows 95 Gray color
		/// </summary>
		public Pen Gray { get; private set; }
		/// <summary>
		/// Windows 95 Blue color for highlighted menu items
		/// </summary>
		public Pen Blue { get; private set; }

		/// <summary>
		/// If menu is activated (highlights works only with activated menu).
		/// This is set to true after the first click on a menu item and reset to false after a menu item is selected or the mouse leaves the menu area.
		/// </summary>
		public bool MenuActivated = false;

		private List<TMenuItem> _menus = new List<TMenuItem>();

		/// <summary>
		/// All clickable menu items. They are forwarded to the FormOptions2.MenuClicked event. 
		/// This is used to identify which menu item was clicked and perform the corresponding action.
		/// </summary>
		public enum MenuItemIndex {
			/// <summary>
			/// No Menu action present with this menu
			/// </summary>
			None = 0,
			/// <summary>
			/// Open pets list
			/// </summary>
			Pets = 10,
			/// <summary>
			/// Order View by Date
			/// </summary>
			View_Date = 21,
			/// <summary>
			/// Order View by Author
			/// </summary>
			View_Author = 22,
			/// <summary>
			/// Order View by Name
			/// </summary>
			View_Name = 23,
			/// <summary>
			/// Open Options (in portable)
			/// </summary>
			Options = 30,
			/// <summary>
			/// Open Animation Options
			/// </summary>
			Option_Animation = 31,
			/// <summary>
			/// Open Application Options
			/// </summary>
			Option_Application = 32,
			/// <summary>
			/// Set Autostart (in installable)
			/// </summary>
			Option_Autostart = 39,
			/// <summary>
			/// Open Info page
			/// </summary>
			Help_Info = 41,
			/// <summary>
			/// Open Help page
			/// </summary>
			Help_Help = 42,
			/// <summary>
			/// Close the window
			/// </summary>
			Close = 99
		};

		/// <summary>
		/// Create Menu for the option page.
		/// Currently only PetList-Menu will be created.
		/// </summary>
		/// <param name="formOption">Parent Form</param>
		public FormOptions2SurfaceMenu(FormOptions2 formOption)
		{
			DoubleBuffered = true;
			Form = formOption;
			FontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			Dock = DockStyle.None;

			Gray = new Pen(Color.FromArgb(193, 196, 200), 3);
			Blue = new Pen(Color.FromArgb(13, 6, 164), 2);

		}

		private class TMenuText
		{
			public string Text { get; set; }
			public int Width { get; set; }
			public int LetterOffset { get; set; }
			public bool Selectable { get; set; } = false;
			public bool Selected { get; set; } = false;
			public MenuItemIndex Index { get; set; } = MenuItemIndex.None;
			public List<TMenuText> SubItems { get; set; } = new List<TMenuText>();
			public TMenuItem Menu { get; set; } = null;
		};

		private void GenerateMenu(List<TMenuText> menuBase)
		{
			int offset = 6;
			menuBase.ForEach(mi =>
			{
				var mo = new TMenuItem(this, null, mi.Text, new Rectangle(offset, 0, mi.Width, 24), mi.LetterOffset);
				_menus.Add(mo);
				mi.Menu = _menus[_menus.Count - 1];
				if (mi.Index != MenuItemIndex.None) mo.MenuIndex = mi.Index;
				offset += mo.Width;
			});

			// Submenus
			menuBase.ForEach(mb =>
			{
				if (mb.SubItems.Count > 0)
				{
					var m = mb.Menu;

					m.Surface = new FormOptions2SurfaceSubMenu(this);
					offset = 5;
					int maxWidth = 0;

					mb.SubItems.ForEach(mi =>
					{
						var mo = new TMenuItem(this, m, mi.Text, new Rectangle(5, offset, mi.Width, 22), mi.LetterOffset);
						m.Submenu.Add(mo);
						mi.Menu = m.Submenu[m.Submenu.Count - 1];
						if (mi.Index != MenuItemIndex.None) mo.MenuIndex = mi.Index;
						if (mi.Selectable)
						{
							mo.IsSelectable = true;
							mo.Selected = mi.Selected;
						}
						offset += 20;
						maxWidth = Math.Max(maxWidth, mi.Width);
					});

					m.Surface.Location = new Point(Left + m.Location.X, 30 + m.Location.Y + m.Size.Height);
					m.Surface.Size = new Size(maxWidth + 12, 22 * m.Submenu.Count + 8);
					m.Surface.Visible = false;
					Form.Controls.Add(m.Surface);
				}
			});
		}

		public void GeneratePetsMenu()
		{
			List<TMenuText> menuView = new List<TMenuText>();
			menuView.Add(new TMenuText { Text = "Date (order by)  ", Width = 138, LetterOffset = 10, Selectable = true, Index = MenuItemIndex.View_Date, Selected = true });
			menuView.Add(new TMenuText { Text = "Author (order by)", Width = 138, LetterOffset = 10, Selectable = true, Index = MenuItemIndex.View_Author });
			menuView.Add(new TMenuText { Text = "Name (order by)  ", Width = 138, LetterOffset = 10, Selectable = true, Index = MenuItemIndex.View_Name });

			List<TMenuText> menuHelp = new List<TMenuText>();
			menuHelp.Add(new TMenuText { Text = "Info ", Width = 68, LetterOffset = 3, Index = MenuItemIndex.Help_Info });
			menuHelp.Add(new TMenuText { Text = "Help ", Width = 68, LetterOffset = 10, Index = MenuItemIndex.Help_Help });

			List<TMenuText> menuOptions = new List<TMenuText>();
			menuOptions.Add(new TMenuText { Text = "Pet Options...        ", Width = 168, LetterOffset = 11, Index = MenuItemIndex.Option_Animation });
			menuOptions.Add(new TMenuText { Text = "App Configurations... ", Width = 168, LetterOffset = 11, Index = MenuItemIndex.Option_Application });
			menuOptions.Add(new TMenuText { Text = "More... (Legacy)      ", Width = 168, LetterOffset = 11, Index = MenuItemIndex.Options });
#if !PORTABLE
			// Has to be the last one!
			menuOptions.Add(new TMenuText { Text = "Autostart with Windows", Width = 168, LetterOffset = 10, Selectable = true, Index = MenuItemIndex.Option_Autostart });
#endif

			List<TMenuText> menuBase = new List<TMenuText>();
			menuBase.Add(new TMenuText { Text = "Pets   ", Width = 35, LetterOffset =  9, Index = MenuItemIndex.Pets  });
			menuBase.Add(new TMenuText { Text = "View   ", Width = 39, LetterOffset = 10, SubItems = menuView });
			menuBase.Add(new TMenuText { Text = "Options", Width = 58, LetterOffset = 10, SubItems = menuOptions });
			menuBase.Add(new TMenuText { Text = "Help   ", Width = 40, LetterOffset = 10, SubItems = menuHelp });

			GenerateMenu(menuBase);

#if !PORTABLE
			UpdateAutostartMenuItem(menuOptions[menuOptions.Count - 1].Menu);
#endif
		}

		public void GenerateDefaultMenu()
		{
			List<TMenuText> menuFile = new List<TMenuText>();
			menuFile.Add(new TMenuText { Text = "Close ", Width = 68, LetterOffset = 10, Index = MenuItemIndex.Close });

			List<TMenuText> menuBase = new List<TMenuText>();
			menuBase.Add(new TMenuText { Text = "File  ", Width = 35, LetterOffset = 10, SubItems = menuFile });

			GenerateMenu(menuBase);
		}

#if !PORTABLE
		private async void UpdateAutostartMenuItem(TMenuItem item)
		{
			try
			{
				var tasks = await StartupTask.GetForCurrentPackageAsync();

				foreach (var task in tasks) // there is only 1 task
				{
					//MessageBox.Show(task.ToString() + " - " + task.State.ToString());
					//await task.RequestEnableAsync();
					if(task.State == StartupTaskState.DisabledByUser || 
						task.State == StartupTaskState.DisabledByPolicy || 
						task.State == StartupTaskState.Disabled)
					{
						item.Selected = false;
					}
					else if (task.State == StartupTaskState.Enabled)
					{
						item.IsEnabled = true;
						item.Selected = true;
					}
				}
			}
			catch (Exception ex)
			{
				Form.InfoText = ex.ToString();
				item.IsEnabled = false;
			}
		}
#endif

		/// <summary>
		/// Unhighlight every item in this menu
		/// </summary>
		/// <param name="item">if set, unhighlight all items in this submenu</param>
		public void UnHighlight(FormOptions2SurfaceSubMenu item = null)
		{
			_menus.ForEach(m =>
			{
				if (item == null)
				{
					if (m.Highlighted)
					{
						m.Highlighted = false;
						if (m.Submenu.Count > 0)
							m.Surface.Visible = false;
						m.Invalidate();
					}
				}
				else
				{
					_menus.ForEach(pm =>
					{
						if (pm.Surface == item)
						{
							pm.Submenu.ForEach(sm =>
							{
								if (sm.Highlighted)
								{
									sm.Highlighted = false;
									sm.Invalidate();
								}
							});
						}
					});
				}
			});
		}
		
		/// <summary>
		/// Paint this menu (without items)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			g.Clear(Gray.Color);

		}
	}

	/// <summary>
	/// Submenu is a drop down menu
	/// </summary>
	public class FormOptions2SurfaceSubMenu : Panel
	{
		/// <summary>
		/// Main Menu
		/// </summary>
		public FormOptions2SurfaceMenu Menu { get; private set; }

		/// <summary>
		/// Constructor for the submenu
		/// </summary>
		/// <param name="menu">The main menu</param>
		public FormOptions2SurfaceSubMenu(FormOptions2SurfaceMenu menu)
		{
			Menu = menu;
			DoubleBuffered = true;
			Dock = DockStyle.None;
		}

		/// <summary>
		/// Paint the drop down rectangle
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			var rect = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height);
			g.FillRectangle(Menu.Gray.Brush, rect);
			ControlPaint.DrawBorder(
				g,
				rect,
				Color.LightGray, 3, ButtonBorderStyle.Solid,
				Color.LightGray, 3, ButtonBorderStyle.Solid,
				Color.Black, 3, ButtonBorderStyle.Outset,
				Color.Black, 3, ButtonBorderStyle.Outset);
			ControlPaint.DrawBorder(
				g,
				new Rectangle(rect.X + 1, rect.Y + 2, rect.Width - 3, rect.Height - 3),
				Color.White, 1, ButtonBorderStyle.Outset,
				Color.White, 1, ButtonBorderStyle.Outset,
				Color.Gray, 2, ButtonBorderStyle.Solid,
				Color.Gray, 2, ButtonBorderStyle.Solid);

		}
	}
}

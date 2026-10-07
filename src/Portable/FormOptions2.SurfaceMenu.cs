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
			/// Set Autostart (in instabllable)
			/// </summary>
			Option_Autostart = 32,
			/// <summary>
			/// Open Info page
			/// </summary>
			Help_Info = 41,
			/// <summary>
			/// Open Help page
			/// </summary>
			Help_Help = 42
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

			int offset = 6;
			_menus.Add(new TMenuItem(this, null, "Pets"   , new Rectangle(offset, 0, 35, 24),  9)); offset += _menus[_menus.Count - 1].Width;
			_menus.Add(new TMenuItem(this, null, "View"   , new Rectangle(offset, 0, 39, 24), 10)); offset += _menus[_menus.Count - 1].Width;
			_menus.Add(new TMenuItem(this, null, "Options", new Rectangle(offset, 0, 58, 24), 10)); offset += _menus[_menus.Count - 1].Width;
			_menus.Add(new TMenuItem(this, null, "Help"   , new Rectangle(offset, 0, 40, 24), 10)); offset += _menus[_menus.Count - 1].Width;

			_menus[0].MenuIndex = MenuItemIndex.Pets;

			_menus[1].Surface = new FormOptions2SurfaceSubMenu(this);
			_menus[1].Submenu.Add(new TMenuItem(this, _menus[1], "Date (order by)", new Rectangle(5, 5, 138, 22), 10));
			_menus[1].Submenu.Add(new TMenuItem(this, _menus[1], "Author (order by)", new Rectangle(5, 25, 138, 22), 10));
			_menus[1].Submenu.Add(new TMenuItem(this, _menus[1], "Name (order by)", new Rectangle(5, 45, 138, 22), 10));
			_menus[1].Submenu[0].Selected = true;
			_menus[1].Submenu[0].IsSelectable = true;
			_menus[1].Submenu[0].MenuIndex = MenuItemIndex.View_Date;
			_menus[1].Submenu[1].IsSelectable = true;
			_menus[1].Submenu[1].MenuIndex = MenuItemIndex.View_Author;
			_menus[1].Submenu[2].IsSelectable = true;
			_menus[1].Submenu[2].MenuIndex = MenuItemIndex.View_Name;
			_menus[1].Surface.Location = new Point(Left + _menus[1].Location.X, 30 + _menus[1].Location.Y + _menus[1].Size.Height);
			_menus[1].Surface.Size = new Size(150, 8 + 22 * _menus[1].Submenu.Count);
			_menus[1].Surface.Visible = false;
			Form.Controls.Add(_menus[1].Surface);

#if PORTABLE
			_menus[2].MenuIndex = MenuItemIndex.Options;
#else
			_menus[2].Surface = new FormOptions2SurfaceSubMenu(this);
			_menus[2].Submenu.Add(new TMenuItem(this, _menus[2], "More...", new Rectangle(5, 5, 168, 22), 11));
			_menus[2].Submenu.Add(new TMenuItem(this, _menus[2], "Autostart with Windows", new Rectangle(5, 25, 168, 22), 10));
			_menus[2].Surface.Location = new Point(Left + _menus[2].Location.X, 30 + _menus[2].Location.Y + _menus[2].Size.Height);
			_menus[2].Surface.Size = new Size(180, 8 + 22 * _menus[2].Submenu.Count);
			_menus[2].Surface.Visible = false;
			Form.Controls.Add(_menus[2].Surface);

			_menus[2].Submenu[1].IsSelectable = true;
			_menus[2].Submenu[1].MenuIndex = MenuItemIndex.Option_Autostart;
			_menus[2].Submenu[0].MenuIndex = MenuItemIndex.Options;
			UpdateAutostartMenuItem();
#endif

			_menus[3].Surface = new FormOptions2SurfaceSubMenu(this);
			_menus[3].Submenu.Add(new TMenuItem(this, _menus[3], "Info", new Rectangle(5, 5, 68, 22), 3));
			_menus[3].Submenu.Add(new TMenuItem(this, _menus[3], "Help", new Rectangle(5, 25, 68, 22), 10));
			_menus[3].Submenu[0].MenuIndex = MenuItemIndex.Help_Info;
			_menus[3].Submenu[1].MenuIndex = MenuItemIndex.Help_Help;
			_menus[3].Surface.Location = new Point(Left + _menus[3].Location.X, 30 + _menus[3].Location.Y + _menus[3].Size.Height);
			_menus[3].Surface.Size = new Size(80, 8 + 22 * _menus[3].Submenu.Count);
			_menus[3].Surface.Visible = false;
			Form.Controls.Add(_menus[3].Surface);


			Gray = new Pen(Color.FromArgb(193, 196, 200), 3);
			Blue = new Pen(Color.FromArgb(13, 6, 164), 2);
		}

#if !PORTABLE
		private async void UpdateAutostartMenuItem()
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
						_menus[2].Submenu[1].Selected = false;
					}
					else if (task.State == StartupTaskState.Enabled)
					{
						_menus[2].Submenu[1].IsEnabled = true;
						_menus[2].Submenu[1].Selected = true;
					}
				}
			}
			catch (Exception ex)
			{
				Form.InfoText = ex.ToString();
				_menus[2].Submenu[1].IsEnabled = false;
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

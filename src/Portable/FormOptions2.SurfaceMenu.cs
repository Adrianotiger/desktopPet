using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DesktopPet.FormOptions2SurfaceMenu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DesktopPet
{
	public class TMenuItem : Panel
	{
		public FormOptions2SurfaceSubMenu Surface { get; set; } = null;
		private FormOptions2SurfaceMenu _menu = null;
		public FormOptions2SurfaceMenu Menu { get { return _menu; } set { _menu = value;  value.Controls.Add(this); } }
		
		public Rectangle Rect { get; private set; }
		string text;
		public string First { get; private set; }
		public string Last { get; private set; }
		public bool Selected { get; set; } = false;
		public bool IsSelectable { get; set; } = false;
		public bool Highlighted { get; set; } = false;
		public int Offset { get; private set; }
		public MenuItemIndex MenuIndex { get; set; } = MenuItemIndex.None;
		public List<TMenuItem> Submenu = new List<TMenuItem>();

		public TMenuItem ParentMenu = null;

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

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (Menu.MenuActivated)
			{
				if (ParentMenu != null)
				{
					Menu.UnHighlight(ParentMenu.Surface);
					Highlighted = true;
				}
				else
				{
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

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!Menu.MenuActivated)
			{
				Menu.MenuActivated = true;
				Highlighted = true;
				OnMouseEnter(new EventArgs());
			}

			if(MenuIndex != MenuItemIndex.None)
			{
				Menu.Form.MenuClicked(MenuIndex);

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

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;


			Brush color = Brushes.Black;
			if (Highlighted && Menu.MenuActivated)
			{
				g.Clear(Menu.Blue.Color);
				color = Brushes.White;

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

	public class FormOptions2SurfaceMenu : Panel
	{
		public FormOptions2 Form { get; private set; }

		public Font FontText { get; private set; }

		public Pen Gray { get; private set; }
		public Pen Blue { get; private set; }

		public bool MenuActivated = false;

		private List<TMenuItem> _menus = new List<TMenuItem>();

		public enum MenuItemIndex {
			None = 0,
			Pets = 10,
			View_Date = 21,
			View_Author = 22,
			View_Name = 23,
			Options = 30,
			Help_Info = 41,
			Help_Help = 42
		};

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

			_menus[2].MenuIndex = MenuItemIndex.Options;

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
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			g.Clear(Gray.Color);

		}
	}

	public class FormOptions2SurfaceSubMenu : Panel
	{
		private Font fontText;
		public FormOptions2SurfaceMenu Menu { get; private set; }

		public FormOptions2SurfaceSubMenu(FormOptions2SurfaceMenu menu)
		{
			Menu = menu;
			DoubleBuffered = true;
			fontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			Dock = DockStyle.None;
		}

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

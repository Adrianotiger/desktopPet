using DesktopPet.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using XmlData;
using static DesktopPet.FormOptions2;
using static DesktopPet.FormOptions2SurfaceMenu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;



#if !PORTABLE
using Windows.ApplicationModel;
#endif

namespace DesktopPet
{
	/// <summary>
	/// New Option Form, with a nice visual and Windows95 style.
	/// It replace the new Options and will call the old Option Form for all options that are not yet implemented in this one.
	/// </summary>
	public partial class FormOptions2 : Form
	{
		private Font fontTitle;
		private Font fontText;
		private Font fontDetail;
		private Icon iconImage;
		private Image win98;
		private Rectangle _closeButtonRect;
		private Rectangle _resizeRect;
		private Rectangle _titleRect;
		private Rectangle _okRect;
		private Rectangle _cancelRect;
		private Point _moves;
		private Point _resizes;
		private Point _moveStart;
		private Pen Gray;
		private Pen Blue;
		private Pen White;
		private Pen Black;
		private int pageSelected = 0;
		private Panel extendedPanel;
		private FormOptions2SurfaceMenu surfaceMenu;
		private bool isDialog = false;
		/// <summary>
		/// List of pets available to download from the web. This is updated every 24 hours, and stored in a temporary folder.
		/// </summary>
		public OptionsPets WebPets;
		/// <summary>
		/// Infotext used to show some debug info or status on the status bar.
		/// </summary>
		public string InfoText = "Options";
		public ColorMatrix IconHighlightMatrix { get; private set; }

		/// <summary>
		/// FormOptions2 can be called with different window types. 
		/// Depending on the window type, menubar and content will be different.
		/// </summary>
		public enum WindowType
		{
			/// <summary>
			/// Show Pets list
			/// </summary>
			Pets = 1,
			/// <summary>
			/// Show all Options like the old Control Panel
			/// </summary>
			AppAnimationOptions = 2,
			AppConfiguration = 3,
			Help = 4,
			Info = 5,
			/// <summary>
			/// Show a Dialog Box with OK and Cancel button
			/// </summary>
			Dialog = 6
		};

		private class TControl
		{
			public Rectangle Rect = new Rectangle(0,0,100,0);
			public bool IsCheckbox = true;
			public int Value = 0;
			public List<int> Values;
			public List<string> ValuesText;
			public int Max = -1;
		};
		private List<TControl> controls = new List<TControl>();

		private class TWindowsLeftInfo
		{
			public Image Image { get; set; } = null;
			public string Title { get; set; } = "Mates";
			public string Details { get; set; } = "Select a mate to\nview its description.";
		};
		private TWindowsLeftInfo WindowsLeftInfo = new TWindowsLeftInfo();

		/// <summary>
		/// Constructor for FormOptions2. Create a windows95-style window to show a list of pets, info or options.
		/// </summary>
		/// <param name="Title">The text to display as title</param>
		/// <param name="winType">The type of window to display</param>
		public FormOptions2(string Title, WindowType winType)
		{
			InitializeComponent();
			Text = Title;

			//this.TopMost = true;
			fontTitle = new Font(Font.FontFamily, 15, FontStyle.Bold, GraphicsUnit.Pixel);
			fontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			fontDetail = new Font(Font.FontFamily, 12, FontStyle.Regular, GraphicsUnit.Pixel);
			iconImage = new Icon(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location), 32, 32);
			win98 = Resources.wvleft;
			_moves = new Point(-1, -1);
			_resizes = new Point(-1, -1);

			// Some windows95 colors
			Gray = new Pen(Color.FromArgb(193, 196, 200), 3);
			Blue = new Pen(Color.FromArgb(13, 6, 164), 2);
			White = new Pen(Color.FromArgb(255, 255, 255), 1);
			Black = new Pen(Color.FromArgb(150, 150, 150), 1);

			// Yellow Highlight
			IconHighlightMatrix = new ColorMatrix(new float[][]
			{
				new float[] { 1, 0.5f, 0, 0, 0 }, // Red
				new float[] { 1, 0.5f, 0, 0, 0 }, // Green
				new float[] { 1, 0, 0, 0, 0 }, // Blue
				new float[] { 0, 0, 0, 1, 0 }, // Alpha
				new float[] { 0, 0, 0, 0, 1 }
			});

			surfaceMenu = new FormOptions2SurfaceMenu(this);

			switch (winType)
			{
				case WindowType.AppAnimationOptions:
					extendedPanel = new FormOptions2SurfaceOptions(this);
					(extendedPanel as FormOptions2SurfaceOptions).GenerateAnimationOptionSurface();
					surfaceMenu.GenerateDefaultMenu();
					Width = 500;
					Height = 400;
					break;
				case WindowType.AppConfiguration:
					extendedPanel = new FormOptions2SurfaceOptions(this);
					(extendedPanel as FormOptions2SurfaceOptions).GenerateConfigurationSurface();
					surfaceMenu.GenerateDefaultMenu();
					Width = 500;
					Height = 400;
					break;
				case WindowType.Dialog:
					Width = 300;
					Height = 200;
					surfaceMenu = null;
					isDialog = true;
					break;
				default:
					extendedPanel = new FormOptions2SurfacePets(this);
					surfaceMenu.GeneratePetsMenu();
					
					Task.Run(async () => { await UpdatePets(); });
					break;
			}
			
			RecalculateSizes();
			if(extendedPanel != null) Controls.Add(extendedPanel);
			if(surfaceMenu != null) Controls.Add(surfaceMenu);
		}

		/// <summary>
		/// Resizing window
		/// </summary>
		private void RecalculateSizes()
		{
			_closeButtonRect = new Rectangle(Width - 28, 8, 20, 20);
			_resizeRect = new Rectangle(Width - 28, Height - 28, 28, 28);
			_titleRect = new Rectangle(6, 6, Width - 12, 24);

			if (extendedPanel != null)
			{
				extendedPanel.Location = new Point(150, 60);
				extendedPanel.Size = new Size(
						ClientRectangle.Width - 160,
						ClientRectangle.Height - 98);
			}

			if (surfaceMenu != null)
			{
				surfaceMenu.Location = new Point(6, 32);
				surfaceMenu.Size = new Size(
						Width - 30,
						24);
			}

			if(isDialog)
			{
				int y = 40;
				int extraHeight = 0;
				controls.ForEach(c =>
				{
					c.Rect = new Rectangle(10, y, ClientRectangle.Width - 20, 50);
					if (c.Values != null) extraHeight = Math.Max(extraHeight, y + c.Values.Count * 22 + 50 + 30);
					y += 50;
				});
				Height = y + 30;

				if (Height < extraHeight) Height = extraHeight;

				_okRect = new Rectangle(15/*+ClientRectangle.Width/2*/, ClientRectangle.Height - 40, ClientRectangle.Width / 2 - 30, 30);
				_cancelRect = new Rectangle(15 + ClientRectangle.Width / 2, ClientRectangle.Height - 40, ClientRectangle.Width / 2 - 30, 30);
			}
		}

		public void AddDialogControl(bool enabled)
		{
			controls.Add(new TControl
			{
				Value = (enabled ? 1 : 0)
			});
			RecalculateSizes();
		}

		public void AddDialogControl(int value, int maxValue)
		{
			controls.Add(new TControl
			{
				Value = value,
				Max = maxValue,
				IsCheckbox = false
			});
			RecalculateSizes();
		}

		public void AddDialogControl(int value, List<int> values, List<string> valuesText)
		{
			controls.Add(new TControl
			{
				Value = value,
				Values = values,
				ValuesText = valuesText,
				IsCheckbox = false,
				Max = -1 /* use Max=-1 for close and Max=-2 for open*/
			});
			RecalculateSizes();
		}

		public int GetDialogControl(int index = 0)
		{
			return controls[index].Value;
		}

		/// <summary>
		/// Handle:
		/// - Moving window
		/// - Resizing window
		/// - Closing window
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (_closeButtonRect.Contains(e.Location))
			{ 
				if(isDialog) this.DialogResult = DialogResult.Cancel;
				Close();
			}
			else if(_titleRect.Contains(e.Location) && _moves.X < 0)
			{
				_moves = new Point(e.Location.X, e.Location.Y);
				_moveStart = new Point(Left, Top);
			}
			else if (isDialog)
			{
				if (_okRect.Contains(e.Location)) { this.DialogResult = DialogResult.OK; return; }
				else if (_cancelRect.Contains(e.Location)) { this.DialogResult = DialogResult.Cancel; return;  }
				else
				{
					controls.ForEach(c =>
					{
						if(c.Max == -2) // open select dialog
						{
							var rect = new Rectangle(c.Rect.X + 20, c.Rect.Y + 30, c.Rect.Width - 40, 24 * c.Values.Count);
							if (rect.Contains(e.Location))
							{
								int index = (int)(((double)(e.Location.Y - rect.Y - 15) / c.Rect.Height) * c.Values.Count);
								c.Value = c.Values[Math.Min(index, c.Values.Count - 1)];
								c.Max = -1;
								Invalidate();
								return;
							}
						}
						if(c.Rect.Contains(e.Location))
						{
							if (c.IsCheckbox)
							{
								c.Value = (c.Value == 0 ? 1 : 0);
							}
							else if(c.Values != null)
							{
								if (c.Max == -1) // closed
								{
									c.Max = -2;
								}
								else
								{
									c.Max = -1;
								}
							}

							Invalidate();
						}
					});
				}
			}
			else if(_resizeRect.Contains(e.Location))
			{
				_resizes = new Point(e.Location.X, e.Location.Y);
				_moveStart = new Point(Width, Height);
			}
			else
			{
				
			}

			if (surfaceMenu != null && surfaceMenu.MenuActivated) { surfaceMenu.MenuActivated = false; surfaceMenu.UnHighlight(); surfaceMenu.Invalidate(); }
		}

		/// <summary>
		/// Called from menu, handles the menu click and performs the action for the selected menu item.
		/// </summary>
		/// <param name="menu"></param>
		public async void MenuClicked(MenuItemIndex menu)
		{
			switch(menu)
			{
				case MenuItemIndex.None:
					{
						if (surfaceMenu.MenuActivated) { surfaceMenu.MenuActivated = false; surfaceMenu.UnHighlight(); surfaceMenu.Invalidate(); }
						break;
					}
				case MenuItemIndex.Pets: // Pets
					{
						var petsSurface = (extendedPanel as FormOptions2SurfacePets);
						if (petsSurface != null && petsSurface.showReadme)
						{
							petsSurface.showReadme = false;
							petsSurface.showDetails = false;
							petsSurface.Invalidate();

							//if (surfaceMenu.MenuActivated) { surfaceMenu.MenuActivated = false; surfaceMenu.UnHighlight(); surfaceMenu.Invalidate(); }
						}
						break;
					}
				case MenuItemIndex.Close: // Close Window
					{
						Close();
						break;
					}
				case MenuItemIndex.View_Author: // View - Author
				case MenuItemIndex.View_Date: // View - Date
				case MenuItemIndex.View_Name: // View - Name
					{
						var petsSurface = (extendedPanel as FormOptions2SurfacePets);

						WebPets.Reorder(menu);
						if (petsSurface != null && !petsSurface.showReadme) petsSurface.Invalidate();
						break;
					}
				case MenuItemIndex.Options: // Options
					{
						FormOptions formoptions = new FormOptions();
						switch (formoptions.ShowDialog())
						{
							case DialogResult.Retry:
								StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "restoring default XML");

								Program.MyData.SetIcon("");
								Program.MyData.SetImages("");
								Program.MyData.SetXml("", "");
								break;
						}
						break;
					}
				case MenuItemIndex.Option_Animation:
					{
						FormOptions2 formoptions = new FormOptions2("Animations Options", WindowType.AppAnimationOptions);
						switch (formoptions.ShowDialog())
						{
							case DialogResult.Retry:
								StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "restoring default XML");

								Program.MyData.SetIcon("");
								Program.MyData.SetImages("");
								Program.MyData.SetXml("", "");

								Program.RestartApp();
								break;
						}
						break;
					}
				case MenuItemIndex.Option_Application:
					{
						FormOptions2 formoptions = new FormOptions2("Application Configurations", WindowType.AppConfiguration);
						switch (formoptions.ShowDialog())
						{
							case DialogResult.Retry:
								break;
						}
						break;
					}
				case MenuItemIndex.Option_Autostart:
					{
#if !PORTABLE
						try
						{
							var tasks = await StartupTask.GetForCurrentPackageAsync();

							foreach (var task2 in tasks)
							{
								if(task2.State == StartupTaskState.Enabled)
								{
									// Only user can disable it.
									Process.Start(new ProcessStartInfo
									{
										FileName = "ms-settings:startupapps",
										UseShellExecute = true
									});
								}
								else
								{
									await task2.RequestEnableAsync();
								}
								break; // only one task should be enabled
							}
						}
						catch (Exception ex)
						{
							InfoText = "Error: " + ex.Message;
						}
#endif
						break;
					}
				case MenuItemIndex.Help_Help:
					{
						var formhelp = new FormHelp();
						formhelp.Show();
						break;
					}
				case MenuItemIndex.Help_Info:
					{
						var forminfo = new AboutBox();

						var xml = new Xml();
						if (xml.ReadXML())
						{
							forminfo.FillData(xml.AnimationXML.Header.Author, xml.AnimationXML.Header.Title, xml.AnimationXML.Header.Version, xml.AnimationXML.Header.Info);
							forminfo.ShowDialog();
						}
						break;
					}
			}
		}

		/// <summary>
		/// End of moving or resizing window, reset the move and resize points.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if(_moves.X >= 0)
			{
				_moves = new Point(-1, -1);
			}
			if (_resizes.X >= 0)
			{
				_resizes = new Point(-1, -1);
			}
		}

		/// <summary>
		/// Move or resize window, depending on where the mouse is located. Change cursor to indicate what action is possible.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (_closeButtonRect.Contains(e.Location))
				Cursor = Cursors.Hand;
			else if (_resizeRect.Contains(e.Location))
				Cursor = Cursors.SizeNWSE;
			else
				Cursor = Cursors.Default;

			if (_moves.X >= 0)
			{
				Left = _moveStart.X + (e.Location.X - _moves.X);
				Top = _moveStart.Y + (e.Location.Y - _moves.Y);
				_moveStart = new Point(Left, Top);
			}
			else if(_resizes.X >= 0)
			{
				Width = _resizes.X + (e.Location.X - _resizes.X);
				Height = _resizes.Y + (e.Location.Y - _resizes.Y);
				if (Width < 400) Width = 400;
				if (Height < 350) Height = 350;
				_moveStart = new Point(Width, Height);
				RecalculateSizes();
				Invalidate();
			}
			
			if(isDialog && e.Button == MouseButtons.Left)
			{
				var c = controls[0];
				if (c.Rect.Contains(e.Location))
				{
					if (c.Max > 0)
					{
						int min = c.Rect.X + 10 + 24;
						int max = c.Rect.Width - 20;
						//int val = (max - min) / (c.Max + 1) * c.Value;

						if (e.Location.X < min)
						{
							if (c.Value > 0) c.Value--;
						}
						else if (e.Location.X > max)
						{
							if (c.Value < c.Max) c.Value++;
						}
						else
						{
							int oldValue = c.Value;
							c.Value = (int)((double)c.Max / (max - min) * (e.Location.X - min));
							if (oldValue != c.Value) Invalidate();
						}
					}
				}
			}
		}

		public void SetLeftInfo(Image image, string title, string details)
		{
			WindowsLeftInfo.Image = image;
			WindowsLeftInfo.Title = title;
			WindowsLeftInfo.Details = details;
			Invalidate();
		}

		/// <summary>
		/// Paint the window, not the content. The content is painted by the surface controls.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			// Background
			g.Clear(Gray.Color);

			// Border
			ControlPaint.DrawBorder(
				g,
				ClientRectangle,
				Color.LightGray, 3, ButtonBorderStyle.Solid,
				Color.LightGray, 3, ButtonBorderStyle.Solid,
				Color.Black, 3, ButtonBorderStyle.Outset,
				Color.Black, 3, ButtonBorderStyle.Outset);

			ControlPaint.DrawBorder(
				g,
				new Rectangle(2, 2, ClientRectangle.Width - 3, ClientRectangle.Height - 3),
				Color.White, 1, ButtonBorderStyle.Outset,
				Color.White, 1, ButtonBorderStyle.Outset,
				Color.Gray, 2, ButtonBorderStyle.Solid,
				Color.Gray, 2, ButtonBorderStyle.Solid);

			// Title
			g.FillRectangle(
				Blue.Brush,
				_titleRect);

			g.DrawIcon(iconImage, new Rectangle(10, 6, 24, 24));

			g.DrawString(
				Text,
				fontTitle,
				Brushes.White,
				34,
				8);

			ControlPaint.DrawButton(
				g,
				_closeButtonRect,
				ButtonState.Normal);

			// draw X
			using (var pen = new Pen(Color.Black, 2))
			{
				g.DrawLine(pen,
				_closeButtonRect.Left + 4,
				_closeButtonRect.Top + 4,
				_closeButtonRect.Right - 6,
				_closeButtonRect.Bottom - 6);

				g.DrawLine(pen,
				_closeButtonRect.Right - 6,
				_closeButtonRect.Top + 4,
				_closeButtonRect.Left + 4,
				_closeButtonRect.Bottom - 6);
			}

			// Contents
			if(isDialog)
			{
				ControlPaint.DrawButton(g, new Rectangle(15/*+ClientRectangle.Width/2*/, ClientRectangle.Height - 40, ClientRectangle.Width / 2 - 30, 30), ButtonState.Normal);
				ControlPaint.DrawButton(g, new Rectangle(15 + ClientRectangle.Width / 2, ClientRectangle.Height - 40, ClientRectangle.Width / 2 - 30, 30), ButtonState.Normal);
				g.DrawString("OK", fontText, Brushes.Black, ClientRectangle.Width / 4 - 10, ClientRectangle.Height - 35);
				g.DrawString("Cancel", fontText, Brushes.Black, ClientRectangle.Width / 4 * 3 - 25, ClientRectangle.Height - 35);
				controls.ForEach(c =>
				{
					if(c.IsCheckbox)
					{
						ControlPaint.DrawCheckBox(g, new Rectangle(c.Rect.X + 20, c.Rect.Y, 24, 24), c.Value == 1 ? ButtonState.Checked : ButtonState.Normal);
						g.DrawString("Enable", fontText, Brushes.Black, c.Rect.X + 50, c.Rect.Y + 5);
					}
					else if(c.Max > 0)
					{
						ControlPaint.DrawBorder(g, new Rectangle(c.Rect.X + 20, c.Rect.Y + 10, c.Rect.Width - 40, 3), Color.Black, ButtonBorderStyle.Outset);
						ControlPaint.DrawScrollButton(g, c.Rect.X + 10, c.Rect.Y, 24, 24, ScrollButton.Left, ButtonState.Normal);
						ControlPaint.DrawScrollButton(g, c.Rect.Width - 20, c.Rect.Y, 24, 24, ScrollButton.Right, ButtonState.Normal);
						int min = c.Rect.X + 10 + 24;
						int max = c.Rect.Width - 20;
						int val = (max - min) / (c.Max + 1) * c.Value;
						ControlPaint.DrawButton(g, new Rectangle(min + val, c.Rect.Y, 20, 24), ButtonState.Normal);

						g.DrawString(
							Text + " [" + c.Value + "]",
							fontTitle,
							Brushes.White,
							34,
							8);
					}
					else if(c.Values.Count > 0)
					{
						ControlPaint.DrawBorder(g, new Rectangle(c.Rect.X + 20, c.Rect.Y + 10, c.Rect.Width - 40, 24), SystemColors.WindowFrame, ButtonBorderStyle.Solid);
						g.FillRectangle(Brushes.White, c.Rect.X + 20 + 1, c.Rect.Y + 10 + 1, c.Rect.Width - 42, 24 - 2);
						ControlPaint.DrawComboButton(g, new Rectangle(c.Rect.Width - 27, c.Rect.Y + 11, 17, 22), ButtonState.Normal);
						int index = c.Values.IndexOf(c.Value);
						g.DrawString(c.ValuesText[index], new Font(fontText, FontStyle.Bold), Brushes.Black, c.Rect.X + 30, c.Rect.Y + 13);

						if(c.Max == -2) // menu open
						{
							ControlPaint.DrawBorder(g, new Rectangle(c.Rect.X + 20, c.Rect.Y + 10 + 24, c.Rect.Width - 60, 24 * c.Values.Count), SystemColors.WindowFrame, ButtonBorderStyle.Solid);
							g.FillRectangle(Brushes.White, c.Rect.X + 20 + 1, c.Rect.Y + 10 + 24 + 1, c.Rect.Width - 62, 24 * c.Values.Count - 2);
							for(int k=0;k<c.Values.Count;k++)
							{
								g.DrawString(c.ValuesText[k], new Font(fontText, FontStyle.Bold), Brushes.Black, c.Rect.X + 30, c.Rect.Y + 13 + 24 + k*24);
							}
						}
					}
				});
				return;
			}
			if (pageSelected == 0)
			{
				g.FillRectangle(new SolidBrush(Color.White), new Rectangle(8, 58, ClientRectangle.Width - 15, ClientRectangle.Height - 91));
				g.DrawImage(win98, new Point(8, 58));

				var sizeF = g.MeasureString(WindowsLeftInfo.Title,
						new Font(fontText.FontFamily, WindowsLeftInfo.Title.Length < 7 ? 24 : 16, FontStyle.Bold),
						new SizeF(130, 80));
				g.DrawString(
					WindowsLeftInfo.Title,
					new Font(fontText.FontFamily, WindowsLeftInfo.Title.Length < 6 ? 24 : 16, FontStyle.Bold),
					Brushes.Black,
					new RectangleF(20, 140 - (int)(sizeF.Height / 3), 130, 80)
					);

				if(WindowsLeftInfo.Image != null)
					g.DrawImage(WindowsLeftInfo.Image, new Rectangle(50, 90 - (int)(sizeF.Height / 3), 48, 48));

				g.DrawString(
					WindowsLeftInfo.Details,
					fontDetail,
					Brushes.Black,
					new Rectangle(20, 190, 130, 300)
					);
			}

			// Content Border
			ControlPaint.DrawBorder(
				g,
				new Rectangle(8, 58, ClientRectangle.Width - 17, ClientRectangle.Height - 92),
				Color.Black, 2, ButtonBorderStyle.Outset,
				Color.Black, 2, ButtonBorderStyle.Outset,
				Color.Gray, 2, ButtonBorderStyle.Inset,
				Color.Gray, 2, ButtonBorderStyle.Inset);

			// Bottom
			ControlPaint.DrawBorder(
				g,
				new Rectangle(6, ClientRectangle.Height - 32, ClientRectangle.Width / 2 - 8, 26),
				Color.Gray, 3, ButtonBorderStyle.Outset,
				Color.Gray, 3, ButtonBorderStyle.Outset,
				Color.White, 1, ButtonBorderStyle.Inset,
				Color.White, 1, ButtonBorderStyle.Inset);

			g.DrawString(InfoText, fontDetail, Brushes.Black, 12, ClientRectangle.Height - 27);

			ControlPaint.DrawBorder(
				g,
				new Rectangle(ClientRectangle.Width / 2, ClientRectangle.Height - 32, ClientRectangle.Width / 2 - 8, 26),
				Color.Gray, 3, ButtonBorderStyle.Outset,
				Color.Gray, 3, ButtonBorderStyle.Outset,
				Color.White, 1, ButtonBorderStyle.Inset,
				Color.White, 1, ButtonBorderStyle.Inset);

			// Resize
			g.DrawLine(Gray, new Point(ClientRectangle.Width - 11, ClientRectangle.Height - 6), new Point(ClientRectangle.Width - 8, ClientRectangle.Height -  9));
			g.DrawLine(Gray, new Point(ClientRectangle.Width - 17, ClientRectangle.Height - 6), new Point(ClientRectangle.Width - 8, ClientRectangle.Height - 15));
			g.DrawLine(Gray, new Point(ClientRectangle.Width - 23, ClientRectangle.Height - 6), new Point(ClientRectangle.Width - 8, ClientRectangle.Height - 21));

			g.DrawLine(Black, new Point(ClientRectangle.Width - 13, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 11));
			g.DrawLine(White, new Point(ClientRectangle.Width - 15, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 13));
						
			g.DrawLine(Black, new Point(ClientRectangle.Width - 19, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 17));
			g.DrawLine(White, new Point(ClientRectangle.Width - 21, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 19));
						
			g.DrawLine(Black, new Point(ClientRectangle.Width - 25, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 23));
			g.DrawLine(White, new Point(ClientRectangle.Width - 27, ClientRectangle.Height - 7), new Point(ClientRectangle.Width - 9, ClientRectangle.Height - 25));
		}

		private async Task UpdatePets()
		{
			InfoText = "Update pet list...";

			if(WebPets == null)
			{
				WebPets = new OptionsPets
				{
					pets = new List<OptionsPet>()
				};
			}

			try
			{
				var progress = new Progress<OptionsPet>(async pet =>
				{
					WebPets.pets.Add(pet);
					if(WebPets.pets.Count == 1 || (WebPets.pets.Count % 3) == 0)
						extendedPanel?.Invalidate();
				});
				await OptionsGit.GetPetList(progress);
			}
			catch(Exception ex)
			{
				InfoText = ex.Message;
				Invalidate();
			}

			InfoText = WebPets.pets.Count + " pets available";
			Invalidate();
			extendedPanel.Invalidate();
		}

		/// <summary>
		/// Download Pet from Github.
		/// It will return the downloaded file, if it exists.
		/// Once downloaded, the pet will be set as default mate in the application.
		/// </summary>
		/// <param name="name">folder (ID) of the pet</param>
		public async void DownloadPet(string name)
		{
			InfoText = "Downloading " + name + "...";
			var petsSurface = (extendedPanel as FormOptions2SurfacePets);
			if (petsSurface != null)
			{
				petsSurface.showReadme = false;
				Invalidate();
				petsSurface.Invalidate();
			}

			try
			{
				var content = await OptionsGit.GetPetXml(name);

				var xml = new XmlDocument();
				xml.LoadXml(content);

				Program.MyData.SetXml(xml.OuterXml, "");
				Program.Mainthread.LoadNewXMLFromString(xml.OuterXml);

				InfoText = "Enjoy " + name + "!";
			}
			catch(Exception ex)
			{
				InfoText = "Error downloading " + name + ": " + ex.Message;
			}
			Invalidate();
		}

		/// <summary>
		/// Download Pet from Github to retrieve details.
		/// It will return the downloaded file, if it exists.
		/// Once downloaded, the details of this pet will be filled.
		/// </summary>
		/// <param name="name">folder (ID) of the pet</param>
		public async void PetDetails(string name)
		{
			InfoText = "Downloading " + name + "...";
			var petsSurface = (extendedPanel as FormOptions2SurfacePets);
			if (petsSurface != null)
			{
				petsSurface.showDetails = true;
				Invalidate();
				petsSurface.Invalidate();
			}

			try
			{
				var content = await OptionsGit.GetPetXml(name);

				WebPets.pets.ForEach(wp =>
				{
					if(wp.folder == name)
					{
						wp.Detail = new XmlDocument();
						wp.Detail.LoadXml(content);
					}
				});

				InfoText = name + " details";
			}
			catch (Exception ex)
			{
				InfoText = "Error downloading " + name + ": " + ex.Message;
			}
			Invalidate();
		}
	}

	/// <summary>
	/// All GitHub functionality to the retrieve pet information and files is in this class. 
	/// It downloads the pets.json file from GitHub, checks if it is updated, and downloads the pet icon and readme files as needed.
	/// </summary>
	public class OptionsGit
	{
		/// <summary>
		/// Base path to the pets
		/// </summary>
		public static string BaseGitUrl = "https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Pets/";
		/// <summary>
		/// Local temporary path, to store the github files.
		/// </summary>
		public static string BaseLocalPath = Path.Combine(Path.GetTempPath(), "esheep64");

		/// <summary>
		/// Get the list of pets, from Github or locally.
		/// </summary>
		/// <returns>List of OptionsPets pets</returns>
		public static async Task GetPetList(IProgress<OptionsPet> progress)
		{
			bool updateList = true;
			bool downloadList = false;

			string jsonString = "";
			string jsonFile = Path.Combine(BaseLocalPath, "pets.json");

			if (Directory.Exists(BaseLocalPath))
			{
				if (File.Exists(jsonFile))
				{
					FileInfo fi = new FileInfo(jsonFile);
					if (fi.LastWriteTime > DateTime.Now.AddDays(-1)) updateList = false;
				}
				else
				{
					downloadList = true;
				}
			}
			else
			{
				downloadList = true;
				Directory.CreateDirectory(BaseLocalPath);
			}

			if (updateList)
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.Add("User-Agent", "DesktopPet");


				jsonString = await client.GetStringAsync(BaseGitUrl + "pets.json");
				if (downloadList)
				{
					File.WriteAllText(jsonFile, jsonString);
				}
				else
				{
					var onlinePets = Newtonsoft.Json.JsonConvert.DeserializeObject<OptionsPets>(jsonString);
					var localPets = Newtonsoft.Json.JsonConvert.DeserializeObject<OptionsPets>(File.ReadAllText(jsonFile));
					var isDifferent = false;
					localPets.pets.ForEach(lp =>
					{
						onlinePets.pets.ForEach(op =>
						{
							if (op.folder == lp.folder && op.lastupdate != lp.lastupdate)
							{
								File.Delete(Path.Combine(BaseLocalPath, lp.folder + ".png"));
								File.Delete(Path.Combine(BaseLocalPath, lp.folder + ".xml"));
								File.Delete(Path.Combine(BaseLocalPath, lp.folder + ".md"));
								isDifferent = true;
							}
						});
					});
					if (isDifferent)
					{
						File.WriteAllText(jsonFile, jsonString);
					}
				}
			}
			else
			{
				jsonString = File.ReadAllText(jsonFile);
			}

			var pets = Newtonsoft.Json.JsonConvert.DeserializeObject<OptionsPets>(jsonString);
			//pets.Reorder();

			pets.pets.ForEach(p =>
			{
				using (Stream stream = GetOrDownload(p.folder, "icon.png"))
				{
					p.Image = Image.FromStream(stream);
					progress.Report(p);
				}
			});
			/*
			for (int j = 0; j < pets.pets.Count; j++)
			{
				using (Stream stream = GetOrDownload(pets.pets[j].folder, "icon.png"))
				{
					pets.pets[j].Image = Image.FromStream(stream);

				}
			}*/
		}

		/// <summary>
		/// Get (if neccessary, download) the README.md file for a pet, and return it as a list of strings, one string per line.
		/// </summary>
		/// <param name="folder">folder on Github or name of the pet</param>
		/// <returns>list of lines, markdown formatted</returns>
		public static List<string> GetReadMe(string folder)	
		{
			List<string> readme = new List<string>();
			using (var stream = GetOrDownload(folder, "README.md"))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					var str = reader.ReadToEnd();
					readme = str.Split(new string[] { "<br>", "<br />", "<br/>", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
				}
			}
			return readme;
		}

		/// <summary>
		/// Get (if neccessary, download) the animations.xml file for a pet, and return it as a string.
		/// </summary>
		/// <param name="folder">folder on Github or name of the pet</param>
		/// <returns>the xml as string</returns>
		public static async Task<string> GetPetXml(string folder)
		{
			var xmlStr = "";
			using (var stream = GetOrDownload(folder, "animations.xml"))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					xmlStr = reader.ReadToEnd();
				}
			}
			return xmlStr;
		}

		/// <summary>
		/// Get the file, if not present locally, download it from Github and store it in the local temporary folder. Return the file as a stream.
		/// </summary>
		/// <param name="folder">folder on Github or name of the pet</param>
		/// <param name="filename">name of the file to get</param>
		/// <returns>the file as a stream</returns>
		static private Stream GetOrDownload(string folder, string filename)
		{
			string localFile = Path.Combine(BaseLocalPath, folder + "." + filename.Substring(filename.IndexOf(".") + 1));
			if (!File.Exists(localFile))//BaseGitUrl + folder + "/" + filename
			{
				//using (WebResponse wrFileResponse = WebRequest.Create(url + WebPets.pets[j].folder + "/icon.png").GetResponse())
				using (WebResponse wrFileResponse = WebRequest.Create(BaseGitUrl + folder + "/" + filename).GetResponse())
				{
					using (Stream stream = wrFileResponse.GetResponseStream())
					{
						using (FileStream file = File.Create(localFile))
						{
							stream.CopyTo(file);
						}
					}
				}
			}

			return File.Open(localFile, FileMode.Open, FileAccess.Read);
		}
	}

}

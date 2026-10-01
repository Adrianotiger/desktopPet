using DesktopPet.Properties;
using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using XmlData;
using static DesktopPet.FormOptions2;
using static DesktopPet.FormOptions2SurfaceMenu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DesktopPet
{
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
		private Point _moves;
		private Point _resizes;
		private Point _moveStart;
		private Pen Gray;
		private Pen Blue;
		private Pen White;
		private Pen Black;
		private int pageSelected = 0;
		public OptionsPets WebPets;
		private FormOptions2SurfacePets surfacePets;
		private FormOptions2SurfaceMenu surfaceMenu;
		public string InfoText = "Options";

		public enum WindowType
		{
			Pets = 1,
			AppOptions = 2,
			AppConfiguration = 3,
			Help = 4,
			Info = 5
		};

		public FormOptions2(string Title, WindowType winType)
		{
			InitializeComponent();

			//this.TopMost = true;
			fontTitle = new Font(Font.FontFamily, 15, FontStyle.Bold, GraphicsUnit.Pixel);
			fontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			fontDetail = new Font(Font.FontFamily, 12, FontStyle.Regular, GraphicsUnit.Pixel);
			iconImage = new Icon(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location), 32, 32);
			win98 = Resources.wvleft;
			_moves = new Point(-1, -1);
			_resizes = new Point(-1, -1);

			surfacePets = new FormOptions2SurfacePets(this);
			surfaceMenu = new FormOptions2SurfaceMenu(this);

			RecalculateSizes();

			Gray = new Pen(Color.FromArgb(193, 196, 200), 3);
			Blue = new Pen(Color.FromArgb( 13,   6, 164), 2);
			White = new Pen(Color.FromArgb(255, 255, 255), 1);
			Black = new Pen(Color.FromArgb(150, 150, 150), 1);

			Task.Run(async() => { await UpdatePets(); });

			Controls.Add(surfacePets);
			Controls.Add(surfaceMenu);
		}

		private void RecalculateSizes()
		{
			_closeButtonRect = new Rectangle(Width - 28, 8, 20, 20);
			_resizeRect = new Rectangle(Width - 28, Height - 28, 28, 28);
			_titleRect = new Rectangle(6, 6, Width - 12, 24);

			surfacePets.Location = new Point(150, 60);
			surfacePets.Size = new Size(
					ClientRectangle.Width - 160,
					ClientRectangle.Height - 98);

			surfaceMenu.Location = new Point(6, 32);
			surfaceMenu.Size = new Size(
					Width - 30,
					24);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (_closeButtonRect.Contains(e.Location))
			{
				Close();
			}
			else if(_titleRect.Contains(e.Location) && _moves.X < 0)
			{
				_moves = new Point(e.Location.X, e.Location.Y);
				_moveStart = new Point(Left, Top);
			}
			else if(_resizeRect.Contains(e.Location))
			{
				_resizes = new Point(e.Location.X, e.Location.Y);
				_moveStart = new Point(Width, Height);
			}
			else
			{
				
			}

			if (surfaceMenu.MenuActivated) { surfaceMenu.MenuActivated = false; surfaceMenu.UnHighlight(); surfaceMenu.Invalidate(); }
		}

		public void MenuClicked(MenuItemIndex menu)
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
						if (surfacePets.showReadme)
						{
							surfacePets.showReadme = false;
							surfacePets.Invalidate();

							//if (surfaceMenu.MenuActivated) { surfaceMenu.MenuActivated = false; surfaceMenu.UnHighlight(); surfaceMenu.Invalidate(); }
						}
						break;
					}
				case MenuItemIndex.View_Author: // View - Author
				case MenuItemIndex.View_Date: // View - Date
				case MenuItemIndex.View_Name: // View - Name
					{
						WebPets.Reorder(menu);
						if (!surfacePets.showReadme) surfacePets.Invalidate();
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
		}


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
			if (pageSelected == 0)
			{
				g.FillRectangle(new SolidBrush(Color.White), new Rectangle(8, 58, ClientRectangle.Width - 15, ClientRectangle.Height - 91));
				g.DrawImage(win98, new Point(8, 58));

				if (surfacePets.selectedPet == null)
				{
					g.DrawString(
						"Mates",
						new Font(fontText.FontFamily, 24, FontStyle.Bold),
						Brushes.Black,
						20,
						130);
					g.DrawString(
						"Select a mate to\nview its description.",
						fontDetail,
						Brushes.Black,
						18,
						190);
				}
				else
				{
					var sizeF = g.MeasureString(surfacePets.selectedPet.folder.Replace("_", " "),
						new Font(fontText.FontFamily, surfacePets.selectedPet.folder.Length < 6 ? 24 : 16, FontStyle.Bold),
						new SizeF(130, 80));
					g.DrawString(
						surfacePets.selectedPet.folder.Replace("_", " "),
						new Font(fontText.FontFamily, surfacePets.selectedPet.folder.Length < 6 ? 24 : 16, FontStyle.Bold),
						Brushes.Black,
						new RectangleF(20, 140 - (int)(sizeF.Height / 3), 130, 80)					
						);

					g.DrawImage(surfacePets.selectedPet.image, new Rectangle(50, 90 - (int)(sizeF.Height / 3), 48, 48));

					g.DrawString(
						"Author: \n  " + surfacePets.selectedPet.author,
						fontDetail,
						Brushes.Black,
						20,
						190);

					g.DrawString(
						"Last Update: \n  " + surfacePets.selectedPet.lastupdate,
						fontDetail,
						Brushes.Black,
						20,
						230);
				}

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
			var taskEnd = false;

			try
			{
				_ = Task.Run(async () =>
				{
					while(!taskEnd)
					{
						await Task.Delay(1000);
						Invalidate();
					}
				});
				WebPets = await OptionsGit.GetPetList();
				taskEnd = true;
			}
			catch(Exception ex)
			{
				InfoText = ex.Message;
				Invalidate();
			}

			taskEnd = true;
			InfoText = WebPets.pets.Count + " pets available";
			Invalidate();
			surfacePets.Invalidate();
		}

		public async void DownloadPet(string name)
		{
			InfoText = "Downloading " + name + "...";
			surfacePets.showReadme = false;
			Invalidate();
			surfacePets.Invalidate();

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
	}

	public class OptionsGit
	{
		public static string BaseGitUrl = "https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Pets/";
		public static string BaseLocalPath = Path.Combine(Path.GetTempPath(), "esheep64");

		public static async Task<OptionsPets> GetPetList()
		{
			OptionsPets pets = null;

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
							if (op.lastupdate != lp.lastupdate)
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

			pets = Newtonsoft.Json.JsonConvert.DeserializeObject<OptionsPets>(jsonString);
			pets.Reorder();

			for (int j = 0; j < pets.pets.Count; j++)
			{
				using (Stream stream = GetOrDownload(pets.pets[j].folder, "icon.png"))
				{
					pets.pets[j].image = Image.FromStream(stream);
				}
			}

			return pets;
		}

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
					/*
					using (Stream objWebStream = wrFileResponse.GetResponseStream())
					{
						MemoryStream ms = new MemoryStream();
						objWebStream.CopyTo(ms, 1024 * 16);
						File.WriteAllBytes(localFile, ms.GetBuffer());
					}*/
				}
			}

			return File.Open(localFile, FileMode.Open, FileAccess.Read);
		}
	}

}

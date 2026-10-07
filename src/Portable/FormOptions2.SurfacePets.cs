using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DesktopPet
{
	/// <summary>
	/// This class represents the surface for displaying pets in the options form.
	/// </summary>
	internal class FormOptions2SurfacePets : Panel
	{
		private FormOptions2 form;
		private Font fontText;
		private class Links
		{
			public Rectangle rect;
			public string url;
		};
		private List<Links> links = new List<Links>();
		private List<Links> buttons = new List<Links>();

		/// <summary>
		/// Selected PET, for the readme or for the highligt.
		/// </summary>
		public OptionsPet selectedPet = null;
		/// <summary>
		/// If readme should be shawn.
		/// </summary>
		public bool showReadme = false;
		/// <summary>
		/// If pet details should be shawn.
		/// </summary>
		public bool showDetails = false;

		public FormOptions2SurfacePets(FormOptions2 formOption)
		{
			DoubleBuffered = true;
			form = formOption;
			fontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			Dock = DockStyle.None;
			AutoScroll = true;
			AutoScrollMinSize = new Size(10, 200);
		}

		private int OnPaintReadme(Graphics g)
		{
			int petx = 25;
			int pety = -VerticalScroll.Value;

			var regex = new Regex(@"(\*\*(.*?)\*\*)|(\*(.*?)\*)|(\[(.*?)\]\((.*?)\))|(https?://[^\s]+)");

			links = new List<Links>();

			g.Clear(Color.White);

			// Controls
			pety += 5;
			buttons = new List<Links>();
			buttons.Add(new Links { rect = new Rectangle(petx + 20, pety, 130, 30), url = "Download" });
			buttons.Add(new Links { rect = new Rectangle(petx + 180, pety, 130, 30), url = "Detail" });
			buttons.Add(new Links { rect = new Rectangle(petx + 340, pety, 130, 30), url = "Back" });

			buttons.ForEach(b =>
			{
				ControlPaint.DrawButton(g, b.rect, ButtonState.Normal);
				var s = g.MeasureString(b.url, fontText);
				g.DrawString(b.url, fontText, Brushes.Black, new Point(b.rect.Left + b.rect.Width / 2 - (int)s.Width / 2, b.rect.Y + 5));
			});

			pety += 55;

			selectedPet.Readme.ForEach(line =>
			{
				if (line == String.Empty) return;

				var rect = new RectangleF(petx, pety, Width - 30, 100);
				var sizeF = new SizeF(Width - 30, 100);

				if (line.StartsWith("###"))
				{
					g.DrawString(line.Replace("###", ""), new Font(fontText.FontFamily, 15, FontStyle.Bold), Brushes.Black, rect);
					pety += (int)g.MeasureString(line.Replace("###", ""), new Font(fontText.FontFamily, 15, FontStyle.Bold), sizeF).Height + 10;
				}
				else if (line.StartsWith("##"))
				{
					g.DrawString(line.Replace("##", ""), new Font(fontText.FontFamily, 18, FontStyle.Bold), Brushes.Black, rect);
					pety += (int)g.MeasureString(line.Replace("##", ""), new Font(fontText.FontFamily, 18, FontStyle.Bold), sizeF).Height + 15;
					g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 8, Width / 2, pety - 8);
				}
				else if (line.StartsWith("#"))
				{
					g.DrawString(line.Replace("#", ""), new Font(fontText.FontFamily, 24, FontStyle.Bold), Brushes.Black, rect);
					pety += (int)g.MeasureString(line.Replace("#", ""), new Font(fontText.FontFamily, 24, FontStyle.Bold), sizeF).Height + 15;
					g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 8, Width - 40, pety - 8);
				}
				else if (line.StartsWith("- ") || line.StartsWith("* "))
				{
					g.DrawString(line.Substring(2), fontText, Brushes.Black, petx + 16, pety);
					g.FillEllipse(Brushes.Black, new Rectangle(petx, pety + 5, 8, 8));
					pety += (int)g.MeasureString(line.Substring(2), fontText, sizeF).Height + 5;
				}
				else
				{
					var matches = regex.Matches(line);
					int start = 0;
					foreach (Match match in matches)
					{
						if (match.Index > start)
						{
							var s = g.MeasureString(line.Substring(0, match.Index - start), fontText, sizeF);
							if (petx + s.Width > Width) { petx = 25; pety += (int)s.Height; rect = new RectangleF(petx, pety, Width - 30, 100); }
							g.DrawString(line.Substring(0, match.Index - start), fontText, Brushes.Black, rect);

							if (s.Height > 25 || s.Width > Width - 50) { petx = 25; pety += (int)s.Height; }
							else petx += (int)s.Width;
							start = match.Index + match.Length;
						}
						else if(match.Index == start)
						{
							start = match.Index + match.Length;
						}

						if (match.Groups[2].Success)
						{
							// Bold
							var s = g.MeasureString(match.Groups[2].Value, new Font(fontText, FontStyle.Bold), sizeF);
							if (petx + s.Width > Width) { petx = 25; pety += (int)s.Height; rect = new RectangleF(petx, pety, Width - 30, 100); }
							g.DrawString(match.Groups[2].Value, new Font(fontText, FontStyle.Bold), Brushes.Black, petx, pety);
							petx += (int)s.Width;
						}
						else if (match.Groups[4].Success)
						{
							// Italic
							var s = g.MeasureString(match.Groups[2].Value, new Font(fontText, FontStyle.Italic), sizeF);
							if (petx + s.Width > Width) { petx = 25; pety += (int)s.Height; rect = new RectangleF(petx, pety, Width - 30, 100); }
							g.DrawString(match.Groups[2].Value, new Font(fontText, FontStyle.Italic), Brushes.Black, petx, pety);
							petx += (int)s.Width;
						}
						else if (match.Groups[6].Success)
						{
							// Link
							var s = g.MeasureString(match.Groups[6].Value, fontText, sizeF);
							if (petx + s.Width > Width) { petx = 25; pety += (int)s.Height; rect = new RectangleF(petx, pety, Width - 30, 100); }
							g.DrawString(match.Groups[6].Value, new Font(fontText, FontStyle.Underline), Brushes.Blue, petx, pety);

							links.Add(new Links { rect = new Rectangle(petx, pety, (int)s.Width, (int)s.Height), url = match.Groups[7].Value });

							petx += (int)s.Width;
						}
						else
						{
							// Direct URL
							var s = g.MeasureString(match.Groups[8].Value, fontText, sizeF);
							if (petx + s.Width > Width) { petx = 25; pety += (int)s.Height; rect = new RectangleF(petx, pety, Width - 30, 100); }
							g.DrawString(match.Groups[8].Value, new Font(fontText, FontStyle.Underline), Brushes.Blue, petx, pety);

							links.Add(new Links { rect = new Rectangle(petx, pety, (int)s.Width, (int)s.Height), url = match.Groups[8].Value });

							petx += (int)s.Width;
						}
					}
					if (start < line.Length)
					{
						var s = g.MeasureString(line.Substring(start), fontText);
						if (petx + s.Width > Width)
						{
							petx = 25;
							if (start > 20) pety += 20;
							rect = new RectangleF(petx, pety, Width - 30, 100);

							g.DrawString(line.Substring(start), fontText, Brushes.Black, rect);
							pety += (int)(s.Height * Math.Round(s.Width / Width));

						}
						else
						{
							g.DrawString(line.Substring(start), fontText, Brushes.Black, petx, pety);
							petx += (int)s.Width;
						}
					}

					//g.DrawString(line, fontText, Brushes.Black, rect);
					pety += (int)g.MeasureString(line, fontText).Height + 5;
					petx = 25;
				}
			});
			return pety;
		}

		private int OnPaintDetail(Graphics g)
		{
			int petx = 25;
			int pety = -VerticalScroll.Value;

			var regex = new Regex(@"(\*\*(.*?)\*\*)|(\*(.*?)\*)|(\[(.*?)\]\((.*?)\))|(https?://[^\s]+)");

			links = new List<Links>();

			g.Clear(Color.White);

			// Controls
			pety += 5;
			buttons = new List<Links>();
			buttons.Add(new Links { rect = new Rectangle(petx + 20, pety, 130, 30), url = "Back" });

			buttons.ForEach(b =>
			{
				ControlPaint.DrawButton(g, b.rect, ButtonState.Normal);
				var s = g.MeasureString(b.url, fontText);
				g.DrawString(b.url, fontText, Brushes.Black, new Point(b.rect.Left + b.rect.Width / 2 - (int)s.Width / 2, b.rect.Y + 5));
			});

			pety += 55;

			var fontBold = new Font(fontText, FontStyle.Bold);

			g.DrawString(selectedPet.folder.Replace("_", " "), new Font(fontText.FontFamily, 24, FontStyle.Bold), Brushes.Black, petx, pety);
			pety += 50;
			g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 8, Width - 40, pety - 8);

			g.DrawString("Author: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(selectedPet.author, fontText, Brushes.Black, petx + 110, pety);
			pety += 20;

			g.DrawString("Published: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(selectedPet.lastupdate, fontText, Brushes.Black, petx + 110, pety);
			pety += 40;

			g.DrawString("XML", new Font(fontText.FontFamily, 18, FontStyle.Bold), Brushes.Black, petx, pety);
			pety += 40;
			g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 8, Width - 40, pety - 8);

			g.DrawString("Size: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString((int)(selectedPet.Detail.InnerText.Length / 1024) + " kb", fontText, Brushes.Black, petx + 110, pety);
			pety += 20;

			var childs = selectedPet.Detail.ChildNodes[1].ChildNodes;
			XmlNode xmlheader = null, xmlspawns = null, xmlimage=null, xmlanimations=null, xmlchilds=null, xmlsounds=null;
			for(var j=0;j<childs.Count;j++)
			{
				var childX = childs[j];
				switch(childs[j].Name)
				{
					case "header": xmlheader = childs[j]; break;
					case "image": xmlimage = childs[j]; break;
					case "spawns": xmlspawns = childs[j]; break;
					case "animations": xmlanimations = childs[j]; break;
					case "childs": xmlchilds = childs[j]; break;
					case "sounds": xmlsounds = childs[j]; break;
				}
			}

			for(var j=0;j<xmlheader?.ChildNodes.Count;j++)
			{
				switch(xmlheader.ChildNodes[j].Name)
				{
					case "author":
					case "title":
					case "petname":
					case "version":
						g.DrawString(xmlheader.ChildNodes[j].Name.Substring(0,1).ToUpper() + xmlheader.ChildNodes[j].Name.Substring(1) + ": ", fontBold, Brushes.Black, petx, pety);
						g.DrawString(xmlheader.ChildNodes[j].InnerText, fontText, Brushes.Black, petx + 110, pety);
						pety += 20;
						break;
					case "info":
						var splits = new string[] { "[br]", "\n\r", "\n", "<br>" };
						var lines = xmlheader.ChildNodes[j].InnerText.Split(splits, StringSplitOptions.RemoveEmptyEntries);
						g.DrawString("Info: ", fontBold, Brushes.Black, petx, pety);
						for (var k = 0; k < lines.Length; k++)
						{
							pety += 20;
							g.DrawString(lines[k], fontText, Brushes.Black, petx, pety);
						}
						pety += 20;
						break;
				}
			}

			int tilesx = 0, tilesy = 0;
			for (var j = 0; j < xmlimage?.ChildNodes.Count; j++)
			{
				switch (xmlimage.ChildNodes[j].Name)
				{
					case "tilesx": tilesx = int.Parse(xmlimage.ChildNodes[j].InnerText); break;
					case "tilesy": tilesy = int.Parse(xmlimage.ChildNodes[j].InnerText);
						g.DrawString("Spritesheet size: ", fontBold, Brushes.Black, petx, pety);
						g.DrawString((tilesx * tilesy) + " images", fontText, Brushes.Black, petx + 150, pety);
						pety += 20;
						break;
				}
			}

			pety += 20;

			g.DrawString("Total Animations: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(xmlanimations?.ChildNodes.Count.ToString() ?? "-", fontText, Brushes.Black, petx + 150, pety);
			pety += 20;

			g.DrawString("Total Childs: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(xmlchilds?.ChildNodes.Count.ToString() ?? "-", fontText, Brushes.Black, petx + 150, pety);
			pety += 20;

			g.DrawString("Total Spawns: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(xmlspawns?.ChildNodes.Count.ToString() ?? "-", fontText, Brushes.Black, petx + 150, pety);
			pety += 20;

			g.DrawString("Total Sounds: ", fontBold, Brushes.Black, petx, pety);
			g.DrawString(xmlsounds?.ChildNodes.Count.ToString() ?? "-", fontText, Brushes.Black, petx + 150, pety);
			pety += 20;

			return pety;
		}

		private int OnPaintPets(Graphics g)
		{
			int petx = 25;
			int pety = -VerticalScroll.Value;

			List<OptionsPet> pets = form.WebPets.pets;

			g.Clear(Color.White);

			for (int j = 0; j < pets.Count; j++)
			{
				if (selectedPet == pets[j])
				{
					g.FillRectangle(Brushes.Yellow, petx - 25, pety + 34, 82, 17);
					using (Pen pen = new Pen(Color.Black, 1))
					{
						pen.DashStyle = DashStyle.Dot;
						g.DrawRectangle(pen, petx - 25, pety + 34, 82, 16);
					}
				}
				if (pets[j].Image == null)
				{
					g.FillRectangle(Brushes.AliceBlue, petx, pety, 32, 32);
				}
				else
				{
					if (selectedPet == pets[j])
					{
						ImageAttributes attributes = new ImageAttributes();

						ColorMatrix matrix = new ColorMatrix(new float[][]
						{
									new float[] { 1, 0.5f, 0, 0, 0 }, // Rot
									new float[] { 1, 0.5f, 0, 0, 0 }, // Grün
									new float[] { 1, 0, 0, 0, 0 }, // Blau
									new float[] { 0, 0, 0, 1, 0 }, // Alpha
									new float[] { 0, 0, 0, 0, 1 }
						});
						attributes.SetColorMatrix(matrix);

						g.DrawImage(
							pets[j].Image,
							new Rectangle(petx, pety, 32, 32),
							0,
							0,
							pets[j].Image.Width,
							pets[j].Image.Height,
							GraphicsUnit.Pixel,
							attributes);
					}
					else
					{
						g.DrawImage(pets[j].Image, petx, pety, 32, 32);
					}
				}

				var text = pets[j].folder;
				if (text.Length > 10) text = text.Substring(0, 8) + "...";
				g.DrawString(text, fontText, Brushes.Black, new Point(petx + 16 - (int)(g.MeasureString(text, fontText).Width / 2), pety + 32));
				petx += 90;
				if (petx > Width - 54) { petx = 25; pety += 60; }
			}
			return pety;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (form.WebPets == null || form.WebPets.pets == null) return; // not ready to render

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			int pety = -VerticalScroll.Value;

			if(showDetails)
			{
				pety = OnPaintDetail(g);
			}
			else if (showReadme)
			{
				pety = OnPaintReadme(g);
			}
			else
			{
				pety = OnPaintPets(g);
			}
			
			if (AutoScrollMinSize.Height < pety + VerticalScroll.Value - 30)
			{
				AutoScrollMinSize = new Size(10, pety + VerticalScroll.Value + 50);
			}
			else if (AutoScrollMinSize.Height > pety + VerticalScroll.Value + 100)
			{
				AutoScrollMinSize = new Size(10, pety + VerticalScroll.Value + 50);
			}
			
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			form.MenuClicked(0);

			if (showReadme)
			{
				links.ForEach(l =>
				{
					if (l.rect.Contains(e.Location))
					{
						System.Diagnostics.Process.Start(new ProcessStartInfo
						{
							FileName = l.url,
							UseShellExecute = true
						});
					}
				});

				buttons.ForEach(b =>
				{
					if(b.rect.Contains(e.Location))
					{
						switch(b.url)
						{
							case "Download": form.DownloadPet(selectedPet.folder); break;
							case "Back": showReadme = false; showDetails = false; Invalidate(); break;
							case "Detail": form.PetDetails(selectedPet.folder); break;
						}
						
					}
				});

				return;
			}

			List<OptionsPet> pets = form.WebPets.pets;
			var oldSelected = selectedPet;

			selectedPet = null;

			int petx = 25;
			int pety = -VerticalScroll.Value;
			for (int j = 0; j < pets.Count; j++)
			{
				if (e.Location.X > petx - 10 && e.Location.X < petx + 52)
				{
					if (e.Location.Y > pety && e.Location.Y < pety + 52)
					{
						selectedPet = pets[j];
						break;
					}
				}
				petx += 90;
				if (petx > Width - 48) { petx = 25; pety += 60; }
			}

			if (oldSelected != selectedPet)
			{
				Invalidate();
				form.Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (showReadme)
			{
				bool hasLink = false;
				links.ForEach(l =>
				{
					if (l.rect.Contains(e.Location))
					{
						hasLink = true;
						Cursor = Cursors.Hand;
					}
				});
				buttons.ForEach(b =>
				{
					if (b.rect.Contains(e.Location))
					{
						hasLink = true;
						Cursor = Cursors.Hand;
					}
				});
				if (!hasLink && Cursor == Cursors.Hand) Cursor = Cursors.Arrow;
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);

			if (showReadme) return;

			form.InfoText = "Downloading README.md";

			if (selectedPet != null)
			{
				showReadme = true;
				VerticalScroll.Value = 0;

				if (selectedPet.Readme.Count == 0)
				{
					try
					{
						selectedPet.Readme = OptionsGit.GetReadMe(selectedPet.folder);
					}catch(Exception ex)
					{
						form.InfoText = ex.Message;
						form.Invalidate();
						return;
					}
				}
			}

			form.InfoText = selectedPet.folder + " Readme.md";
			Invalidate();
		}
	}

	/// <summary>
	/// Represents a pet with its folder, author, and last update date.
	/// Base-Info to list in the option page.
	/// </summary>
	public class OptionsPet
	{
		/// <summary>
		/// Folder on GitHub
		/// </summary>
		public string folder { get; set; }
		/// <summary>
		/// Author name
		/// </summary>
		public string author { get; set; }
		/// <summary>
		/// Last update of the animation file
		/// </summary>
		public string lastupdate { get; set; }

		/// <summary>
		/// Image to show in the windows95-explorer on the left side
		/// </summary>
		public Image Image { get; set; } = null;
		/// <summary>
		/// Readme to parse and show on the page
		/// </summary>
		public List<String> Readme { get; set; } = new List<string>();
		/// <summary>
		/// Detail XML, loaded directly from XML animation file
		/// </summary>
		public XmlDocument Detail { get; set; } = null;
	}
	/// <summary>
	/// Class conaining a list of pets and a method to reorder them by last update date.
	/// </summary>
	public class OptionsPets
	{
		/// <summary>
		/// List of pets
		/// </summary>
		public List<OptionsPet> pets { get; set; }
		/// <summary>
		/// Reorder the pets, once loaded, by last update date, so the most recent ones are on top.
		/// </summary>
		public void Reorder(FormOptions2SurfaceMenu.MenuItemIndex sorting = FormOptions2SurfaceMenu.MenuItemIndex.View_Date)
		{
			pets.Sort(delegate (OptionsPet x, OptionsPet y)
			{
				switch(sorting)
				{
					case FormOptions2SurfaceMenu.MenuItemIndex.View_Author:
						return x.author.CompareTo(y.author);
					case FormOptions2SurfaceMenu.MenuItemIndex.View_Name:
						return x.folder.CompareTo(y.folder);
					default:
						return y.lastupdate.CompareTo(x.lastupdate);
				}
			});
		}
	}

}

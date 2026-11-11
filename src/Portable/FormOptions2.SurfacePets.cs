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
using System.Net.Http.Headers;
using System.Runtime.Remoting.Contexts;
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
	public class OptionsPetIcon
	{
		public OptionsPet pet;
		public int x = 0;
		public int y = 0;
		public int w = 82;
		public int h = 48;

		public OptionsPetIcon(OptionsPet pet)
		{
			this.pet = pet;
		}
	};

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
		public List<OptionsPetIcon> allPets = null;
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

		public void ReloadPets()
		{
			allPets = null;
			Invalidate();
		}

		void DrawWords(string text, Graphics g, Font font, ref int x, ref int y, int maxX, int minX=25)
		{
			string[] words = text.Split(' ');

			for (int i = 0; i < words.Length; i++)
			{
				if (words[i].Length == 0) continue;

				var subr = g.MeasureString(words[i], fontText);
				if (x + subr.Width > maxX)
				{
					x = minX;
					y += (int)subr.Height + 2;
				}
				g.DrawString(words[i], fontText, Brushes.Black, x, y);
				x += (int)(Math.Floor(subr.Width));
			}
		}

		private int OnPaintReadme(Graphics g)
		{
			int startPetX = 25;
			int petx = 25;
			int pety = -VerticalScroll.Value;
			int rightBorder = 50;

			var regex = new Regex(@"(\*\*(.*?)\*\*)|(\*(.*?)\*)|(\[(.*?)\]\((.*?)\))|(https?://[^\s]+)");

			links = new List<Links>();

			// Use this for a debug-mode when you want to see, word for word, how the text is drawn.
			//Graphics g = this.CreateGraphics();
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
					g.DrawString(line.Replace("###", ""), new Font(fontText.FontFamily, 15, FontStyle.Bold), Brushes.Black, petx, pety);
					pety += (int)g.MeasureString(line.Replace("###", ""), new Font(fontText.FontFamily, 15, FontStyle.Bold), sizeF).Height + 10;
				}
				else if (line.StartsWith("##"))
				{
					pety += 10;
					g.DrawString(line.Replace("##", ""), new Font(fontText.FontFamily, 18, FontStyle.Bold), Brushes.Black, petx, pety);
					pety += (int)g.MeasureString(line.Replace("##", ""), new Font(fontText.FontFamily, 18, FontStyle.Bold), sizeF).Height + 15;
					g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 11, Width / 2, pety - 11);
				}
				else if (line.StartsWith("#"))
				{
					g.DrawString(line.Replace("#", ""), new Font(fontText.FontFamily, 24, FontStyle.Bold), Brushes.Black, petx, pety);
					pety += (int)g.MeasureString(line.Replace("#", ""), new Font(fontText.FontFamily, 24, FontStyle.Bold), sizeF).Height + 15;
					g.DrawLine(new Pen(Brushes.Gray, 1), 20, pety - 8, Width - 40, pety - 8);
				}
				else
				{
					int start = 0;
					int startPetX2 = startPetX;

					//if (line.StartsWith("- ") || line.StartsWith("* "))
					var matchli = Regex.Match(line, @"^\s*([*-]|(\d+)\.)\s+(.+)$");
					if (matchli.Success)
					{
						if (line[0] == ' ')
						{
							if (line[0] == '-' || line[0] == '*')
							{
								g.DrawEllipse(Pens.Black, new Rectangle(petx + 10, pety + 5, 8, 8));
							}
							else
							{
								g.DrawString(matchli.Groups[1].Value, new Font(fontText, FontStyle.Bold), Brushes.Black, startPetX + 15, pety);
							}
							startPetX2 += 15;
						}
						else
						{
							if (line[0] == '-' || line[0] == '*')
							{
								g.FillEllipse(Brushes.Black, new Rectangle(petx, pety + 5, 8, 8));
							}
							else
							{
								g.DrawString(matchli.Groups[1].Value, new Font(fontText, FontStyle.Bold), Brushes.Black, startPetX + 5, pety);
							}
						}
						int charsQty = line.Length - matchli.Groups[3].Length;
						line = line.Substring(charsQty);
						startPetX2 += 20;
						petx = startPetX2;
					}

					var matches = regex.Matches(line);

					foreach (Match match in matches)
					{
						if (match.Index > start)
						{
							// split normal string to words
							var substr = line.Substring(start, match.Index - start);
							DrawWords(substr, g, fontText, ref petx, ref pety, Width - rightBorder, startPetX2);
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
					// Draw the rest of the line or the entire line when no special commands was found
					if (start < line.Length)
					{
						// split normal string to words
						var substr = line.Substring(start);

						DrawWords(substr, g, fontText, ref petx, ref pety, Width - rightBorder, startPetX2);
					}

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
			if (selectedPet.Detail == null)
			{
				g.DrawString("Error loading XML!", fontText, Brushes.DarkRed, petx + 110, pety);
			}
			else
			{
				g.DrawString((int)(selectedPet.Detail.InnerText.Length / 1024) + " kb", fontText, Brushes.Black, petx + 110, pety);
				pety += 20;

				var childs = selectedPet.Detail.ChildNodes[1].ChildNodes;
				XmlNode xmlheader = null, xmlspawns = null, xmlimage = null, xmlanimations = null, xmlchilds = null, xmlsounds = null;
				for (var j = 0; j < childs.Count; j++)
				{
					var childX = childs[j];
					switch (childs[j].Name)
					{
						case "header": xmlheader = childs[j]; break;
						case "image": xmlimage = childs[j]; break;
						case "spawns": xmlspawns = childs[j]; break;
						case "animations": xmlanimations = childs[j]; break;
						case "childs": xmlchilds = childs[j]; break;
						case "sounds": xmlsounds = childs[j]; break;
					}
				}

				for (var j = 0; j < xmlheader?.ChildNodes.Count; j++)
				{
					switch (xmlheader.ChildNodes[j].Name)
					{
						case "author":
						case "title":
						case "petname":
						case "version":
							g.DrawString(xmlheader.ChildNodes[j].Name.Substring(0, 1).ToUpper() + xmlheader.ChildNodes[j].Name.Substring(1) + ": ", fontBold, Brushes.Black, petx, pety);
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

				int tilesx = 0, tilesy = 0, skins = 0;
				for (var j = 0; j < xmlimage?.ChildNodes.Count; j++)
				{
					switch (xmlimage.ChildNodes[j].Name)
					{
						case "tilesx": tilesx = int.Parse(xmlimage.ChildNodes[j].InnerText); break;
						case "tilesy":
							tilesy = int.Parse(xmlimage.ChildNodes[j].InnerText);
							g.DrawString("Spritesheet size: ", fontBold, Brushes.Black, petx, pety);
							g.DrawString((tilesx * tilesy) + " images", fontText, Brushes.Black, petx + 150, pety);
							pety += 20;
							break;
						case "skins":
							skins = int.Parse(xmlimage.ChildNodes[j].InnerText);
							g.DrawString(skins + " skin(s)", fontText, Brushes.Black, petx + 150, pety);
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
			}

			return pety;
		}

		private int OnPaintPets(Graphics g)
		{
			int petx = 25;
			int pety = 10-VerticalScroll.Value;
			string path = "";

			g.Clear(Color.White);

			if(allPets == null)
			{
				allPets = new List<OptionsPetIcon>();
				// Make copy of list as this is an async call!
				allPets.AddRange(form.WebPets.pets.ToList().Select(a => new OptionsPetIcon(a)));
				form.DevPets.ToList().ForEach( dev => allPets.AddRange(dev.pets.Select(a => new OptionsPetIcon(a))) );
				//List<OptionsPet> pets = form.WebPets.pets;
			}

			//for (int j = 0; j < allPets.Count; j++)
			allPets.ForEach(petIcon =>
			{
				if(petIcon.pet.WebPath != path)
				{
					if (path != "")
					{
						pety += 60;
						petx = 25;

						ControlPaint.DrawBorder(g, new Rectangle(5, pety, Width - 25, 2), Color.LightGray, ButtonBorderStyle.Outset);
						g.DrawString(path, new Font(fontText, FontStyle.Bold | FontStyle.Underline), Brushes.LightSteelBlue, petx - 20, pety + 5);
						pety += 35;
					}

					path = petIcon.pet.WebPath;
					
				}

				// calc position:
				petIcon.x = petx;
				petIcon.y = pety;

				if (selectedPet == petIcon.pet)
				{
					g.FillRectangle(Brushes.Yellow, petIcon.x, petIcon.y + petIcon.h - fontText.Height + 2, petIcon.w, fontText.Height);
					using (Pen pen = new Pen(Color.Black, 1))
					{
						pen.DashStyle = DashStyle.Dot;
						g.DrawRectangle(pen, petIcon.x, petIcon.y + petIcon.h - fontText.Height + 2, petIcon.w, fontText.Height);
					}
				}

				if (petIcon.pet.Image == null)
				{
					g.FillRectangle(Brushes.AliceBlue, petIcon.x + 10, petIcon.y, petIcon.w - 20, petIcon.h - 15);
				}
				else
				{
					if (selectedPet == petIcon.pet)
					{
						ImageAttributes attributes = new ImageAttributes();
						attributes.SetColorMatrix(form.IconHighlightMatrix);

						g.DrawImage(
							selectedPet.Image,
							new Rectangle(petIcon.x + (petIcon.w-32)/2, petIcon.y, 32, 32),
							0,
							0,
							selectedPet.Image.Width,
							selectedPet.Image.Height,
							GraphicsUnit.Pixel,
							attributes);
					}
					else
					{
						g.DrawImage(petIcon.pet.Image, petIcon.x + (petIcon.w - 32) / 2, petIcon.y, 32, 32);
					}
				}

				var text = petIcon.pet.folder;
				if (text.Length > 10) text = text.Substring(0, 8) + "...";
				g.DrawString(text, fontText, Brushes.Black, new Point(petIcon.x + petIcon.w / 2 - (int)(g.MeasureString(text, fontText).Width / 2), petIcon.y + petIcon.h - fontText.Height));

				petx += 90;
				if (petx > Width - petIcon.w) { petx = 25; pety += 60; }
			});

			/*
				if (devIndex >= form.DevPets.Count())
				{
					pets = null;
				}
				else
				{
					pety += 60;
					petx = 25;
					pets = form.DevPets[0].pets;
					ControlPaint.DrawBorder(g, new Rectangle(5, pety, Width - 25, 2), Color.LightGray, ButtonBorderStyle.Outset);
					g.DrawString(form.DevPets[devIndex].BasePath, new Font(fontText, FontStyle.Bold |FontStyle.Underline), Brushes.DarkBlue, petx - 20, pety + 5);
					pety += 35;
				}
				devIndex++;
			*/
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
						Program.OpenLink(l.url);
					}
				});

				buttons.ForEach(b =>
				{
					if(b.rect.Contains(e.Location))
					{
						switch(b.url)
						{
							case "Download": form.DownloadPet(selectedPet); break;
							case "Back": showReadme = false; showDetails = false; Invalidate(); break;
							case "Detail": form.PetDetails(selectedPet); break;
						}
						
					}
				});

				return;
			}

			var oldSelected = selectedPet;

			selectedPet = null;

			allPets.ForEach(p =>
			{
				if(e.Location.Y >= p.y && e.Location.Y <= p.y + p.h)
				{
					if(e.Location.X >= p.x && e.Location.X <= p.x + p.w)
					{
						selectedPet = p.pet;
						return;
					}
				}
			});

			if (oldSelected != selectedPet)
			{
				Invalidate();
				if (selectedPet == null)
					form.SetLeftInfo(null, "Mate", "Select a mate to\nview its description.");
				else
					form.SetLeftInfo(
						selectedPet.Image, 
						selectedPet.folder.Replace("_", " "),
						"Author: \n  " + selectedPet.author + "\n\n" + "Last Update: \n  " + selectedPet.lastupdate);
				//form.Invalidate();
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
						form.InfoText = l.url;
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
						selectedPet.Readme = OptionsGit.GetReadMe(selectedPet);
					}catch(Exception ex)
					{
						form.InfoText = ex.Message;
						form.Invalidate();
						return;
					}
				}

				form.InfoText = selectedPet.folder + " Readme.md";
				VerticalScroll.Value = 0;
			}

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

		public String WebPath { get; set; } = "";
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
		/// Base path on the web (gitHub folder)
		/// </summary>
		public string BasePath { get; set; } = "";
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

using DesktopPet.Properties;
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
using static DesktopPet.LocalData;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DesktopPet
{
	/// <summary>
	/// This class represents the surface for displaying option icons in the options form.
	/// </summary>
	internal class FormOptions2SurfaceOptions : Panel
	{
		private FormOptions2 form;
		private Font fontText;

		private List<OptionIcons> options = new List<OptionIcons>();
		private OptionIcons selectedOption = null;
		private bool showActiveOptions = false;

		public static class SpecialOptions
		{
			public const string RestorePet = "Restore Pet";
		}


		public FormOptions2SurfaceOptions(FormOptions2 formOption)
		{
			DoubleBuffered = true;
			form = formOption;
			fontText = new Font(Font.FontFamily, 14, FontStyle.Regular, GraphicsUnit.Pixel);
			Dock = DockStyle.None;
		}

		public void GenerateAnimationOptionSurface()
		{ 
			// Note: Order Options by Title, like on Windows
			options.Add(new OptionIcons
			{
				Title = "Multiscreen",
				Setting = SettingName.Multiscreen,
				IsCheckbox = true,
				Value = Program.MyData.GetIntValue(SettingName.Multiscreen),
				Image = Resources.ico_multi,
				Description = "If you have more than 1 screen, the pet will not spawn only on the primary monitor."
			});

			options.Add(new OptionIcons
			{
				Title = "Smooth Movements",
				Setting = SettingName.SmoothMovements,
				IsCheckbox = true,
				Value = Program.MyData.GetIntValue(SettingName.SmoothMovements),
				Image = Resources.ico_smooth,
				Description = "Movement are croppy / jittery as it moves several pixels on each frame. If you have it smoother, the movement can be unnatural."
			});

			options.Add(new OptionIcons
			{
				Title = "Taskbar Focus",
				Setting = SettingName.StealTaskbarFocus,
				IsCheckbox = true,
				Value = Program.MyData.GetIntValue(SettingName.StealTaskbarFocus),
				Image = Resources.ico_focus,
				Description = "Prevent the Taskbar to overlap the pet (click on the tray icon will be more difficult as the pet will steal the focus)."
			});

			options.Add(new OptionIcons
			{
				Title = "Volume",
				Setting = SettingName.Volume,
				IsCheckbox = false,
				MaxValue = 100,
				Value = Program.MyData.GetIntValue(SettingName.Volume),
				Image = Resources.ico_sound,
				Description = "If the pet contains also sounds/fx, you can set the volume for it."
			});

			options.Add(new OptionIcons
			{
				Title = "Win Foreground",
				Setting = SettingName.WinForeground,
				IsCheckbox = true,
				Value = Program.MyData.GetIntValue(SettingName.WinForeground),
				Image = Resources.ico_foreground,
				Description = "The original eSheep was able to bring in front the collided window. This will decrease your productivity, so it is disabled by default."
			});

			showActiveOptions = true;
			selectedOption = options[0];
			OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

			Invalidate();
		}

		public void GenerateConfigurationSurface()
		{
			// Note: Order Options by Title, like on Windows

			options.Add(new OptionIcons
			{
				Title = "Pet Quantity",
				Setting = SettingName.AutostartPets,
				IsCheckbox = false,
				MaxValue = 16,
				Value = Program.MyData.GetIntValue(SettingName.AutostartPets),
				Image = Resources.ico_quantity,
				Description = "Select the quantity of pets to start when the application starts."
			});

			options.Add(new OptionIcons
			{
				Title = SpecialOptions.RestorePet,
				Setting = SettingName.Undefined,
				IsCheckbox = true,
				Value = 0,
				Image = Resources.ico_restore,
				Description = "If you have some troubles, you can restore the default pet animation (the original one integrated in the app)."
			});

			options.Add(new OptionIcons
			{
				Title = "Scale",
				Setting = SettingName.PetScale,
				IsCheckbox = false,
				Value = Program.MyData.GetIntValue(SettingName.PetScale),
				Values = new List<int> { 1, 2, 4},
				ValuesText = new List<string> { "1x", "2x", "4x"},
				Image = Resources.ico_resize,
				NeedRestart = true,
				Description = "On HD monitors, the pet is really small. \nScale your pet to 1x, 2x or 4x."
			});

			selectedOption = options[0];
			OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

			Invalidate();
		}

		private int OnPaintIcons(Graphics g)
		{
			int petx = 25;
			int pety = -VerticalScroll.Value;
			int iconSize = 48;

			g.Clear(Color.White);

			pety += 10;

			for (int j = 0; j < options.Count; j++)
			{
				if (selectedOption == options[j])
				{
					g.FillRectangle(Brushes.Yellow, petx - 25, pety + iconSize + 2, iconSize + 50, 17);
					using (Pen pen = new Pen(Color.Black, 1))
					{
						pen.DashStyle = DashStyle.Dot;
						g.DrawRectangle(pen, petx - 25, pety + iconSize + 2, iconSize + 50, 16);
					}
				}
				if (options[j].Image == null)
				{
					g.FillRectangle(Brushes.AliceBlue, petx, pety, iconSize, iconSize);
				}
				else
				{
					if (selectedOption == options[j])
					{
						ImageAttributes attributes = new ImageAttributes();

						attributes.SetColorMatrix(form.IconHighlightMatrix);

						g.DrawImage(
							options[j].Image,
							new Rectangle(petx, pety, iconSize, iconSize),
							0,
							0,
							options[j].Image.Width,
							options[j].Image.Height,
							GraphicsUnit.Pixel,
							attributes);
					}
					else
					{
						g.DrawImage(options[j].Image, petx, pety, iconSize, iconSize);
					}

					if (showActiveOptions && options[j].Value > 0)
					{
						g.DrawImage(Resources.enabled, petx + iconSize - 16, pety + iconSize - 16 - 5, 32, 32);
					}
				}

				var text = options[j].Title;
				if (text.Length > 13) text = text.Substring(0, 12) + "...";
				g.DrawString(text, fontText, Brushes.Black, new Point(petx + iconSize/2 - (int)(g.MeasureString(text, fontText).Width / 2), pety + iconSize));
				petx += iconSize + 60;
				if (petx > Width - 54) { petx = 25; pety += iconSize + 28; }
			}
			return pety;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var g = e.Graphics;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

			int pety = OnPaintIcons(g);
			
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

			var oldSelected = selectedOption;
			selectedOption = null;

			int petx = 25;
			int pety = -VerticalScroll.Value;
			int iconSize = 48;

			pety += 10;
			for (int j = 0; j < options.Count; j++)
			{
				if (e.Location.X > petx - 10 && e.Location.X < petx + iconSize + 20)
				{
					if (e.Location.Y > pety && e.Location.Y < pety + iconSize + 20)
					{
						selectedOption = options[j];
						break;
					}
				}
				petx += iconSize + 60;
				if (petx > Width - 54) { petx = 25; pety += iconSize + 28; }
			}

			if (oldSelected != selectedOption)
			{
				Invalidate();
				if (selectedOption == null)
				{
					form.SetLeftInfo(null, "Options", "Select an Option\nto view its description.");
				}
				else
				{
					var description = selectedOption.Description;
					description += "\n\n";
					if (selectedOption.IsCheckbox) description += selectedOption.Value > 0 ? "Enabled" : "Disabled";
					else if (selectedOption.MaxValue > 0) description += selectedOption.Title + ": " + selectedOption.Value;
					form.SetLeftInfo(
						selectedOption.Image,
						selectedOption.Title,
						description);
				}
				//form.Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			

		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);

			if(selectedOption != null)
			{
				var dialog = new FormOptions2(selectedOption.Title, FormOptions2.WindowType.Dialog);
				int value = 0;
				if (selectedOption.IsCheckbox)
				{
					value =  Program.MyData.GetIntValue(selectedOption.Setting);
					dialog.AddDialogControl(value > 0);
				}
				else if(selectedOption.MaxValue > 0)
				{
					value = Program.MyData.GetIntValue(selectedOption.Setting);
					dialog.AddDialogControl(value, selectedOption.MaxValue);
				}
				else if (selectedOption.Values.Count > 0)
				{
					value = Program.MyData.GetIntValue(selectedOption.Setting);
					dialog.AddDialogControl(value, selectedOption.Values, selectedOption.ValuesText);
				}
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					int value2 = dialog.GetDialogControl();
					if(value != value2)
					{
						if (selectedOption.Setting == SettingName.Undefined)
						{
							switch(selectedOption.Title)
							{
								case SpecialOptions.RestorePet:
									form.DialogResult = DialogResult.Retry;
									form.Close();
									break;
							}
						}
						else
						{
							Program.MyData.SetIntValue(selectedOption.Setting, value2);
							selectedOption.Value = value2;
							if (selectedOption.NeedRestart)
							{
								Program.RestartApp();
							}
							else
							{
								OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Represents a pet with its folder, author, and last update date.
	/// Base-Info to list in the option page.
	/// </summary>
	public class OptionIcons
	{
		/// <summary>
		/// Title of the icon, you can see it in the left side of the window, on the icon and on the dialog when you change the value.
		/// </summary>
		public string Title { get; set; } = "TITLE";
		/// <summary>
		/// Description of the option, you can see it on the left side of the window
		/// </summary>
		public string Description { get; set; } = "Description";

		/// <summary>
		/// Image to show in the windows95-explorer on the left side
		/// </summary>
		public Image Image { get; set; } = null;
		/// <summary>
		/// Setting to change in the AppSettings. <see cref="SettingName"/>
		/// </summary>
		public string Setting { get; set; } = "";
		/// <summary>
		/// If Checkbox, you can ENABLE or DISABLE this option
		/// </summary>
		public bool IsCheckbox { get; set; } = false;
		/// <summary>
		/// Each Option has a integrer value (a checkbox is 0 or 1, combobox is the index)
		/// </summary>
		public int Value { get; set; } = 0;
		/// <summary>
		/// If Option is a combobox, set the possible values
		/// </summary>
		public List<int> Values { get; set; } = new List<int>();
		/// <summary>
		/// If Option is a combobox, set the possible text/description for each value
		/// </summary>
		public List<string> ValuesText { get; set; } = new List<string>();
		/// <summary>
		/// If Option is a scrollbar, se the maximum possible value
		/// </summary>
		public int MaxValue { get; set; } = 0;
		/// <summary>
		/// If the application will be restarted, once the option is changed (for example when you scale the pets)
		/// </summary>
		public bool NeedRestart { get; set; } = false;
	}

}

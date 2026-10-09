using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DesktopPet
{
    /// <summary>
    /// LocalData for the portable version.
    /// Todo: Create an interface (Portable + Store version)
    /// </summary>
    public class LocalData
    {
        Configuration AppConfiguration = null;
        KeyValueConfigurationCollection AppSettings = null;
		readonly bool isInstalled = false;

        /// <summary>
        /// Keys for valid Application Settings
        /// </summary>
        public static class SettingName
        {
            /// <summary>
            /// If Undefined is set, no Setting will be modified
            /// </summary>
            public const string Undefined = "";
            /// <summary>
            /// Volume-Key
            /// </summary>
            public const string Volume = "Volume";
            /// <summary>
            /// Pet-Scaling-Key
            /// </summary>
            public const string PetScale = "Scale";
            /// <summary>
            /// Multiscreen-Key
            /// </summary>
			public const string Multiscreen = "Multiscreen";
            /// <summary>
            /// Window To Foreground-Key
            /// </summary>
            public const string WinForeground = "WinForeground";
            /// <summary>
            /// Steal Taskbar-Key
            /// </summary>
            public const string StealTaskbarFocus = "StealTaskbarFocus";
            /// <summary>
            /// Autostart Pets when application starts - Key
            /// </summary>
			public const string AutostartPets = "AutostartPets";
            /// <summary>
            /// Smooth animations-Key
            /// </summary>
			public const string SmoothMovements = "Smooth";
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalData"/> class.
		/// </summary>
		public LocalData()
        {
			try
            {
#if PORTABLE
                string configFile = "DesktopPet.config";

                if (Program.IsApplicationInstalled())
                {
                    isInstalled = true;
                }
#else
				string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eSheep64");
				if(!Directory.Exists(configDir))
					Directory.CreateDirectory(configDir);

				string configFile = Path.Combine(configDir, "DesktopPet.config");
#endif
				AppConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
					new ExeConfigurationFileMap { ExeConfigFilename = configFile }, ConfigurationUserLevel.None);

				LoadSettings();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error opening settings: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

		/// <summary>
		/// Loads the settings from the configuration file (if installed, the cofiguration is in the user data folder)
		/// </summary>
		public void LoadSettings()
        {
            //var settings = AppConfiguration.AppSettings.Settings;
            foreach (SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                if (AppConfiguration.AppSettings.Settings[currentProperty.Name] == null)
                {
                    AppConfiguration.AppSettings.Settings.Add(currentProperty.Name, currentProperty.DefaultValue.ToString());
                }
            }
            AppSettings = AppConfiguration.AppSettings.Settings;
        }

		/// <summary>
		/// Set FX-Sound volume (0.0 - 1.0)
		/// </summary>
		/// <param name="volume">Value from 0 to 1.0</param>
		public void SetVolume(double volume)
        {
            int iVolume = (int)(volume * 100);
            if (iVolume.ToString() != AppSettings[SettingName.Volume].Value)
            {
                Properties.Settings.Default.Volume = iVolume;
                AppSettings[SettingName.Volume].Value = iVolume.ToString();
                Save();
            }
        }

		/// <summary>
		/// Get the FX-Sound volume (0.0 - 1.0)
		/// </summary>
		/// <returns>Value from 0 for no sound to 1.0 for full sound</returns>
		public float GetVolume()
        {
			int.TryParse(AppSettings[SettingName.Volume].Value, out int iVolume);
			return (float)(iVolume / 100.0);
        }

		/// <summary>
		/// Set the scale of the pet (1, 2, 4, 8). The limit is in the option dialog. The scale is stored as a power of 2 (0, 1, 2, 3). The default is 1 (0).
		/// </summary>
		/// <param name="pow2">Set the scale as a power of 2</param>
		public void SetScale(int pow2)
        {
            if (pow2.ToString() != AppSettings[SettingName.PetScale].Value)
            {
                Properties.Settings.Default.Scale = pow2;
                AppSettings[SettingName.PetScale].Value = pow2.ToString();
                Save();
            }
        }
        /// <summary>
        /// Get the current scale of the pet.
        /// </summary>
        /// <returns></returns>
        public int GetScale()
        {
            if (int.TryParse(AppSettings[SettingName.PetScale].Value, out int iScale))
            {
                return iScale;
            }
            return 1;
        }

        /// <summary>
        /// If multiscreen is enable in the option
        /// </summary>
        /// <returns>true, if multiscreen is enabled and the pet should move between screens</returns>
        public bool GetMultiscreen()
        {
            bool.TryParse(AppSettings[SettingName.Multiscreen].Value, out bool ret);
            return ret;
        }

		/// <summary>
		/// Set if the pet should move between screens (multiscreen)
		/// </summary>
		/// <param name="multi">true, if you want to see the pet moving over 2 screens</param>
		public void SetMultiscreen(bool multi)
        {
            if (multi.ToString() != AppSettings[SettingName.Multiscreen].Value)
            {
                Properties.Settings.Default.Multiscreen = multi;
                AppSettings[SettingName.Multiscreen].Value = multi.ToString();
                Save();
            }
        }

        /// <summary>
        /// If foreground is set in the options.
        /// </summary>
        /// <returns>true, if the pet should be in the foreground</returns>
        public bool GetWindowForeground()
        {
            bool.TryParse(AppSettings[SettingName.WinForeground].Value, out bool ret);
            return ret;
        }

		/// <summary>
		/// Set if the pet should be in the foreground (always on top)
		/// </summary>
		/// <param name="foreground">true, if the pet should be in the foreground</param>
		public void SetWindowForeground(bool foreground)
        {
            if (foreground.ToString() != AppSettings[SettingName.WinForeground].Value)
            {
                Properties.Settings.Default.WinForeground = foreground;
                AppSettings[SettingName.WinForeground].Value = foreground.ToString();
                Save();
            }
        }

		/// <summary>
		/// Taskbar has a high priority for the focus, so the pet will go to background on it. If this option is set, the pet will steal the focus from the taskbar and will be in the foreground.
        /// Note: this will have strange effects on Windows.
		/// </summary>
		/// <param name="steal">true, if the pet should steal the taskbar focus</param>
		public void SetStealTaskbarFocus(bool steal)
        {
            if (steal.ToString() != AppSettings[SettingName.StealTaskbarFocus].Value)
            {
                Properties.Settings.Default.WinForeground = steal;
                AppSettings[SettingName.StealTaskbarFocus].Value = steal.ToString();
                Save();
            }
        }

		/// <summary>
		/// Get if the pet should steal the taskbar focus. If this option is set, the pet will steal the focus from the taskbar and will be in the foreground.
		/// </summary>
		/// <returns>true, if the pet should steal the taskbar focus</returns>
		public bool GetStealTaskbarFocus()
        {
            bool.TryParse(AppSettings[SettingName.StealTaskbarFocus].Value, out bool ret);
            return ret;
        }

		/// <summary>
		/// Get the number of pets that should be started automatically. The default is 1.
		/// </summary>
		/// <returns>the number of pets to start automatically</returns>
		public int GetAutoStartPets()
        {
            int.TryParse(AppSettings[SettingName.AutostartPets].Value, out int ret);
            return Math.Max(1, ret);
        }

		/// <summary>
		/// Set the number of pets that should be started automatically. The default is 1.
		/// </summary>
		/// <param name="autostart">the number of pets to start automatically</param>
		public void SetAutoStartPets(int autostart)
        {
            if (autostart.ToString() != AppSettings[SettingName.AutostartPets].Value)
            {
                Properties.Settings.Default.AutostartPets = autostart;
                AppSettings[SettingName.AutostartPets].Value = autostart.ToString();
                Save();
            }
        }

        /// <summary>
        /// If the Movements of the pet should be smooth
        /// </summary>
        /// <returns>true, if movements should be smoother</returns>
        public bool GetSmoothMovements()
        {
			bool.TryParse(AppSettings[SettingName.SmoothMovements].Value, out bool ret);
			return ret;
		}

		/// <summary>
		/// To "serialize" the settings, every setting can be retrieved as integer
		/// </summary>
		/// <param name="setting">Name of the setting, <see cref="SettingName"/></param>
		/// <returns>Value of the setting, or 0</returns>
		public int GetIntValue(string setting)
        {
            if(AppSettings.AllKeys.Contains(setting) && setting != SettingName.Undefined)
            {
                if (AppSettings[setting] == null) // If setting is not present in the local machine, create it
                {
                    AppSettings.Add(setting, "0");
                }

                if(int.TryParse(AppSettings[setting].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ret1))
                {
                    return ret1;
                }
				else if (float.TryParse(AppSettings[setting].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret3))
				{
					return (int)(ret3 * 100);
				}
				else if(bool.TryParse(AppSettings[setting].Value, out bool ret2))
                {
                    return (ret2 ? 1 : 0);
                }
            }
            return 0;
        }

		/// <summary>
		/// Set a setting with key and value
		/// </summary>
		/// <param name="setting">Name of the setting, <see cref="SettingName"/></param>
		/// <param name="value">integer Value of the setting (checkbox: 0 or 1), (combobox:one of the values in the list)</param>
		/// <returns>true if setting was set successfully</returns>
		public bool SetIntValue(string setting, int value)
        {
			if (AppSettings.AllKeys.Contains(setting) && setting != SettingName.Undefined)
			{
				if (int.TryParse(AppSettings[setting].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ret1))
				{
                    try
                    {
                        Properties.Settings.Default[setting] = value;
                    }
                    catch(Exception)
                    {
						Properties.Settings.Default[setting] = (float)value; // we have floats and int (both can be parsed as int, but must be saved separately)
					}
					AppSettings[setting].Value = value.ToString(CultureInfo.InvariantCulture);
					Save();
					return true;
				}
				else if (bool.TryParse(AppSettings[setting].Value, out bool ret2))
				{
					Properties.Settings.Default[setting] = (value > 0);
					AppSettings[setting].Value = ((bool)(value > 0)).ToString(CultureInfo.InvariantCulture);
					Save();
					return true;
				}
			}
            return false;
		}

		/// <summary>
		/// Set the XML data for the pet. The XML is a string containing the entire pet animation.
		/// </summary>
		/// <param name="xml">xml as string</param>
		/// <param name="folder">is not used yet (should be removed)</param>
		public void SetXml(string xml, string folder)
        {
            Properties.Settings.Default.xml = xml;
            AppSettings["xml"].Value = xml;
            Save();
        }

		/// <summary>
		/// The XML data for the pet. The XML is a string containing the entire pet animation.
		/// </summary>
		/// <returns>xml as string</returns>
		public string GetXml()
        {
            return AppSettings["xml"].Value;
        }

		/// <summary>
		/// Load the XML data for the pet. The XML is a string containing the entire pet animation. The XML can be loaded from a local file, a web URL or from the settings.
		/// </summary>
		/// <returns>xml as string</returns>
		public string LoadXML()
        {
            //XmlSerializer mySerializer = new XmlSerializer(typeof(XmlData.RootNode));
            // To read the file, create a FileStream.
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

#if PORTABLE
            if (File.Exists(Application.StartupPath + "\\installpet.xml"))
            {
                string sXML = System.Text.Encoding.Default.GetString(File.ReadAllBytes(Application.StartupPath + "\\installpet.xml"));
                File.Delete(Application.StartupPath + "\\installpet.xml");
                writer.Write(sXML);
                SetXml(sXML, "");
                return sXML;
            }
            else if (Program.ArgumentLocalXML != "")
            {
                string sXML = System.Text.Encoding.Default.GetString(File.ReadAllBytes(Program.ArgumentLocalXML));
                writer.Write(sXML);
                return sXML;
            }
            else if (Program.ArgumentWebXML != "")
            {
                System.Net.WebClient client = new System.Net.WebClient();
                string sXML = client.DownloadString(Program.ArgumentWebXML);
                writer.Write(sXML);
                return sXML;
            }
            else
#endif
            {
                writer.Write(AppSettings["xml"].Value);
                return AppSettings["xml"].Value;
            }
        }

		/// <summary>
		/// Get the images from the pets.
		/// </summary>
		/// <returns>a base64 encoded string of the images</returns>
		public string GetImages()
        {
            return AppSettings["Images"].Value;
        }

		/// <summary>
		/// Set the images for the pets. The images are stored as a base64 encoded string.
		/// </summary>
		/// <param name="images">a base64 encoded string of the images</param>
		public void SetImages(string images)
        {
            Properties.Settings.Default.Images = images;
            AppSettings["Images"].Value = images;
            //Save();
        }

		/// <summary>
		/// Get the icon for the application. The icon is stored as a base64 encoded string.
		/// </summary>
		/// <returns>a base64 encoded string of the icon</returns>
		public string GetIcon()
        {
            return AppSettings["Icon"].Value;
        }

		/// <summary>
		/// Set the icon for the application. The icon is stored as a base64 encoded string.
		/// </summary>
		/// <param name="icon">a base64 encoded string of the icon</param>
		public void SetIcon(string icon)
        {
            Properties.Settings.Default.Icon = icon;
            AppSettings["Icon"].Value = icon;
            //Save();
        }

		/// <summary>
		/// Check if this is the first boot of the application. This is not implemented in the portable version.
		/// </summary>
		/// <returns>always returns false</returns>
		public bool IsFirstBoot()
        {
            return false;
        }
		/// <summary>
		/// Can list for changes, to reload the XML or the options. This is not implemented in the portable version.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public delegate void MyFunction(object source, FileSystemEventArgs e);

		/// <summary>
		/// Listen for changes in the XML file. This is not implemented in the portable version.
		/// </summary>
		/// <param name="f"></param>
		public void ListenOnXMLChanged(MyFunction f)
        {
            // not implemented in the portable version
        }

		/// <summary>
		/// Listen for changes in the options. This is not implemented in the portable version.
		/// </summary>
		/// <param name="f"></param>
		public void ListenOnOptionsChanged(MyFunction f)
        {
            // not implemented in the portable version
        }

        /// <summary>
        /// Save the settings in the option dialog.
        /// </summary>
        private void Save()
        {
            if (isInstalled)
            {
                Properties.Settings.Default.Save();
                AppConfiguration.Save();
            }
            else
            {
                AppConfiguration.Save();
            }
        }
    }
}

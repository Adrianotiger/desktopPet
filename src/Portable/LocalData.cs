using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Configuration;

namespace DesktopPet
{
    public class LocalData
    {
        Configuration AppConfiguration = null;
        KeyValueConfigurationCollection AppSettings = null;
        bool isInstalled = false;

        public LocalData()
        {
            try
            {
                if (Program.IsApplicationInstalled())
                {
                    isInstalled = true;
                    //AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                    AppConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
                        new ExeConfigurationFileMap { ExeConfigFilename = "DesktopPet.config" }, ConfigurationUserLevel.None);
                }
                else
                {
                    AppConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
                        new ExeConfigurationFileMap { ExeConfigFilename = "DesktopPet.config" }, ConfigurationUserLevel.None);
                }
                LoadSettings();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error opening settings: " + ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadSettings()
        {
            var settings = AppConfiguration.AppSettings.Settings;
            foreach (SettingsProperty currentProperty in Properties.Settings.Default.Properties)
            {
                if (AppConfiguration.AppSettings.Settings[currentProperty.Name] == null)
                {
                    AppConfiguration.AppSettings.Settings.Add(currentProperty.Name, currentProperty.DefaultValue.ToString());
                }
            }
            AppSettings = AppConfiguration.AppSettings.Settings;
        }

        public void SetVolume(double volume)
        {
            int iVolume = (int)(volume * 100);
            if (iVolume.ToString() != AppSettings["Volume"].Value)
            {
                Properties.Settings.Default.Volume = iVolume;
                AppSettings["Volume"].Value = iVolume.ToString();
                Save();
            }
        }
        public float GetVolume()
        {
            int iVolume = 30;
            int.TryParse(AppSettings["Volume"].Value, out iVolume);
            return (float)(iVolume / 100.0);
        }

        public bool GetMultiscreen()
        {
            bool.TryParse(AppSettings["Multiscreen"].Value, out bool ret);
            return ret;
        }

        public void SetMultiscreen(bool multi)
        {
            if (multi.ToString() != AppSettings["Multiscreen"].Value)
            {
                Properties.Settings.Default.Multiscreen = multi;
                AppSettings["Multiscreen"].Value = multi.ToString();
                Save();
            }
        }

        public bool GetWindowForeground()
        {
            bool.TryParse(AppSettings["WinForeground"].Value, out bool ret);
            return ret;
        }

        public void SetWindowForeground(bool foreground)
        {
            if (foreground.ToString() != AppSettings["WinForeground"].Value)
            {
                Properties.Settings.Default.WinForeground = foreground;
                AppSettings["WinForeground"].Value = foreground.ToString();
                Save();
            }
        }

        public int GetAutoStartPets()
        {
            int.TryParse(AppSettings["AutostartPets"].Value, out int ret);
            return Math.Max(1, ret);
        }

        public void SetAutoStartPets(int autostart)
        {
            if (autostart.ToString() != AppSettings["AutostartPets"].Value)
            {
                Properties.Settings.Default.AutostartPets = autostart;
                AppSettings["AutostartPets"].Value = autostart.ToString();
                Save();
            }
        }

        public void SetXml(string xml, string folder)
        {
            Properties.Settings.Default.xml = xml;
            AppSettings["xml"].Value = xml;
            Save();
        }

        public string GetXml()
        {
            return AppSettings["xml"].Value;
        }

        public string LoadXML()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(XmlData.RootNode));
            // To read the file, create a FileStream.
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

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
            {
                writer.Write(AppSettings["xml"].Value);
                return AppSettings["xml"].Value;
            }
        }

        public string GetImages()
        {
            return AppSettings["Images"].Value;
        }

        public void SetImages(string images)
        {
            Properties.Settings.Default.Images = images;
            AppSettings["Images"].Value = images;
            //Save();
        }

        public string GetIcon()
        {
            return AppSettings["Icon"].Value;
        }

        public void SetIcon(string icon)
        {
            Properties.Settings.Default.Icon = icon;
            AppSettings["Icon"].Value = icon;
            //Save();
        }

        public bool IsFirstBoot()
        {
            return false;
        }

        public delegate void MyFunction(object source, FileSystemEventArgs e);

        public void ListenOnXMLChanged(MyFunction f)
        {
            // not implemented in the portable version
        }

        public void ListenOnOptionsChanged(MyFunction f)
        {
            // not implemented in the portable version
        }

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

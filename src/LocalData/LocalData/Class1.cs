using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocalData
{
    public class LocalData
    {
        static Windows.Storage.ApplicationDataContainer LocalSettings = null;
        static Windows.Storage.StorageFolder LocalFolder = null;
        //static bool ValuesUpdated = false;

        private static string Images = "";
        private static string Icon = "";
        private static string Xml = "";
        private static double Volume = 0.3;
        private static bool WinForeGround = false;
        private static int AutostartPets = 1;

        public LocalData()
        {
            if(LocalSettings == null)
            {
                LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                LocalSettings.Values["Volume"] = 0.3;
                LocalSettings.Values["WinForeGround"] = false;
                LocalSettings.Values["AutostartPets"] = 1;

                if (LocalSettings.Values.ContainsKey("Volume"))
                    Volume = (double)LocalSettings.Values["Volume"];
                else
                    LocalSettings.Values["Volume"] = Volume;

                if (LocalSettings.Values.ContainsKey("WinForeGround"))
                    WinForeGround = (bool)LocalSettings.Values["WinForeGround"];
                else
                    LocalSettings.Values["WinForeGround"] = WinForeGround;

                if (LocalSettings.Values.ContainsKey("AutostartPets"))
                    AutostartPets = (int)LocalSettings.Values["AutostartPets"];
                else
                    LocalSettings.Values["AutostartPets"] = AutostartPets;

                LoadXML();
                LoadImages();
                LoadIcon();
            }
        }

        public void SetVolume(double volume)
        {
            if (volume != Volume)
            {
                Volume = volume;
                LocalSettings.Values["Volume"] = volume;
            }
        }

        public double GetVolume()
        {
            return Volume;
        }

        public void SetWindowForeground(bool setOnCollision)
        {
            if (WinForeGround != setOnCollision)
            {
                WinForeGround = setOnCollision;
                LocalSettings.Values["WinForeground"] = setOnCollision;
            }
        }

        public bool GetWindowForeground()
        {
            return WinForeGround;
        }

        public void SetAutoStartPets(int startingPets)
        {
            if (AutostartPets != startingPets)
            {
                AutostartPets = startingPets;
                LocalSettings.Values["AutostartPets"] = startingPets;
            }
        }

        public int GetAutoStartPets()
        {
            return AutostartPets;
        }

        public void SetXml(string newXml)
        {
            Xml = newXml;

            var buffer = Encoding.UTF8.GetBytes(newXml);
            var f = File.OpenWrite(LocalFolder.Path + "\\animation.xml");
            f.Write(buffer, 0, buffer.Length);
            f.Close();
        }

        public string GetXml()
        {
            return Xml;
        }

        private void LoadXML()
        {
            var buffer = new Byte[1024 * 64];
            if (!File.Exists(LocalFolder.Path + "\\animation.xml"))
            {
                var fs = File.Create(LocalFolder.Path + "\\animation.xml");
                fs.Close();
            }
            Xml = "";
            var f = File.OpenRead(LocalFolder.Path + "\\animation.xml");
            var bytesRead = 0;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Xml += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Read(buffer, 0, buffer.Length);
            f.Close();
        }

        public void SetIcon(string newIcon)
        {
            Icon = newIcon;

            var buffer = Encoding.UTF8.GetBytes(newIcon);
            var f = File.OpenWrite(LocalFolder.Path + "\\icon.xml");
            f.Write(buffer, 0, buffer.Length);
            f.Close();
        }

        public string GetIcon()
        {
            return Icon;
        }

        private void LoadIcon()
        {
            var buffer = new Byte[1024 * 64];
            if (!File.Exists(LocalFolder.Path + "\\icon.xml"))
            {
                var fs = File.Create(LocalFolder.Path + "\\icon.xml");
                fs.Close();
            }
            Icon = "";
            var f = File.OpenRead(LocalFolder.Path + "\\icon.xml");
            var bytesRead = 0;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Icon += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Read(buffer, 0, buffer.Length);
            f.Close();
        }

        public void SetImages(string newImages)
        {
            Images = newImages;

            var buffer = Encoding.UTF8.GetBytes(newImages);
            var f = File.OpenWrite(LocalFolder.Path + "\\images.xml");
            f.Write(buffer, 0, buffer.Length);
            f.Close();
        }

        public string GetImages()
        {
            return Images;
        }

        private void LoadImages()
        {
            var buffer = new Byte[1024 * 64];
            if (!File.Exists(LocalFolder.Path + "\\images.xml"))
            {
                var fs = File.Create(LocalFolder.Path + "\\images.xml");
                fs.Close();
            }
            Images = "";
            var f = File.OpenRead(LocalFolder.Path + "\\images.xml");
            var bytesRead = 0;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Images += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Read(buffer, 0, buffer.Length);
            f.Close();
        }

    }

}

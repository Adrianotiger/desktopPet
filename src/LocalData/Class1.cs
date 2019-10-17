using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;

namespace LocalData
{
    public class PetUpdate
    {
        public string Name { get; set; } = "";
        public DateTime Date { get; set; } = new DateTime(2000,1,1);
    }
    public class Settings
    {
        public double Volume { get; set; } = 0.3;
        public bool WinForeGround { get; set; } = false;
        public int AutostartPets { get; set; } = 1;
        public bool MultiScreenEnabled { get; set; } = true;
        public string CurrentPet { get; set; } = "esheep64";
        public List<PetUpdate> LastUpdate { get; set; } = new List<PetUpdate>();
    }

    public class LocalData
    {
        static Settings LocalSettings = null;
        static string LocalFolder;

        static bool FirstBoot = false;
        static string Images = "";
        static string Icon = "";
        static bool Developer = false;
        static string DeveloperPets = "";
        static string Xml = "";

        private FileSystemWatcher watcherXml = null;
        private FileSystemWatcher watcherJson = null;

        private static readonly String GITHUB_FOLDER = "https://raw.githubusercontent.com/Adrianotiger/desktopPet/master";
        public static readonly String GITHUB_PETDOCS = "https://adrianotiger.github.io/desktopPet/Pets/";
        private static readonly String GITHUB_PETFOLDER = "/Pets/";
        //public static readonly String GITHUB_APITREE = "https://api.github.com/repos/Adrianotiger/desktopPet/git/trees/9769cf227eaf8322c028d2be2a9671d692b9f293"; <<- can't be used without token/login

        public LocalData(string StorageFolder, string exeFile)
        {
            LocalFolder = StorageFolder;

            DeveloperPets = GITHUB_FOLDER + GITHUB_PETFOLDER + "pets.json";

            if (!Directory.Exists(LocalFolder)) Directory.CreateDirectory(LocalFolder);
            if (LocalSettings == null)
            {
                LoadSettings();

                LoadXML();
                LoadImages();
                LoadIcon();
            }
        }

        public void LoadSettings()
        {
            try
            {
                LocalSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(LocalFolder + "\\_settings_.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            if (LocalSettings == null)
            {
                LocalSettings = new Settings();
                FirstBoot = true;
            }
        }

        public bool IsFirstBoot()
        {
            return FirstBoot;
        }

        public void SetVolume(double volume)
        {
            if (volume != LocalSettings.Volume && Math.Abs(volume - LocalSettings.Volume) > 0.04)
            {
                LocalSettings.Volume = volume;
                SaveSettings();
            }
        }

        public double GetVolume()
        {
            return LocalSettings.Volume;
        }

        public void SetMultiscreen(bool enable)
        {
            if (enable != LocalSettings.MultiScreenEnabled)
            {
                LocalSettings.MultiScreenEnabled = enable;
                SaveSettings();
            }
        }

        public bool GetMultiscreen()
        {
            return LocalSettings.MultiScreenEnabled;
        }

        public void SetWindowForeground(bool setOnCollision)
        {
            if (LocalSettings.WinForeGround != setOnCollision)
            {
                LocalSettings.WinForeGround = setOnCollision;
                SaveSettings();
            }
        }

        public bool GetWindowForeground()
        {
            return LocalSettings.WinForeGround;
        }

        public void SetAutoStartPets(int startingPets)
        {
            if (LocalSettings.AutostartPets != startingPets)
            {
                LocalSettings.AutostartPets = startingPets;
                SaveSettings();
            }
        }

        public int GetAutoStartPets()
        {
            return LocalSettings.AutostartPets;
        }

        public void SetXml(string newXml, string folder)
        {
            if (Xml != newXml)
            {
                Xml = newXml;

                var buffer = Encoding.UTF8.GetBytes(newXml);
                File.Delete(LocalFolder + "\\animations.xml");

                var f = File.Create(LocalFolder + "\\animations.xml");
                f.Write(buffer, 0, buffer.Length);
                f.Close();

                LocalSettings.CurrentPet = folder;
                SaveSettings();
            }
        }

        public string GetCurrentPet()
        {
            return LocalSettings.CurrentPet;
        }

        public string GetXml()
        {
            return Xml;
        }

        public void LoadXML()
        {
            var buffer = new Byte[1024 * 64];
            if (!File.Exists(LocalFolder + "\\animations.xml"))
            {
                var fs = File.Create(LocalFolder + "\\animations.xml");
                fs.Close();
            }
            Xml = "";
            var f = File.OpenRead(LocalFolder + "\\animations.xml");
            int bytesRead;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Xml += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Close();
        }

        public void SetIcon(string newIcon)
        {
            Icon = newIcon;

            var buffer = Encoding.UTF8.GetBytes(newIcon);
            File.Delete(LocalFolder + "\\icon.xml");
            var f = File.Create(LocalFolder + "\\icon.xml");
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
            if (!File.Exists(LocalFolder + "\\icon.xml"))
            {
                var fs = File.Create(LocalFolder + "\\icon.xml");
                fs.Close();
            }
            Icon = "";
            var f = File.OpenRead(LocalFolder + "\\icon.xml");
            int bytesRead;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Icon += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Close();
        }

        public void SetImages(string newImages)
        {
            Images = newImages;

            var buffer = Encoding.UTF8.GetBytes(newImages);
            File.Delete(LocalFolder + "\\images.xml");
            var f = File.Create(LocalFolder + "\\images.xml");
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
            if (!File.Exists(LocalFolder + "\\images.xml"))
            {
                var fs = File.Create(LocalFolder + "\\images.xml");
                fs.Close();
            }
            Images = "";
            var f = File.OpenRead(LocalFolder + "\\images.xml");
            int bytesRead;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                Images += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Close();
        }

        public bool NeedToLoadNew(string petFolder, DateTimeOffset lastUpdate)
        {
            var itemFound = LocalSettings.LastUpdate.Find(item => item.Name == petFolder);
            if (itemFound != null)
            {
                return (itemFound.Date < lastUpdate);
            }
            else
            {
                return true;
            }
        }

        public void SavePetXML(string xml, string petName, DateTime lastUpdate)
        {
            var buffer = Encoding.UTF8.GetBytes(xml);
            var f = File.OpenWrite(LocalFolder + "\\pet_" + petName + ".xml");
            f.Write(buffer, 0, buffer.Length);
            f.Close();
            DateTimeOffset dto = lastUpdate;

            var itemFound = LocalSettings.LastUpdate.Find(item => item.Name == petName);
            if (itemFound != null) itemFound.Date = dto.DateTime;
            else LocalSettings.LastUpdate.Add(new PetUpdate { Date = dto.DateTime, Name = petName });
            SaveSettings();
        }

        public string GetPetXML(string petName)
        {
            string retXML = "";
            var buffer = new Byte[1024 * 64];
            var f = File.OpenRead(LocalFolder + "\\pet_" + petName + ".xml");
            int bytesRead;
            do
            {
                bytesRead = f.Read(buffer, 0, 1024 * 64);
                retXML += Encoding.UTF8.GetString(buffer, 0, bytesRead);
            } while (bytesRead > 0);
            f.Close();
            return retXML;
        }

        public delegate void MyFunction(object source, FileSystemEventArgs e);

        public void ListenOnXMLChanged(MyFunction f)
        {
            if (watcherXml == null)
            {
                watcherXml = new FileSystemWatcher
                {
                    Path = LocalFolder,
                    /* Watch for changes in LastAccess and LastWrite times, and 
                       the renaming of files or directories. */
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    // Only watch text files.
                    Filter = "animations.xml"
                };
            }
            else
            {
                watcherXml.EnableRaisingEvents = false;
            }

            // Add event handlers.
            watcherXml.Changed += new FileSystemEventHandler(f);

            // Begin watching.
            watcherXml.EnableRaisingEvents = true;
        }

        public void ListenOnOptionsChanged(MyFunction f)
        {
            if (watcherJson == null)
            {
                watcherJson = new FileSystemWatcher
                {
                    Path = LocalFolder,
                    /* Watch for changes in LastAccess and LastWrite times, and 
                       the renaming of files or directories. */
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    // Only watch text files.
                    Filter = "_settings_.json"
                };
            }
            else
            {
                watcherJson.EnableRaisingEvents = false;
            }

            // Add event handlers.
            watcherJson.Changed += new FileSystemEventHandler(f);

            // Begin watching.
            watcherJson.EnableRaisingEvents = true;
        }

        public void SetDeveloper(bool isDev)
        {
            Developer = isDev;
        }

        public bool IsDeveloper()
        {
            return Developer;
        }

        public void SetDeveloperGithubPets(string url)
        {
            DeveloperPets = url;
        }

        public string GetDeveloperGitHubPets()
        {
            return DeveloperPets;
        }

        public string GetGitHubPetListFile()
        {
            if(Developer)
            {
                return DeveloperPets;
            }
            else
            {
                return GITHUB_FOLDER + "/Pets/pets.json";
            }
        }

        public string GetGitHubPetFile(string petId)
        {
            if (Developer)
            {
                return DeveloperPets.Remove(DeveloperPets.LastIndexOf("/") + 1) + petId + "/animations.xml";
            }
            else
            {
                return GITHUB_FOLDER + "/Pets/" + petId + "/animations.xml";
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (watcherJson != null) watcherJson.EnableRaisingEvents = false;
                var output = JsonConvert.SerializeObject(LocalSettings);
                File.WriteAllText(LocalFolder + "\\_settings_.json", output);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (watcherJson != null) watcherJson.EnableRaisingEvents = true;
            }
        }
    }

}

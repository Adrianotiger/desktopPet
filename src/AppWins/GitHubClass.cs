using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace AppWins
{
    [DataContract]
    internal class GitHubPet
    {
        [DataMember]
        internal string folder { get; set; }

        [DataMember]
        internal string author { get; set; }

        [DataMember]
        internal string lastupdate { get { return updatedate.ToShortDateString(); } set { updatedate = DateTime.Parse(value); } }

        internal DateTime updatedate { get; set; }
    }

    [DataContract]
    internal class GitHubPets
    {
        [DataMember]
        internal GitHubPet[] pets { get; set; }
    }
    
    class GitHubClass
    {
        GitHubPets AllPets = new GitHubPets();
        List<string> PetsToLoad = new List<string>();

        LocalData.LocalData MyData;

        public GitHubClass()
        {
            MyData = new LocalData.LocalData();
        }

        public async Task LoadPetList()
        {
            var urlPets = LocalData.LocalData.GITHUB_PETLIST;
            WebClient client = new WebClient();
            try
            {
                var jsonPets = await client.DownloadStringTaskAsync(new Uri(urlPets));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonPets)))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GitHubPets));
                    AllPets = ser.ReadObject(ms) as GitHubPets;
                }

                AllPets.pets = AllPets.pets.OrderBy(x => x.lastupdate).ToArray();
            }
            catch (Exception ex)
            {
                int k = 0;
            }
        }

        public List<string> VerifyPets()
        {
            for(var k=0;k< AllPets.pets.Count();k++)
            {
                if(MyData.NeedToLoadNew(AllPets.pets[k].folder, AllPets.pets[k].updatedate))
                {
                    PetsToLoad.Add(AllPets.pets[k].folder);
                }
            }
            return PetsToLoad;
        }

        public async Task DownloadPet(string folder)
        {
            var urlPet = LocalData.LocalData.GITHUB_FOLDER + "/Pets/" + folder + "/animations.xml";

            WebClient client = new WebClient();
            try
            {
                var xmlPet = await client.DownloadStringTaskAsync(new Uri(urlPet));
                DateTime dt = DateTime.Now;
                foreach(var p in AllPets.pets)
                {
                    if(p.folder == folder)
                    {
                        dt = p.updatedate;
                        break;
                    }
                }
                MyData.SavePetXML(xmlPet, folder, dt);
            }
            catch (Exception ex)
            {
                int k = 0;
            }
        }

        public void FillSource(ref List<PetSelection> pets)
        {
            foreach (var p in AllPets.pets)
            {
                pets.Add(new PetSelection {
                    Author = p.author,
                    Folder = p.folder,
                    LastUpdate = p.updatedate,
                    IsLoading = true
                } );
            }
        }

        public async Task<PetSelection> FillData(string folder)
        {
            var p = new PetSelection();
            foreach (var pet in AllPets.pets)
            {
                if (pet.folder == folder)
                {
                    p.LastUpdate = pet.updatedate;
                    p.Author = pet.author;
                    p.Folder = pet.folder;
                    break;
                }
            }

            var xml = MyData.GetPetXML(folder);

            var xmlNode = LocalData.AnimationXML.ParseXML(xml);

            p.Animations = xmlNode.Animations.Animation.Length;
            p.Author = xmlNode.Header.Author;
            p.Childs = xmlNode.Childs.Child != null ? xmlNode.Childs.Child.Length : 0;
            p.Description = xmlNode.Header.Info;
            p.Image = new BitmapImage();
            byte[] value = Convert.FromBase64String(xmlNode.Header.Icon);
            using (InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream())
            {
                using (DataWriter dataWriter = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
                {
                    dataWriter.WriteBytes(value);
                    await dataWriter.StoreAsync();
                }
                await p.Image.SetSourceAsync(randomAccessStream);
            }
            p.PetName = xmlNode.Header.Petname;
            p.Sizekb = xml.Length / 1024;
            p.Spawns = xmlNode.Spawns.Spawn != null ? xmlNode.Spawns.Spawn.Length : 0;
            p.Title = xmlNode.Header.Title;
            p.Version = xmlNode.Header.Version;
            p.IsLoading = false;

            return p;
        }
        
    }
}

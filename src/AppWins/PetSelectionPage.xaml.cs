using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace OptionsWindow
{
    public class PetSelection
    {
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string PetName { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }

        public DateTime LastUpdate { get; set; }
        public int Animations { get; set; }
        public int Spawns { get; set; }
        public int Childs { get; set; }
        public bool IsLoading { get; set; }
        public int Sizekb { get; set; }
        public string ItemColor { get; set; }

        public BitmapImage Image { get; set; }
    }

    
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PetSelectionPage : Page
    {
        private GitHubClass GitHub = new GitHubClass();
        public List<PetSelection> Pets = new List<PetSelection>(16);
        static MainPage mainPage = null;

        public PetSelectionPage()
        {
            this.InitializeComponent();

            this.Loaded += PetSelectionPage_Loaded;
        }

        public void SetMainPage(MainPage page)
        {
            mainPage = page;
        }

        private async void PetSelectionPage_Loaded(object sender, RoutedEventArgs e)
        {
            petDescription.Visibility = Visibility.Collapsed;

            loadingText.Text = "Loading GitHub pet list...";
            await GitHub.LoadPetList();
            loadingBar.Value = 10;

            loadingText.Text = "Check new Pets...";
            var toLoad = GitHub.VerifyPets();
            loadingBar.Value = 20;

            for (var retry = 0; retry < 3; retry++)
            {
                for (var k = 0; k < toLoad.Count; k++)
                {
                    loadingText.Text = "Downloading " + (k + 1).ToString() + " from " + toLoad.Count.ToString();
                    loadingBar.Value = 20 + (k / toLoad.Count) * 40;
                    await GitHub.DownloadPet(toLoad[k]);
                }

                toLoad.Clear();

                GitHub.FillSource(ref Pets);

                PetsBox.ItemsSource = Pets;

                for (var k = 0; k < Pets.Count; k++)
                {
                    loadingText.Text = "Loading " + (k + 1).ToString() + " from " + Pets.Count.ToString();
                    loadingBar.Value = 60 + (k / Pets.Count) * 40;
                    var petId = Pets[k].Folder;
                    Pets[k] = await GitHub.FillData(petId);
                    if(Pets[k] == null)
                    {
                        toLoad.Add(petId);
                    }
                }

                if (toLoad.Count == 0)
                    break;
                else
                    Pets.Clear();
            }

            PetsBox.ItemsSource = null;

            loadingText.Text = "Loaded!";
            loadingPanel.Visibility = Visibility.Collapsed;

            PetsBox.ItemsSource = Pets;
        }

        private void PetsBox_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = e.ClickedItem as PetSelection;
            petImage.Source = s.Image;
            petAuthor.Text = s.Author;
            petTitle.Text = s.Title;
            petName.Text = s.PetName;
            petVersion.Text = s.Version;
            petUpdate.Text = s.LastUpdate.ToLongDateString();
            petSize.Text = s.Sizekb + " kb";
            petRating.Text = "";
            int rating = (int)Math.Max(0, Math.Min(7, Math.Sqrt(s.Animations) / 2));
            rating += (int)(Math.Max(0, Math.Min(2, s.Spawns / 10)));
            rating += (int)(Math.Max(0, Math.Min(1, s.Childs / 3)));
            for(var k=0;k<rating;k++)
            {
                petRating.Text += "\u2B50";
            }
            var info = s.Description.Replace("[br]", "\n");
            info = Regex.Replace(info, @"\[link:(.*)\]", "$1");
            petInfo.Text = info;
            petInstall.Tag = s.Folder;

            petDescription.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(var p in Pets)
            {
                if(p.Folder == (sender as Button).Tag.ToString())
                {
                    App.MyData.SetXml(App.MyData.GetPetXML(p.Folder), p.Folder);
                    petDescription.Visibility = Visibility.Collapsed;
                    mainPage.RemoveOptionPage();
                    break;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            petDescription.Visibility = Visibility.Collapsed;
        }
    }
}

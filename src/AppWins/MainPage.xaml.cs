using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.AppService;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace OptionsWindow
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Dictionary<string, Page> contentPages = new Dictionary<string, Page>();

        public static AppServiceConnection Connection = null;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(480, 320);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 100));
        }
                
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter.ToString() == "options")
            {
                Page newPage = new PetOptionsPage();
                myContent.Children.Add(newPage);
                contentPages.Add("Pet options", newPage);
            }
            else if (e.Parameter.ToString() == "help")
            {
                Page newPage = new HelpPage();
                myContent.Children.Add(newPage);
                contentPages.Add("Help", newPage);
            }
            else if (e.Parameter.ToString() == "about")
            {
                Page newPage = new PetAboutPage();
                myContent.Children.Add(newPage);
                contentPages.Add("Pet info", newPage);
            }
        }

        public void RemoveOptionPage()
        {
            if (contentPages.ContainsKey("Pet options"))
            {
                myContent.Children.Remove(contentPages["Pet options"]);
                contentPages.Remove("Pet options");
            }
            if (contentPages.ContainsKey("Pet info"))
            {
                myContent.Children.Remove(contentPages["Pet info"]);
                contentPages.Remove("Pet info");
            }
        }
        
        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                
            }
            else if (args.InvokedItem != null)
            {
                var navItemTag = args.InvokedItem.ToString();

                for(var k=0;k<contentPages.Count;k++)
                {
                    contentPages.ElementAt(k).Value.Visibility = Visibility.Collapsed;
                }
                
                if (contentPages.ContainsKey(navItemTag))
                {
                    contentPages[navItemTag].Visibility = Visibility.Visible;
                }
                else
                {
                    Page newPage = null;

                    switch (navItemTag)
                    {
                        case "Pet selection":
                            newPage = new PetSelectionPage();
                            ((PetSelectionPage)(newPage)).SetMainPage(this);
                            break;
                        case "Pet options":
                            newPage = new PetOptionsPage();
                            break;
                        case "Pet info":
                            newPage = new PetAboutPage();
                            break;
                        case "Application settings":
                            newPage = new AppOptionsPage();
                            break;
                        case "About":
                            newPage = new AboutPage();
                            break;
                        case "Help":
                            newPage = new HelpPage();
                            break;
                    }

                    if (newPage != null)
                    {
                        myContent.Children.Add(newPage);

                        contentPages.Add(navItemTag, newPage);
                    }
                }
            }
        }
    }
}

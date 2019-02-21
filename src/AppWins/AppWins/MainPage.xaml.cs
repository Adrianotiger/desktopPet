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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace AppWins
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Dictionary<string, Page> contentPages = new Dictionary<string, Page>();

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(480, 320);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 100));
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
                            break;
                        case "Pet options":
                            newPage = new PetOptionsPage();
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

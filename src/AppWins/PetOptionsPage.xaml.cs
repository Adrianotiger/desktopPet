using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace OptionsWindow
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PetOptionsPage : Page
    {
        public PetOptionsPage()
        {
            this.InitializeComponent();

            this.Loaded += PetOptionsPage_Loaded;
        }

        private void PetOptionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var xmlNode = XmlData.AnimationXML.ParseXML(App.MyData.GetXml());

                if (xmlNode ==null || xmlNode.Sounds == null || xmlNode.Sounds.Sound == null || xmlNode.Sounds.Sound.Length == 0)
                {
                    volumeInfo1.Visibility = Visibility.Visible;
                    volumeSlider.IsEnabled = false;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error reading XML file: " + ex.Message);
                volumeInfo1.Visibility = Visibility.Visible;
                volumeSlider.IsEnabled = false;
            }

            volumeSlider.Value = App.MyData.GetVolume();
            foregroundWindowToggle.IsOn = App.MyData.GetWindowForeground();
            multiScreenToggle.IsOn = App.MyData.GetMultiscreen();

            volumeSlider.Header = ((int)(volumeSlider.Value * 100)).ToString() + " %";

            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            foregroundWindowToggle.Toggled += ForegroundWindowToggle_Toggled;
            multiScreenToggle.Toggled += MultiScreenToggle_Toggled;
        }

        private void MultiScreenToggle_Toggled(object sender, RoutedEventArgs e)
        {
            App.MyData.SetMultiscreen(multiScreenToggle.IsOn);
        }

        private void ForegroundWindowToggle_Toggled(object sender, RoutedEventArgs e)
        {
            App.MyData.SetWindowForeground(foregroundWindowToggle.IsOn);
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if ((int)(e.OldValue * 100) != (int)(e.NewValue * 100))
            {
                App.MyData.SetVolume(volumeSlider.Value);
                volumeSlider.Header = ((int)(volumeSlider.Value * 100)).ToString() + " %";
            }
        }
    }
}

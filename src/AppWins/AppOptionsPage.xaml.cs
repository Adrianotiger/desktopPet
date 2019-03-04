using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace OptionsWindow
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AppOptionsPage : Page
    {
        public AppOptionsPage()
        {
            this.InitializeComponent();

            developerPanel.Visibility = Visibility.Collapsed;

            this.Loaded += AppOptionsPage_Loaded;
        }

        private async void AppOptionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartupTask startupTask = await StartupTask.GetAsync("eSheepId");

            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    // Task is disabled but can be enabled.
                    autostartToggle.IsEnabled = true;
                    taskText.Visibility = Visibility.Collapsed;
                    break;
                case StartupTaskState.DisabledByUser:
                    autostartToggle.IsOn = false;
                    autostartToggle.IsEnabled = false;
                    taskText.Visibility = Visibility.Visible;
                    break;
                case StartupTaskState.DisabledByPolicy:
                    taskText.Visibility = Visibility.Visible;
                    autostartToggle.IsEnabled = false;
                    (sender as ToggleSwitch).IsOn = false;
                    break;
                case StartupTaskState.Enabled:
                    autostartToggle.IsOn = true;
                    autostartToggle.IsEnabled = false;
                    Debug.WriteLine("Startup is enabled.");
                    break;
            }

            petsQuantitySlider.Value = App.MyData.GetAutoStartPets();

            petsQuantitySlider.Header = petsQuantitySlider.Value;

            autostartToggle.Toggled += AutostartToggle_Toggled;
            petsQuantitySlider.ValueChanged += PetsQuantitySlider_ValueChanged;
        }

        private void PetsQuantitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            App.MyData.SetAutoStartPets((int)petsQuantitySlider.Value);
            petsQuantitySlider.Header = ((int)petsQuantitySlider.Value).ToString();
        }

        private async void AutostartToggle_Toggled(object sender, RoutedEventArgs e)
        {
            StartupTask startupTask = await StartupTask.GetAsync("eSheepId");

            if ((sender as ToggleSwitch).IsOn)
            {
                switch (startupTask.State)
                {
                    case StartupTaskState.Disabled:
                        // Task is disabled but can be enabled.
                        StartupTaskState newState = await startupTask.RequestEnableAsync();
                        Debug.WriteLine("Request to enable startup, result = {0}", newState);
                        if (newState != StartupTaskState.Enabled) (sender as ToggleSwitch).IsOn = false;
                        break;
                    case StartupTaskState.DisabledByUser:
                        // Task is disabled and user must enable it manually.
                        MessageDialog dialog = new MessageDialog(
                            "You have disabled this app's ability to run " +
                            "as soon as you sign in, but if you change your mind, " +
                            "you can enable this in the Startup tab in Task Manager.",
                            "eSheep Startup");
                        await dialog.ShowAsync();
                        (sender as ToggleSwitch).IsOn = false;
                        break;
                    case StartupTaskState.DisabledByPolicy:
                        Debug.WriteLine(
                            "Startup disabled by group policy, or not supported on this device");
                        (sender as ToggleSwitch).IsOn = false;
                        break;
                    case StartupTaskState.Enabled:
                        Debug.WriteLine("Startup is enabled.");
                        break;
                }
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            developerPanel.Visibility = (sender as ToggleSwitch).IsOn ? Visibility.Visible : Visibility.Collapsed;
            App.MyData.SetDeveloper((sender as ToggleSwitch).IsOn);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.MyData.SetDeveloperGithubPets((sender as TextBox).Text);
        }
    }
}

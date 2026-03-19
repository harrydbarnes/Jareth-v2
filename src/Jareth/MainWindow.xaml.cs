using System;
using Jareth.Core.Models;
using Jareth.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Jareth;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();

        // Apply Mica backdrop
        SystemBackdrop = new MicaBackdrop();

        // Set window size
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new SizeInt32(1280, 800));

        _ = ViewModel.InitialiseAsync();
    }

    private void NewRecordingButton_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.RecordingPage));
    }

    private void FolderListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is MeetingFolder folder)
        {
            _ = ViewModel.LoadMeetingsForFolderAsync(folder.Name);
        }
    }

    private void MeetingListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Meeting meeting)
        {
            ContentFrame.Navigate(typeof(Views.MeetingDetailPage), meeting);
        }
    }

    private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "New Folder",
            Content = new TextBox { PlaceholderText = "Folder name", MinWidth = 280 },
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var textBox = (TextBox)dialog.Content;
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                await ViewModel.CreateFolderAsync(textBox.Text.Trim());
            }
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.SettingsPage));
    }
}

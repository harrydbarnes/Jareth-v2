using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jareth.Core.Models;
using Jareth.ViewModels;

namespace Jareth;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();

        ViewModel = App.Services.GetRequiredService<MainViewModel>();

        // Try to set Mica backdrop
        TrySetMicaBackdrop();

        // Load data
        _ = ViewModel.InitializeAsync();
    }

    private void TrySetMicaBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            var micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            this.SystemBackdrop = micaBackdrop;
        }
    }

    private void ShowView(string viewName)
    {
        WelcomePage.Visibility = viewName == "Welcome" ? Visibility.Visible : Visibility.Collapsed;
        RecordingPage.Visibility = viewName == "Recording" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = viewName == "Settings" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnNewRecordingClick(object sender, RoutedEventArgs e)
    {
        ShowView("Recording");
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        ShowView("Settings");
    }

    private async void OnNewFolderClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "New Folder",
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };

        var textBox = new TextBox { PlaceholderText = "Folder name" };
        dialog.Content = textBox;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await ViewModel.CreateFolderAsync(textBox.Text.Trim());
        }
    }

    private async void OnFolderSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowView("Welcome");
        await ViewModel.LoadMeetingsAsync();
    }

    private async void OnRenameFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is Folder folder)
        {
            if (folder.Name == "Uncategorised") return;

            var dialog = new ContentDialog
            {
                Title = "Rename Folder",
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var textBox = new TextBox { Text = folder.Name };
            dialog.Content = textBox;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                await ViewModel.RenameFolderAsync(folder.Id, textBox.Text.Trim());
            }
        }
    }

    private async void OnDeleteFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is Folder folder)
        {
            if (folder.Name == "Uncategorised") return;

            var dialog = new ContentDialog
            {
                Title = "Delete Folder",
                Content = $"Are you sure you want to delete '{folder.Name}'? Meetings will be moved to Uncategorised.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteFolderAsync(folder.Id);
            }
        }
    }
}

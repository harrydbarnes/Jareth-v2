using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jareth.ViewModels;

namespace Jareth.Views;

public sealed partial class SettingsPage : UserControl
{
    private SettingsViewModel? _viewModel;

    public SettingsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = App.Services.GetRequiredService<SettingsViewModel>();
        await _viewModel.LoadAsync();

        // Populate microphone list
        MicrophoneSelector.ItemsSource = _viewModel.AvailableMicrophones;
        if (!string.IsNullOrEmpty(_viewModel.SelectedMicrophone))
        {
            MicrophoneSelector.SelectedItem = _viewModel.SelectedMicrophone;
        }

        // Set audio source
        foreach (ComboBoxItem item in DefaultAudioSourceSelector.Items)
        {
            if (item.Tag?.ToString() == _viewModel.DefaultAudioSource)
            {
                DefaultAudioSourceSelector.SelectedItem = item;
                break;
            }
        }

        // Set theme
        foreach (ComboBoxItem item in ThemeSelector.Items)
        {
            if (item.Tag?.ToString() == _viewModel.Theme)
            {
                ThemeSelector.SelectedItem = item;
                break;
            }
        }

        // Set font
        foreach (ComboBoxItem item in FontSelector.Items)
        {
            if (item.Tag?.ToString() == _viewModel.Font)
            {
                FontSelector.SelectedItem = item;
                break;
            }
        }

        // Set API keys
        OpenAiKeyBox.Password = _viewModel.OpenAiApiKey;
        AnthropicKeyBox.Password = _viewModel.AnthropicApiKey;
        GeminiKeyBox.Password = _viewModel.GeminiApiKey;
        LocalModelEndpointBox.Text = _viewModel.LocalModelEndpoint;
        WhisperKeyBox.Password = _viewModel.WhisperApiKey;

        // Set storage path
        StoragePathText.Text = _viewModel.StoragePath;

        // Set version
        VersionText.Text = $"Version {_viewModel.AppVersion}";
    }

    private async void OnSaveSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.SelectedMicrophone = MicrophoneSelector.SelectedItem as string ?? string.Empty;
        _viewModel.DefaultAudioSource = (DefaultAudioSourceSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "mic";
        _viewModel.Theme = (ThemeSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "System";
        _viewModel.Font = (FontSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Roboto";
        _viewModel.LocalModelEndpoint = LocalModelEndpointBox.Text;

        await _viewModel.SaveAsync(
            OpenAiKeyBox.Password,
            AnthropicKeyBox.Password,
            GeminiKeyBox.Password,
            WhisperKeyBox.Password);

        // Show saved confirmation
        var dialog = new ContentDialog
        {
            Title = "Settings Saved",
            Content = "Your settings have been saved.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _ = Windows.System.Launcher.LaunchFolderPathAsync(_viewModel.StoragePath);
        }
    }
}

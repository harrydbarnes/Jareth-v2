using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jareth.Core.Helpers;
using Jareth.Core.Models;
using Jareth.ViewModels;

namespace Jareth.Views;

public sealed partial class RecordingPage : UserControl
{
    private RecordingViewModel? _viewModel;

    public RecordingPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = App.Services.GetRequiredService<RecordingViewModel>();

        // Show greeting
        GreetingText.Text = GreetingHelper.GetGreeting(0);

        // Populate microphone list
        _viewModel.RefreshMicrophones();
        MicrophoneCombo.ItemsSource = _viewModel.AvailableMicrophones;
        if (_viewModel.AvailableMicrophones.Count > 0)
        {
            MicrophoneCombo.SelectedIndex = 0;
        }

        // Subscribe to events
        _viewModel.TimerUpdated += OnTimerUpdated;
        _viewModel.LevelUpdated += OnLevelUpdated;
        _viewModel.RecordingCompleted += OnRecordingCompleted;
    }

    private async void OnRecordButtonClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        if (!_viewModel.IsRecording)
        {
            var title = string.IsNullOrWhiteSpace(MeetingTitleBox.Text)
                ? $"Meeting {DateTime.Now:yyyy-MM-dd HH-mm}"
                : MeetingTitleBox.Text;

            var source = AudioSource.Microphone;
            if (SystemOnlyRadio.IsChecked == true) source = AudioSource.System;
            else if (BothRadio.IsChecked == true) source = AudioSource.Both;

            var micName = MicrophoneCombo.SelectedItem as string;

            AudioSourceLabel.Text = source switch
            {
                AudioSource.Microphone => "Mic Only",
                AudioSource.System => "System Audio",
                AudioSource.Both => "Both (Mixed)",
                _ => "Unknown"
            };

            await _viewModel.StartRecordingAsync(title, source, micName);

            RecordButtonText.Text = "Stop Recording";
            RecordIcon.Glyph = "\uE71A";
            RecordButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Red);
        }
        else
        {
            await _viewModel.StopRecordingAsync();

            RecordButtonText.Text = "Start Recording";
            RecordIcon.Glyph = "\uE7C8";
            RecordButton.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["JarethAccentBrush"];
            TimerText.Text = "00:00:00";
            LevelMeter.Value = 0;
        }
    }

    private void OnTimerUpdated(object? sender, TimeSpan elapsed)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TimerText.Text = elapsed.ToString(@"hh\:mm\:ss");
        });
    }

    private void OnLevelUpdated(object? sender, float level)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            LevelMeter.Value = level * 100;
        });
    }

    private void OnRecordingCompleted(object? sender, Meeting meeting)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Reset UI
            MeetingTitleBox.Text = string.Empty;
        });
    }
}

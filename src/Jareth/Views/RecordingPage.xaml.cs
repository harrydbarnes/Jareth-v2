using System;
using Jareth.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Jareth.Views;

public sealed partial class RecordingPage : Page
{
    public RecordingViewModel ViewModel { get; }

    public RecordingPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<RecordingViewModel>();
        ViewModel.SetDispatcherQueue(DispatcherQueue);
    }

    private async void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsRecording)
            await ViewModel.StopRecordingAsync();
        else
            await ViewModel.StartRecordingAsync();
    }

    public double GetPeakBarWidth(float level)
        => Math.Clamp(level, 0f, 1f) * 280.0;

    public string GetRecordIcon(bool isRecording)
        => isRecording ? "\uE71A" : "\uE717";
}

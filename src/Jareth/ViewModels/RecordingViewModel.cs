using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jareth.Core.Helpers;
using Jareth.Core.Models;
using Jareth.Core.Services;
using Microsoft.UI.Dispatching;

namespace Jareth.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IAudioService _audio;
    private readonly IStorageService _storage;
    private readonly IDatabaseService _db;
    private readonly ISettingsService _settings;

    private CancellationTokenSource? _cts;
    private System.Diagnostics.Stopwatch? _stopwatch;
    private DispatcherTimer? _timer;
    private Meeting? _currentMeeting;
    private DispatcherQueue? _dispatcherQueue;

    [ObservableProperty] private string _greeting = string.Empty;
    [ObservableProperty] private string _meetingTitle = "Untitled Meeting";
    [ObservableProperty] private string _selectedAudioSource = "Both";
    [ObservableProperty] private string _elapsedTime = "00:00:00";
    [ObservableProperty] private float _peakLevel;
    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private System.Collections.ObjectModel.ObservableCollection<string> _microphoneDevices = new();
    [ObservableProperty] private string _selectedMicrophone = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public string[] AudioSourceOptions { get; } = { "Mic Only", "System Only", "Both" };

    public RecordingViewModel(IAudioService audio, IStorageService storage,
        IDatabaseService db, ISettingsService settings)
    {
        _audio = audio;
        _storage = storage;
        _db = db;
        _settings = settings;

        _audio.PeakLevelChanged += (s, level) =>
        {
            _dispatcherQueue?.TryEnqueue(() => PeakLevel = level);
        };

        LoadGreeting();
        LoadMicrophoneDevices();
        SelectedAudioSource = _settings.Settings.DefaultAudioSource;
    }

    public void SetDispatcherQueue(DispatcherQueue queue) => _dispatcherQueue = queue;

    private void LoadGreeting()
    {
        // Count meetings today — simplified: use 0 for now
        Greeting = GreetingHelper.GetGreeting(DateTime.Now, 0);
    }

    private void LoadMicrophoneDevices()
    {
        MicrophoneDevices.Clear();
        foreach (var d in _audio.GetMicrophoneDevices())
            MicrophoneDevices.Add(d);

        if (!string.IsNullOrEmpty(_settings.Settings.DefaultMicrophoneDevice))
            SelectedMicrophone = _settings.Settings.DefaultMicrophoneDevice;
        else if (MicrophoneDevices.Count > 0)
            SelectedMicrophone = MicrophoneDevices[0];
    }

    [RelayCommand]
    public async Task StartRecordingAsync()
    {
        if (IsRecording) return;

        StatusMessage = "Starting...";
        _currentMeeting = await _storage.CreateMeetingFolderAsync(
            MeetingTitle, SelectedAudioSource, "Uncategorised");

        var audioPath = _storage.GetAudioFilePath(_currentMeeting.FolderPath);
        var mode = SelectedAudioSource switch
        {
            "Mic Only" => AudioSourceMode.MicOnly,
            "System Only" => AudioSourceMode.SystemOnly,
            _ => AudioSourceMode.Both
        };

        _cts = new CancellationTokenSource();
        await _audio.StartRecordingAsync(audioPath, mode, _cts.Token);

        IsRecording = true;
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += (s, e) =>
        {
            ElapsedTime = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        };
        _timer.Start();
        StatusMessage = "Recording...";
    }

    [RelayCommand]
    public async Task StopRecordingAsync()
    {
        if (!IsRecording || _currentMeeting == null) return;

        StatusMessage = "Stopping...";
        _timer?.Stop();
        _cts?.Cancel();
        await _audio.StopRecordingAsync();

        _currentMeeting.DurationSeconds = (int)(_stopwatch?.Elapsed.TotalSeconds ?? 0);
        await _storage.SaveMetadataAsync(_currentMeeting);
        await _db.IndexMeetingAsync(_currentMeeting);

        IsRecording = false;
        StatusMessage = "Saved.";
        ElapsedTime = "00:00:00";
    }
}

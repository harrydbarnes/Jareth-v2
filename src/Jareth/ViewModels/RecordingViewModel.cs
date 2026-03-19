using Jareth.Core.Models;
using Jareth.Core.Services;

namespace Jareth.ViewModels;

public class RecordingViewModel
{
    private readonly IAudioService _audioService;
    private readonly IStorageService _storageService;
    private readonly IDatabaseService _databaseService;
    private string _currentMeetingFolder = string.Empty;
    private string _currentTitle = string.Empty;
    private AudioSource _currentAudioSource;
    private DateTime _recordingStartTime;

    public bool IsRecording => _audioService.IsRecording;
    public IReadOnlyList<string> AvailableMicrophones => _audioService.AvailableMicrophones;

    public event EventHandler<TimeSpan>? TimerUpdated;
    public event EventHandler<float>? LevelUpdated;
    public event EventHandler<Meeting>? RecordingCompleted;

    public RecordingViewModel(IAudioService audioService, IStorageService storageService, IDatabaseService databaseService)
    {
        _audioService = audioService;
        _storageService = storageService;
        _databaseService = databaseService;

        _audioService.ElapsedTimeChanged += (s, e) => TimerUpdated?.Invoke(this, e);
        _audioService.PeakLevelChanged += (s, e) => LevelUpdated?.Invoke(this, e);
    }

    public void RefreshMicrophones()
    {
        _audioService.RefreshMicrophones();
    }

    public async Task StartRecordingAsync(string title, AudioSource source, string? microphoneName = null)
    {
        _currentTitle = title;
        _currentAudioSource = source;
        _currentMeetingFolder = await _storageService.CreateMeetingFolderAsync(title);
        var audioPath = Path.Combine(_currentMeetingFolder, "audio.wav");

        _recordingStartTime = DateTime.Now;
        await _audioService.StartRecordingAsync(audioPath, source, microphoneName);
    }

    public async Task StopRecordingAsync()
    {
        await _audioService.StopRecordingAsync();

        var duration = DateTime.Now - _recordingStartTime;
        var meeting = new Meeting
        {
            Title = _currentTitle,
            Date = _recordingStartTime,
            DurationSeconds = (int)duration.TotalSeconds,
            AudioSource = _currentAudioSource.ToString().ToLowerInvariant(),
            FolderPath = _currentMeetingFolder,
            FolderCategory = "Uncategorised"
        };

        await _storageService.SaveMetadataAsync(meeting);
        await _databaseService.UpsertMeetingAsync(meeting);

        RecordingCompleted?.Invoke(this, meeting);
    }
}

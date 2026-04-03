using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface IAudioService
{
    bool IsRecording { get; }
    TimeSpan ElapsedTime { get; }
    float CurrentPeakLevel { get; }
    IReadOnlyList<string> AvailableMicrophones { get; }

    event EventHandler<float>? PeakLevelChanged;
    event EventHandler<TimeSpan>? ElapsedTimeChanged;
    event EventHandler? RecordingStarted;
    event EventHandler? RecordingStopped;

    void RefreshMicrophones();
    Task StartRecordingAsync(string outputPath, AudioSource source, string? microphoneDeviceName = null, CancellationToken cancellationToken = default);
    Task StopRecordingAsync();
}

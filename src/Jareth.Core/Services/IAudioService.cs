using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jareth.Core.Services;

public enum AudioSourceMode
{
    MicOnly,
    SystemOnly,
    Both
}

public interface IAudioService
{
    bool IsRecording { get; }
    float CurrentPeakLevel { get; }
    event EventHandler<float>? PeakLevelChanged;
    Task StartRecordingAsync(string outputFilePath, AudioSourceMode mode, CancellationToken cancellationToken = default);
    Task StopRecordingAsync();
    IEnumerable<string> GetMicrophoneDevices();
}

using NAudio.Wave;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public class AudioService : IAudioService
{
    private WaveInEvent? _micCapture;
    private WasapiLoopbackCapture? _systemCapture;
    private WaveFileWriter? _waveWriter;
    private CancellationTokenSource? _timerCts;
    private DateTime _recordingStartTime;
    private readonly object _writerLock = new();

    public bool IsRecording { get; private set; }
    public TimeSpan ElapsedTime { get; private set; }
    public float CurrentPeakLevel { get; private set; }
    public IReadOnlyList<string> AvailableMicrophones { get; private set; } = Array.Empty<string>();

    public event EventHandler<float>? PeakLevelChanged;
    public event EventHandler<TimeSpan>? ElapsedTimeChanged;
    public event EventHandler? RecordingStarted;
    public event EventHandler? RecordingStopped;

    public void RefreshMicrophones()
    {
        var devices = new List<string>();
        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var caps = WaveInEvent.GetCapabilities(i);
            devices.Add(caps.ProductName);
        }
        AvailableMicrophones = devices.AsReadOnly();
    }

    public async Task StartRecordingAsync(string outputPath, AudioSource source, string? microphoneDeviceName = null, CancellationToken cancellationToken = default)
    {
        if (IsRecording) return;

        var targetFormat = new WaveFormat(44100, 16, 2);

        lock (_writerLock)
        {
            _waveWriter = new WaveFileWriter(outputPath, targetFormat);
        }

        switch (source)
        {
            case AudioSource.Microphone:
                StartMicrophoneCapture(targetFormat, microphoneDeviceName);
                break;
            case AudioSource.System:
                StartSystemCapture(targetFormat);
                break;
            case AudioSource.Both:
                StartMicrophoneCapture(targetFormat, microphoneDeviceName);
                StartSystemCapture(targetFormat);
                break;
        }

        IsRecording = true;
        _recordingStartTime = DateTime.Now;
        RecordingStarted?.Invoke(this, EventArgs.Empty);

        _timerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = UpdateTimerAsync(_timerCts.Token);
    }

    public Task StopRecordingAsync()
    {
        if (!IsRecording) return Task.CompletedTask;

        _timerCts?.Cancel();

        _micCapture?.StopRecording();
        _micCapture?.Dispose();
        _micCapture = null;

        _systemCapture?.StopRecording();
        _systemCapture?.Dispose();
        _systemCapture = null;

        lock (_writerLock)
        {
            _waveWriter?.Dispose();
            _waveWriter = null;
        }

        IsRecording = false;
        RecordingStopped?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    private void StartMicrophoneCapture(WaveFormat targetFormat, string? deviceName)
    {
        int deviceIndex = 0;
        if (!string.IsNullOrEmpty(deviceName))
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var caps = WaveInEvent.GetCapabilities(i);
                if (caps.ProductName == deviceName)
                {
                    deviceIndex = i;
                    break;
                }
            }
        }

        _micCapture = new WaveInEvent
        {
            DeviceNumber = deviceIndex,
            WaveFormat = targetFormat,
            BufferMilliseconds = 100
        };

        _micCapture.DataAvailable += OnAudioDataAvailable;
        _micCapture.StartRecording();
    }

    private void StartSystemCapture(WaveFormat targetFormat)
    {
        _systemCapture = new WasapiLoopbackCapture();

        _systemCapture.DataAvailable += (sender, e) =>
        {
            if (e.BytesRecorded == 0) return;

            // Convert system audio format to target format
            var sourceFormat = _systemCapture.WaveFormat;
            using var sourceStream = new RawSourceWaveStream(
                new MemoryStream(e.Buffer, 0, e.BytesRecorded),
                sourceFormat);

            // Resample if needed
            if (sourceFormat.SampleRate != targetFormat.SampleRate ||
                sourceFormat.BitsPerSample != targetFormat.BitsPerSample ||
                sourceFormat.Channels != targetFormat.Channels)
            {
                using var resampler = new MediaFoundationResampler(sourceStream, targetFormat);
                resampler.ResamplerQuality = 60;
                var buffer = new byte[e.BytesRecorded * 2];
                int bytesRead;
                while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    WriteToFile(buffer, bytesRead);
                    UpdatePeakLevel(buffer, bytesRead);
                }
            }
            else
            {
                WriteToFile(e.Buffer, e.BytesRecorded);
                UpdatePeakLevel(e.Buffer, e.BytesRecorded);
            }
        };

        _systemCapture.StartRecording();
    }

    private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded == 0) return;

        WriteToFile(e.Buffer, e.BytesRecorded);
        UpdatePeakLevel(e.Buffer, e.BytesRecorded);
    }

    private void WriteToFile(byte[] buffer, int bytesRecorded)
    {
        lock (_writerLock)
        {
            _waveWriter?.Write(buffer, 0, bytesRecorded);
        }
    }

    private void UpdatePeakLevel(byte[] buffer, int bytesRecorded)
    {
        float max = 0;
        for (int i = 0; i < bytesRecorded - 1; i += 2)
        {
            short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
            float sampleValue = Math.Abs(sample / 32768f);
            if (sampleValue > max) max = sampleValue;
        }
        CurrentPeakLevel = max;
        PeakLevelChanged?.Invoke(this, max);
    }

    private async Task UpdateTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ElapsedTime = DateTime.Now - _recordingStartTime;
                ElapsedTimeChanged?.Invoke(this, ElapsedTime);
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when recording stops
        }
    }
}

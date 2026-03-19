using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Jareth.Core.Services;

public class AudioService : IAudioService
{
    private WaveFileWriter? _writer;
    private IWaveIn? _micCapture;
    private WasapiLoopbackCapture? _loopbackCapture;
    private volatile bool _isRecording;
    private float _peakLevel;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public bool IsRecording => _isRecording;
    public float CurrentPeakLevel => _peakLevel;
    public event EventHandler<float>? PeakLevelChanged;

    public IEnumerable<string> GetMicrophoneDevices()
    {
        var devices = new List<string>();
        for (int i = 0; i < WaveIn.DeviceCount; i++)
        {
            var caps = WaveIn.GetCapabilities(i);
            devices.Add(caps.ProductName);
        }
        return devices;
    }

    public async Task StartRecordingAsync(string outputFilePath, AudioSourceMode mode, CancellationToken cancellationToken = default)
    {
        if (_isRecording) return;

        _isRecording = true;
        var waveFormat = new WaveFormat(44100, 16, 2);

        await Task.Run(() =>
        {
            switch (mode)
            {
                case AudioSourceMode.MicOnly:
                    StartMicCapture(outputFilePath, waveFormat);
                    break;
                case AudioSourceMode.SystemOnly:
                    StartLoopbackCapture(outputFilePath);
                    break;
                case AudioSourceMode.Both:
                    StartMixedCapture(outputFilePath, waveFormat);
                    break;
            }
        }, cancellationToken);
    }

    private void StartMicCapture(string outputFilePath, WaveFormat waveFormat)
    {
        _writer = new WaveFileWriter(outputFilePath, waveFormat);
        _micCapture = new WaveInEvent { WaveFormat = waveFormat };
        _micCapture.DataAvailable += (s, e) =>
        {
            _writeLock.Wait();
            try
            {
                _writer.Write(e.Buffer, 0, e.BytesRecorded);
                UpdatePeakLevel(e.Buffer, e.BytesRecorded);
            }
            finally { _writeLock.Release(); }
        };
        _micCapture.StartRecording();
    }

    private void StartLoopbackCapture(string outputFilePath)
    {
        _loopbackCapture = new WasapiLoopbackCapture();
        _writer = new WaveFileWriter(outputFilePath, _loopbackCapture.WaveFormat);
        _loopbackCapture.DataAvailable += (s, e) =>
        {
            _writeLock.Wait();
            try
            {
                _writer.Write(e.Buffer, 0, e.BytesRecorded);
                UpdatePeakLevel(e.Buffer, e.BytesRecorded);
            }
            finally { _writeLock.Release(); }
        };
        _loopbackCapture.StartRecording();
    }

    private void StartMixedCapture(string outputFilePath, WaveFormat waveFormat)
    {
        // For mixed mode, capture both and write mixed output
        var micFormat = waveFormat;
        _micCapture = new WaveInEvent { WaveFormat = micFormat };
        _loopbackCapture = new WasapiLoopbackCapture();

        _writer = new WaveFileWriter(outputFilePath, micFormat);

        _micCapture.DataAvailable += (s, e) =>
        {
            _writeLock.Wait();
            try
            {
                _writer.Write(e.Buffer, 0, e.BytesRecorded);
                UpdatePeakLevel(e.Buffer, e.BytesRecorded);
            }
            finally { _writeLock.Release(); }
        };

        _loopbackCapture.DataAvailable += (s, e) =>
        {
            // Convert loopback to 16-bit PCM if needed and mix
            _writeLock.Wait();
            try
            {
                var resampledBuffer = ConvertToFormat(e.Buffer, e.BytesRecorded,
                    _loopbackCapture.WaveFormat, micFormat);
                _writer.Write(resampledBuffer, 0, resampledBuffer.Length);
            }
            finally { _writeLock.Release(); }
        };

        _micCapture.StartRecording();
        _loopbackCapture.StartRecording();
    }

    private static byte[] ConvertToFormat(byte[] buffer, int count, WaveFormat from, WaveFormat to)
    {
        if (from.Equals(to)) return buffer[..count];
        using var ms = new MemoryStream(buffer, 0, count);
        using var reader = new RawSourceWaveStream(ms, from);
        using var resampler = new MediaFoundationResampler(reader, to);
        using var outMs = new MemoryStream();
        var readBuffer = new byte[4096];
        int read;
        while ((read = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
            outMs.Write(readBuffer, 0, read);
        return outMs.ToArray();
    }

    private void UpdatePeakLevel(byte[] buffer, int bytesRecorded)
    {
        float max = 0f;
        for (int i = 0; i < bytesRecorded - 1; i += 2)
        {
            var sample = BitConverter.ToInt16(buffer, i) / 32768f;
            if (Math.Abs(sample) > max) max = Math.Abs(sample);
        }
        _peakLevel = max;
        PeakLevelChanged?.Invoke(this, max);
    }

    public async Task StopRecordingAsync()
    {
        if (!_isRecording) return;
        _isRecording = false;

        _micCapture?.StopRecording();
        _loopbackCapture?.StopRecording();

        await Task.Delay(200); // allow final buffers to flush

        await _writeLock.WaitAsync();
        try
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }
        finally { _writeLock.Release(); }

        _micCapture?.Dispose();
        _micCapture = null;
        _loopbackCapture?.Dispose();
        _loopbackCapture = null;
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Jareth.Core.Models;
using Jareth.Core.Services;
using Xunit;

namespace Jareth.Tests;

public class StorageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly StorageService _service;

    public StorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "JarethTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);

        var settings = new TestSettingsService(_tempRoot);
        _service = new StorageService(settings);
    }

    [Fact]
    public async Task CreateMeetingFolder_CreatesDirectory()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Test Meeting", "both", "Work");
        Assert.True(Directory.Exists(meeting.FolderPath));
    }

    [Fact]
    public async Task CreateMeetingFolder_MetadataFileExists()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Meta Test", "mic", "Personal");
        var metaPath = Path.Combine(meeting.FolderPath, "metadata.json");
        Assert.True(File.Exists(metaPath));
    }

    [Fact]
    public async Task SaveAndLoadMetadata_RoundTrip()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Round Trip", "both", "Work");
        meeting.DurationSeconds = 300;
        await _service.SaveMetadataAsync(meeting);

        var loaded = await _service.LoadMetadataAsync(meeting.FolderPath);
        Assert.NotNull(loaded);
        Assert.Equal(300, loaded!.DurationSeconds);
        Assert.Equal("Round Trip", loaded.Title);
    }

    [Fact]
    public async Task SaveAndLoadTranscript_RoundTrip()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Transcript Test", "both", "Work");
        const string transcript = "# Transcript\nSpeaker 1: Hello.";
        await _service.SaveTranscriptAsync(meeting.FolderPath, transcript);

        var loaded = await _service.LoadTranscriptAsync(meeting.FolderPath);
        Assert.Equal(transcript, loaded);
    }

    [Fact]
    public async Task SaveAndLoadSummary_RoundTrip()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Summary Test", "both", "Work");
        const string summary = "## Summary\nKey points discussed.";
        await _service.SaveSummaryAsync(meeting.FolderPath, summary);

        var loaded = await _service.LoadSummaryAsync(meeting.FolderPath);
        Assert.Equal(summary, loaded);
    }

    [Fact]
    public async Task ListAllMeetings_ReturnsAllCreated()
    {
        await _service.CreateMeetingFolderAsync("Meeting A", "both", "Work");
        await _service.CreateMeetingFolderAsync("Meeting B", "mic", "Personal");

        var list = await _service.ListAllMeetingsAsync();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task DeleteMeeting_RemovesDirectory()
    {
        var meeting = await _service.CreateMeetingFolderAsync("Delete Me", "both", "Work");
        Assert.True(Directory.Exists(meeting.FolderPath));

        await _service.DeleteMeetingAsync(meeting.FolderPath);
        Assert.False(Directory.Exists(meeting.FolderPath));
    }

    [Fact]
    public void GetAudioFilePath_ReturnsWavPath()
    {
        var path = _service.GetAudioFilePath("/some/folder");
        Assert.EndsWith("audio.wav", path);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private sealed class TestSettingsService : ISettingsService
    {
        private readonly string _rootPath;
        public AppSettings Settings { get; }

        public TestSettingsService(string rootPath)
        {
            _rootPath = rootPath;
            Settings = new AppSettings { JarethFolderPath = rootPath };
        }

        public Task LoadAsync() => Task.CompletedTask;
        public Task SaveAsync() => Task.CompletedTask;
    }
}

using Jareth.Core.Models;
using Jareth.Core.Services;

namespace Jareth.Tests;

public class StorageServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly StorageService _service;

    public StorageServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"JarethTest_{Guid.NewGuid():N}");
        _service = new StorageService(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public void Constructor_CreatesMeetingsFolder()
    {
        Assert.True(Directory.Exists(_service.MeetingsFolderPath));
    }

    [Fact]
    public async Task CreateMeetingFolder_CreatesDirectory()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("Test Meeting");
        Assert.True(Directory.Exists(folderPath));
        Assert.Contains("Test Meeting", folderPath);
    }

    [Fact]
    public async Task CreateMeetingFolder_HandlesduplicateNames()
    {
        var path1 = await _service.CreateMeetingFolderAsync("Duplicate");
        var path2 = await _service.CreateMeetingFolderAsync("Duplicate");
        Assert.NotEqual(path1, path2);
        Assert.True(Directory.Exists(path1));
        Assert.True(Directory.Exists(path2));
    }

    [Fact]
    public async Task SaveAndReadMetadata_RoundTrip()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("Metadata Test");
        var meeting = new Meeting
        {
            Title = "Metadata Test",
            DurationSeconds = 120,
            AudioSource = "mic",
            FolderCategory = "Work",
            FolderPath = folderPath
        };

        await _service.SaveMetadataAsync(meeting);
        var loaded = await _service.ReadMetadataAsync(folderPath);

        Assert.NotNull(loaded);
        Assert.Equal(meeting.Title, loaded.Title);
        Assert.Equal(meeting.DurationSeconds, loaded.DurationSeconds);
        Assert.Equal(meeting.AudioSource, loaded.AudioSource);
        Assert.Equal(meeting.FolderCategory, loaded.FolderCategory);
    }

    [Fact]
    public async Task ReadMetadata_ReturnsNullForMissingFile()
    {
        var result = await _service.ReadMetadataAsync("/nonexistent/path");
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAndReadTranscript_RoundTrip()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("Transcript Test");
        var content = "# Transcript\n\nHello, world!";

        await _service.SaveTranscriptAsync(folderPath, content);
        var loaded = await _service.ReadTranscriptAsync(folderPath);

        Assert.Equal(content, loaded);
    }

    [Fact]
    public async Task ReadTranscript_ReturnsNullForMissing()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("Empty");
        var result = await _service.ReadTranscriptAsync(folderPath);
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAndReadSummary_RoundTrip()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("Summary Test");
        var content = "# Summary\n\nKey points here.";

        await _service.SaveSummaryAsync(folderPath, content);
        var loaded = await _service.ReadSummaryAsync(folderPath);

        Assert.Equal(content, loaded);
    }

    [Fact]
    public async Task ListAllMeetings_ReturnsAllSavedMeetings()
    {
        var folder1 = await _service.CreateMeetingFolderAsync("Meeting 1");
        var folder2 = await _service.CreateMeetingFolderAsync("Meeting 2");

        await _service.SaveMetadataAsync(new Meeting
        {
            Title = "Meeting 1",
            FolderPath = folder1,
            Date = DateTime.Now.AddHours(-1)
        });
        await _service.SaveMetadataAsync(new Meeting
        {
            Title = "Meeting 2",
            FolderPath = folder2,
            Date = DateTime.Now
        });

        var meetings = await _service.ListAllMeetingsAsync();
        Assert.Equal(2, meetings.Count);
        Assert.Equal("Meeting 2", meetings[0].Title); // Most recent first
    }

    [Fact]
    public async Task DeleteMeeting_RemovesFolder()
    {
        var folderPath = await _service.CreateMeetingFolderAsync("To Delete");
        await _service.SaveMetadataAsync(new Meeting
        {
            Title = "To Delete",
            FolderPath = folderPath
        });

        Assert.True(Directory.Exists(folderPath));

        await _service.DeleteMeetingAsync(folderPath);
        Assert.False(Directory.Exists(folderPath));
    }
}

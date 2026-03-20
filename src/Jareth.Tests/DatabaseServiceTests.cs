using Jareth.Core.Models;
using Jareth.Core.Services;
using Xunit;

namespace Jareth.Tests;

public class DatabaseServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly DatabaseService _service;

    public DatabaseServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"JarethDbTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _service = new DatabaseService(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public async Task Initialize_CreatesTablesAndDefaultFolder()
    {
        await _service.InitializeAsync();

        var folders = await _service.GetFoldersAsync();
        Assert.Contains(folders, f => f.Name == "Uncategorised");
    }

    [Fact]
    public async Task UpsertAndGetMeetings()
    {
        await _service.InitializeAsync();

        var meeting = new Meeting
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test Meeting",
            Date = DateTime.Now,
            DurationSeconds = 300,
            AudioSource = "mic",
            FolderCategory = "Uncategorised",
            FolderPath = "/test/path"
        };

        await _service.UpsertMeetingAsync(meeting);
        var meetings = await _service.GetAllMeetingsAsync();

        Assert.Single(meetings);
        Assert.Equal("Test Meeting", meetings[0].Title);
    }

    [Fact]
    public async Task GetMeetingsByFolder_FiltersCorrectly()
    {
        await _service.InitializeAsync();

        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = "1",
            Title = "Work Meeting",
            FolderCategory = "Work",
            FolderPath = "/test/work"
        });
        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = "2",
            Title = "Personal Meeting",
            FolderCategory = "Personal",
            FolderPath = "/test/personal"
        });

        var workMeetings = await _service.GetMeetingsByFolderAsync("Work");
        Assert.Single(workMeetings);
        Assert.Equal("Work Meeting", workMeetings[0].Title);
    }

    [Fact]
    public async Task DeleteMeeting_RemovesFromDatabase()
    {
        await _service.InitializeAsync();

        var id = Guid.NewGuid().ToString();
        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = id,
            Title = "To Delete",
            FolderPath = "/test/delete"
        });

        await _service.DeleteMeetingAsync(id);
        var meetings = await _service.GetAllMeetingsAsync();
        Assert.Empty(meetings);
    }

    [Fact]
    public async Task CreateAndGetFolders()
    {
        await _service.InitializeAsync();
        await _service.CreateFolderAsync("Work");
        await _service.CreateFolderAsync("Personal");

        var folders = await _service.GetFoldersAsync();
        Assert.True(folders.Count >= 3); // Uncategorised + Work + Personal
        Assert.Contains(folders, f => f.Name == "Work");
        Assert.Contains(folders, f => f.Name == "Personal");
    }

    [Fact]
    public async Task RenameFolder_UpdatesMeetings()
    {
        await _service.InitializeAsync();
        await _service.CreateFolderAsync("OldName");

        var folders = await _service.GetFoldersAsync();
        var folder = folders.First(f => f.Name == "OldName");

        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = "1",
            Title = "Test",
            FolderCategory = "OldName",
            FolderPath = "/test"
        });

        await _service.RenameFolderAsync(folder.Id, "NewName");

        var meetings = await _service.GetMeetingsByFolderAsync("NewName");
        Assert.Single(meetings);

        var oldMeetings = await _service.GetMeetingsByFolderAsync("OldName");
        Assert.Empty(oldMeetings);
    }

    [Fact]
    public async Task DeleteFolder_MovesMeetingsToUncategorised()
    {
        await _service.InitializeAsync();
        await _service.CreateFolderAsync("ToDelete");

        var folders = await _service.GetFoldersAsync();
        var folder = folders.First(f => f.Name == "ToDelete");

        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = "1",
            Title = "Orphan",
            FolderCategory = "ToDelete",
            FolderPath = "/test"
        });

        await _service.DeleteFolderAsync(folder.Id);

        var meetings = await _service.GetMeetingsByFolderAsync("Uncategorised");
        Assert.Single(meetings);
        Assert.Equal("Orphan", meetings[0].Title);
    }

    [Fact]
    public async Task RebuildIndex_ReplacesAllData()
    {
        await _service.InitializeAsync();

        // Add initial data
        await _service.UpsertMeetingAsync(new Meeting
        {
            Id = "old",
            Title = "Old Meeting",
            FolderPath = "/old"
        });

        // Rebuild with new data
        var newMeetings = new List<Meeting>
        {
            new() { Id = "new1", Title = "New Meeting 1", FolderPath = "/new1", FolderCategory = "Work" },
            new() { Id = "new2", Title = "New Meeting 2", FolderPath = "/new2", FolderCategory = "Personal" }
        };
        await _service.RebuildIndexAsync(newMeetings);

        var meetings = await _service.GetAllMeetingsAsync();
        Assert.Equal(2, meetings.Count);
        Assert.DoesNotContain(meetings, m => m.Title == "Old Meeting");
    }
}

using System.Text.Json;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public class StorageService : IStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string JarethFolderPath { get; }
    public string MeetingsFolderPath { get; }

    public StorageService(string? basePath = null)
    {
        JarethFolderPath = basePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Jareth");
        MeetingsFolderPath = Path.Combine(JarethFolderPath, "Meetings");
        Directory.CreateDirectory(MeetingsFolderPath);
    }

    public Task<string> CreateMeetingFolderAsync(string title)
    {
        var folderName = $"{DateTime.Now:yyyy-MM-dd} {SanitizeFolderName(title)}";
        var folderPath = Path.Combine(MeetingsFolderPath, folderName);

        // Ensure unique folder name
        var counter = 1;
        var originalPath = folderPath;
        while (Directory.Exists(folderPath))
        {
            folderPath = $"{originalPath} ({counter++})";
        }

        Directory.CreateDirectory(folderPath);
        return Task.FromResult(folderPath);
    }

    public async Task SaveMetadataAsync(Meeting meeting)
    {
        var filePath = Path.Combine(meeting.FolderPath, "metadata.json");
        var json = JsonSerializer.Serialize(meeting, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Meeting?> ReadMetadataAsync(string meetingFolderPath)
    {
        var filePath = Path.Combine(meetingFolderPath, "metadata.json");
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        var meeting = JsonSerializer.Deserialize<Meeting>(json);
        if (meeting != null)
            meeting.FolderPath = meetingFolderPath;
        return meeting;
    }

    public async Task SaveTranscriptAsync(string meetingFolderPath, string content)
    {
        var filePath = Path.Combine(meetingFolderPath, "transcript.md");
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<string?> ReadTranscriptAsync(string meetingFolderPath)
    {
        var filePath = Path.Combine(meetingFolderPath, "transcript.md");
        if (!File.Exists(filePath))
            return null;
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveSummaryAsync(string meetingFolderPath, string content)
    {
        var filePath = Path.Combine(meetingFolderPath, "summary.md");
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<string?> ReadSummaryAsync(string meetingFolderPath)
    {
        var filePath = Path.Combine(meetingFolderPath, "summary.md");
        if (!File.Exists(filePath))
            return null;
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task<List<Meeting>> ListAllMeetingsAsync()
    {
        var meetings = new List<Meeting>();

        if (!Directory.Exists(MeetingsFolderPath))
            return meetings;

        foreach (var dir in Directory.GetDirectories(MeetingsFolderPath))
        {
            var meeting = await ReadMetadataAsync(dir);
            if (meeting != null)
                meetings.Add(meeting);
        }

        return meetings.OrderByDescending(m => m.Date).ToList();
    }

    public Task DeleteMeetingAsync(string meetingFolderPath)
    {
        if (Directory.Exists(meetingFolderPath))
            Directory.Delete(meetingFolderPath, recursive: true);
        return Task.CompletedTask;
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("", name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}

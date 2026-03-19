using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public class StorageService : IStorageService
{
    private readonly string _rootPath;

    public string JarethRootPath => _rootPath;

    public StorageService(ISettingsService settingsService)
    {
        _rootPath = settingsService.Settings.JarethFolderPath;
        if (string.IsNullOrEmpty(_rootPath))
        {
            _rootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Jareth", "Meetings");
        }
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<Meeting> CreateMeetingFolderAsync(string title, string audioSource, string folderCategory)
    {
        var meeting = new Meeting
        {
            Title = title,
            Date = DateTime.Now,
            AudioSource = audioSource,
            FolderCategory = folderCategory
        };

        var safeName = $"{meeting.Date:yyyy-MM-dd} {SanitiseName(title)}";
        var folderPath = Path.Combine(_rootPath, safeName);
        Directory.CreateDirectory(folderPath);
        meeting.FolderPath = folderPath;

        await SaveMetadataAsync(meeting);
        return meeting;
    }

    public async Task SaveMetadataAsync(Meeting meeting)
    {
        var path = Path.Combine(meeting.FolderPath, "metadata.json");
        var json = JsonSerializer.Serialize(meeting, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<Meeting?> LoadMetadataAsync(string meetingFolderPath)
    {
        var path = Path.Combine(meetingFolderPath, "metadata.json");
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path);
        var meeting = JsonSerializer.Deserialize<Meeting>(json);
        if (meeting != null) meeting.FolderPath = meetingFolderPath;
        return meeting;
    }

    public async Task SaveTranscriptAsync(string meetingFolderPath, string transcript)
    {
        var path = Path.Combine(meetingFolderPath, "transcript.md");
        await File.WriteAllTextAsync(path, transcript);
    }

    public async Task<string> LoadTranscriptAsync(string meetingFolderPath)
    {
        var path = Path.Combine(meetingFolderPath, "transcript.md");
        return File.Exists(path) ? await File.ReadAllTextAsync(path) : string.Empty;
    }

    public async Task SaveSummaryAsync(string meetingFolderPath, string summary)
    {
        var path = Path.Combine(meetingFolderPath, "summary.md");
        await File.WriteAllTextAsync(path, summary);
    }

    public async Task<string> LoadSummaryAsync(string meetingFolderPath)
    {
        var path = Path.Combine(meetingFolderPath, "summary.md");
        return File.Exists(path) ? await File.ReadAllTextAsync(path) : string.Empty;
    }

    public async Task<List<Meeting>> ListAllMeetingsAsync()
    {
        var meetings = new List<Meeting>();
        if (!Directory.Exists(_rootPath)) return meetings;

        foreach (var dir in Directory.GetDirectories(_rootPath))
        {
            var meeting = await LoadMetadataAsync(dir);
            if (meeting != null) meetings.Add(meeting);
        }

        meetings.Sort((a, b) => b.Date.CompareTo(a.Date));
        return meetings;
    }

    public Task DeleteMeetingAsync(string meetingFolderPath)
    {
        if (Directory.Exists(meetingFolderPath))
            Directory.Delete(meetingFolderPath, recursive: true);
        return Task.CompletedTask;
    }

    public string GetAudioFilePath(string meetingFolderPath)
        => Path.Combine(meetingFolderPath, "audio.wav");

    private static string SanitiseName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
            name = name.Replace(c, '_');
        return name.Length > 50 ? name[..50] : name;
    }
}

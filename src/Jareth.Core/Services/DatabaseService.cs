using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jareth.Core.Models;
using Microsoft.Data.Sqlite;

namespace Jareth.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _dbPath;

    public DatabaseService(ISettingsService settingsService)
    {
        var root = settingsService.Settings.JarethFolderPath;
        if (string.IsNullOrEmpty(root))
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Jareth");

        var jarethRoot = Path.GetDirectoryName(root) ?? root;
        Directory.CreateDirectory(jarethRoot);
        _dbPath = Path.Combine(jarethRoot, "jareth.db");
    }

    private SqliteConnection CreateConnection()
        => new($"Data Source={_dbPath}");

    public async Task InitialiseAsync()
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS meetings (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                date TEXT NOT NULL,
                duration_seconds INTEGER NOT NULL DEFAULT 0,
                audio_source TEXT NOT NULL DEFAULT 'both',
                folder_category TEXT NOT NULL DEFAULT 'Uncategorised',
                folder_path TEXT NOT NULL DEFAULT ''
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task IndexMeetingAsync(Meeting meeting)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR REPLACE INTO meetings (id, title, date, duration_seconds, audio_source, folder_category, folder_path)
            VALUES ($id, $title, $date, $dur, $src, $cat, $path);
            """;
        cmd.Parameters.AddWithValue("$id", meeting.Id);
        cmd.Parameters.AddWithValue("$title", meeting.Title);
        cmd.Parameters.AddWithValue("$date", meeting.Date.ToString("o"));
        cmd.Parameters.AddWithValue("$dur", meeting.DurationSeconds);
        cmd.Parameters.AddWithValue("$src", meeting.AudioSource);
        cmd.Parameters.AddWithValue("$cat", meeting.FolderCategory);
        cmd.Parameters.AddWithValue("$path", meeting.FolderPath);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Meeting>> GetIndexedMeetingsAsync(string? folderCategory = null)
    {
        var meetings = new List<Meeting>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();

        if (string.IsNullOrEmpty(folderCategory) || folderCategory == "All Meetings")
            cmd.CommandText = "SELECT id, title, date, duration_seconds, audio_source, folder_category, folder_path FROM meetings ORDER BY date DESC;";
        else
        {
            cmd.CommandText = "SELECT id, title, date, duration_seconds, audio_source, folder_category, folder_path FROM meetings WHERE folder_category = $cat ORDER BY date DESC;";
            cmd.Parameters.AddWithValue("$cat", folderCategory);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            meetings.Add(new Meeting
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                Date = DateTime.Parse(reader.GetString(2)),
                DurationSeconds = reader.GetInt32(3),
                AudioSource = reader.GetString(4),
                FolderCategory = reader.GetString(5),
                FolderPath = reader.GetString(6)
            });
        }
        return meetings;
    }

    public async Task RemoveMeetingFromIndexAsync(string meetingId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM meetings WHERE id = $id;";
        cmd.Parameters.AddWithValue("$id", meetingId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RebuildIndexAsync(IStorageService storageService)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var clearCmd = conn.CreateCommand();
        clearCmd.CommandText = "DELETE FROM meetings;";
        await clearCmd.ExecuteNonQueryAsync();

        var meetings = await storageService.ListAllMeetingsAsync();
        foreach (var m in meetings)
            await IndexMeetingAsync(m);
    }
}

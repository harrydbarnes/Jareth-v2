using Microsoft.Data.Sqlite;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string jarethFolderPath)
    {
        var dbPath = Path.Combine(jarethFolderPath, "jareth.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Meetings (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Date TEXT NOT NULL,
                DurationSeconds INTEGER NOT NULL,
                AudioSource TEXT NOT NULL,
                FolderCategory TEXT NOT NULL DEFAULT 'Uncategorised',
                FolderPath TEXT NOT NULL,
                SummaryStyle TEXT,
                TranscriptionProvider TEXT,
                SummaryProvider TEXT
            );

            CREATE TABLE IF NOT EXISTS Folders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                CreatedAt TEXT NOT NULL
            );

            INSERT OR IGNORE INTO Folders (Name, CreatedAt) VALUES ('Uncategorised', datetime('now'));
        ";
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpsertMeetingAsync(Meeting meeting)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Meetings (Id, Title, Date, DurationSeconds, AudioSource, FolderCategory, FolderPath, SummaryStyle, TranscriptionProvider, SummaryProvider)
            VALUES (@Id, @Title, @Date, @DurationSeconds, @AudioSource, @FolderCategory, @FolderPath, @SummaryStyle, @TranscriptionProvider, @SummaryProvider)
        ";
        command.Parameters.AddWithValue("@Id", meeting.Id);
        command.Parameters.AddWithValue("@Title", meeting.Title);
        command.Parameters.AddWithValue("@Date", meeting.Date.ToString("o"));
        command.Parameters.AddWithValue("@DurationSeconds", meeting.DurationSeconds);
        command.Parameters.AddWithValue("@AudioSource", meeting.AudioSource);
        command.Parameters.AddWithValue("@FolderCategory", meeting.FolderCategory);
        command.Parameters.AddWithValue("@FolderPath", meeting.FolderPath);
        command.Parameters.AddWithValue("@SummaryStyle", meeting.SummaryStyle);
        command.Parameters.AddWithValue("@TranscriptionProvider", meeting.TranscriptionProvider);
        command.Parameters.AddWithValue("@SummaryProvider", meeting.SummaryProvider);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Meeting>> GetAllMeetingsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Meetings ORDER BY Date DESC";

        var meetings = new List<Meeting>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            meetings.Add(ReadMeeting(reader));
        }
        return meetings;
    }

    public async Task<List<Meeting>> GetMeetingsByFolderAsync(string folderCategory)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Meetings WHERE FolderCategory = @FolderCategory ORDER BY Date DESC";
        command.Parameters.AddWithValue("@FolderCategory", folderCategory);

        var meetings = new List<Meeting>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            meetings.Add(ReadMeeting(reader));
        }
        return meetings;
    }

    public async Task DeleteMeetingAsync(string meetingId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Meetings WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", meetingId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Folder>> GetFoldersAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT f.Id, f.Name, f.CreatedAt,
                   (SELECT COUNT(*) FROM Meetings m WHERE m.FolderCategory = f.Name) as MeetingCount
            FROM Folders f
            ORDER BY f.Name
        ";

        var folders = new List<Folder>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            folders.Add(new Folder
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                CreatedAt = DateTime.Parse(reader.GetString(2)),
                MeetingCount = reader.GetInt32(3)
            });
        }
        return folders;
    }

    public async Task CreateFolderAsync(string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Folders (Name, CreatedAt) VALUES (@Name, @CreatedAt)";
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task RenameFolderAsync(int folderId, string newName)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Get old name first
        var getCommand = connection.CreateCommand();
        getCommand.CommandText = "SELECT Name FROM Folders WHERE Id = @Id";
        getCommand.Parameters.AddWithValue("@Id", folderId);
        var oldName = (string?)await getCommand.ExecuteScalarAsync();

        if (oldName == null) return;

        // Update folder name
        var updateFolder = connection.CreateCommand();
        updateFolder.CommandText = "UPDATE Folders SET Name = @NewName WHERE Id = @Id";
        updateFolder.Parameters.AddWithValue("@NewName", newName);
        updateFolder.Parameters.AddWithValue("@Id", folderId);
        await updateFolder.ExecuteNonQueryAsync();

        // Update meetings in that folder
        var updateMeetings = connection.CreateCommand();
        updateMeetings.CommandText = "UPDATE Meetings SET FolderCategory = @NewName WHERE FolderCategory = @OldName";
        updateMeetings.Parameters.AddWithValue("@NewName", newName);
        updateMeetings.Parameters.AddWithValue("@OldName", oldName);
        await updateMeetings.ExecuteNonQueryAsync();
    }

    public async Task DeleteFolderAsync(int folderId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Get folder name first
        var getCommand = connection.CreateCommand();
        getCommand.CommandText = "SELECT Name FROM Folders WHERE Id = @Id";
        getCommand.Parameters.AddWithValue("@Id", folderId);
        var folderName = (string?)await getCommand.ExecuteScalarAsync();

        if (folderName == null) return;

        // Move meetings to Uncategorised
        var updateMeetings = connection.CreateCommand();
        updateMeetings.CommandText = "UPDATE Meetings SET FolderCategory = 'Uncategorised' WHERE FolderCategory = @Name";
        updateMeetings.Parameters.AddWithValue("@Name", folderName);
        await updateMeetings.ExecuteNonQueryAsync();

        // Delete folder
        var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM Folders WHERE Id = @Id";
        deleteCommand.Parameters.AddWithValue("@Id", folderId);
        await deleteCommand.ExecuteNonQueryAsync();
    }

    public async Task RebuildIndexAsync(List<Meeting> meetings)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Clear existing meetings
        var clearCommand = connection.CreateCommand();
        clearCommand.CommandText = "DELETE FROM Meetings";
        await clearCommand.ExecuteNonQueryAsync();

        // Reinsert all meetings
        foreach (var meeting in meetings)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Meetings (Id, Title, Date, DurationSeconds, AudioSource, FolderCategory, FolderPath, SummaryStyle, TranscriptionProvider, SummaryProvider)
                VALUES (@Id, @Title, @Date, @DurationSeconds, @AudioSource, @FolderCategory, @FolderPath, @SummaryStyle, @TranscriptionProvider, @SummaryProvider)
            ";
            command.Parameters.AddWithValue("@Id", meeting.Id);
            command.Parameters.AddWithValue("@Title", meeting.Title);
            command.Parameters.AddWithValue("@Date", meeting.Date.ToString("o"));
            command.Parameters.AddWithValue("@DurationSeconds", meeting.DurationSeconds);
            command.Parameters.AddWithValue("@AudioSource", meeting.AudioSource);
            command.Parameters.AddWithValue("@FolderCategory", meeting.FolderCategory);
            command.Parameters.AddWithValue("@FolderPath", meeting.FolderPath);
            command.Parameters.AddWithValue("@SummaryStyle", meeting.SummaryStyle);
            command.Parameters.AddWithValue("@TranscriptionProvider", meeting.TranscriptionProvider);
            command.Parameters.AddWithValue("@SummaryProvider", meeting.SummaryProvider);
            await command.ExecuteNonQueryAsync();

            // Ensure folder exists
            var folderCommand = connection.CreateCommand();
            folderCommand.CommandText = "INSERT OR IGNORE INTO Folders (Name, CreatedAt) VALUES (@Name, @CreatedAt)";
            folderCommand.Parameters.AddWithValue("@Name", meeting.FolderCategory);
            folderCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
            await folderCommand.ExecuteNonQueryAsync();
        }
    }

    private static Meeting ReadMeeting(SqliteDataReader reader)
    {
        return new Meeting
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date"))),
            DurationSeconds = reader.GetInt32(reader.GetOrdinal("DurationSeconds")),
            AudioSource = reader.GetString(reader.GetOrdinal("AudioSource")),
            FolderCategory = reader.GetString(reader.GetOrdinal("FolderCategory")),
            FolderPath = reader.GetString(reader.GetOrdinal("FolderPath")),
            SummaryStyle = reader.IsDBNull(reader.GetOrdinal("SummaryStyle")) ? "default" : reader.GetString(reader.GetOrdinal("SummaryStyle")),
            TranscriptionProvider = reader.IsDBNull(reader.GetOrdinal("TranscriptionProvider")) ? "whisper-local" : reader.GetString(reader.GetOrdinal("TranscriptionProvider")),
            SummaryProvider = reader.IsDBNull(reader.GetOrdinal("SummaryProvider")) ? "openai" : reader.GetString(reader.GetOrdinal("SummaryProvider"))
        };
    }
}

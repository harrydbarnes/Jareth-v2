using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface IStorageService
{
    string JarethFolderPath { get; }
    string MeetingsFolderPath { get; }

    Task<string> CreateMeetingFolderAsync(string title);
    Task SaveMetadataAsync(Meeting meeting);
    Task<Meeting?> ReadMetadataAsync(string meetingFolderPath);
    Task SaveTranscriptAsync(string meetingFolderPath, string content);
    Task<string?> ReadTranscriptAsync(string meetingFolderPath);
    Task SaveSummaryAsync(string meetingFolderPath, string content);
    Task<string?> ReadSummaryAsync(string meetingFolderPath);
    Task<List<Meeting>> ListAllMeetingsAsync();
    Task DeleteMeetingAsync(string meetingFolderPath);
}

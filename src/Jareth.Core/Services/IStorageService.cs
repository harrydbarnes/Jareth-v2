using System.Collections.Generic;
using System.Threading.Tasks;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface IStorageService
{
    string JarethRootPath { get; }
    Task<Meeting> CreateMeetingFolderAsync(string title, string audioSource, string folderCategory);
    Task SaveMetadataAsync(Meeting meeting);
    Task<Meeting?> LoadMetadataAsync(string meetingFolderPath);
    Task SaveTranscriptAsync(string meetingFolderPath, string transcript);
    Task<string> LoadTranscriptAsync(string meetingFolderPath);
    Task SaveSummaryAsync(string meetingFolderPath, string summary);
    Task<string> LoadSummaryAsync(string meetingFolderPath);
    Task<List<Meeting>> ListAllMeetingsAsync();
    Task DeleteMeetingAsync(string meetingFolderPath);
    string GetAudioFilePath(string meetingFolderPath);
}

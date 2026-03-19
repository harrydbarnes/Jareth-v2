using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task UpsertMeetingAsync(Meeting meeting);
    Task<List<Meeting>> GetAllMeetingsAsync();
    Task<List<Meeting>> GetMeetingsByFolderAsync(string folderCategory);
    Task DeleteMeetingAsync(string meetingId);
    Task<List<Folder>> GetFoldersAsync();
    Task CreateFolderAsync(string name);
    Task RenameFolderAsync(int folderId, string newName);
    Task DeleteFolderAsync(int folderId);
    Task RebuildIndexAsync(List<Meeting> meetings);
}

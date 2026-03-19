using System.Collections.Generic;
using System.Threading.Tasks;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface IDatabaseService
{
    Task InitialiseAsync();
    Task IndexMeetingAsync(Meeting meeting);
    Task<List<Meeting>> GetIndexedMeetingsAsync(string? folderCategory = null);
    Task RemoveMeetingFromIndexAsync(string meetingId);
    Task RebuildIndexAsync(IStorageService storageService);
}

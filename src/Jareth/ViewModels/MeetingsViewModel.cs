using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Jareth.Core.Models;
using Jareth.Core.Services;

namespace Jareth.ViewModels;

public partial class MeetingsViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly IStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<Meeting> _meetings = new();

    [ObservableProperty]
    private Meeting? _selectedMeeting;

    public MeetingsViewModel(IDatabaseService db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task LoadAsync(string? folderCategory = null)
    {
        var items = await _db.GetIndexedMeetingsAsync(folderCategory);
        Meetings.Clear();
        foreach (var m in items)
            Meetings.Add(m);
    }

    public async Task DeleteMeetingAsync(Meeting meeting)
    {
        await _db.RemoveMeetingFromIndexAsync(meeting.Id);
        await _storage.DeleteMeetingAsync(meeting.FolderPath);
        Meetings.Remove(meeting);
    }
}

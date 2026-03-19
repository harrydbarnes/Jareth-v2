using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jareth.Core.Models;
using Jareth.Core.Services;
using Microsoft.UI.Xaml;

namespace Jareth.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly ISettingsService _settings;

    [ObservableProperty]
    private ObservableCollection<MeetingFolder> _folders = new();

    [ObservableProperty]
    private ObservableCollection<Meeting> _meetings = new();

    [ObservableProperty]
    private MeetingFolder? _selectedFolder;

    [ObservableProperty]
    private Meeting? _selectedMeeting;

    [ObservableProperty]
    private string _selectedFolderName = "All Meetings";

    public Visibility IsMeetingsEmpty => Meetings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public MainViewModel(IDatabaseService db, ISettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task InitialiseAsync()
    {
        // Load folders from settings
        Folders.Clear();
        foreach (var f in _settings.Settings.Folders)
            Folders.Add(f);

        // Load all meetings
        await LoadMeetingsForFolderAsync(null);
    }

    public async Task LoadMeetingsForFolderAsync(string? folderName)
    {
        SelectedFolderName = folderName ?? "All Meetings";
        var items = await _db.GetIndexedMeetingsAsync(folderName);
        Meetings.Clear();
        foreach (var m in items)
            Meetings.Add(m);

        OnPropertyChanged(nameof(IsMeetingsEmpty));
    }

    public async Task CreateFolderAsync(string name)
    {
        var folder = new MeetingFolder { Name = name };
        _settings.Settings.Folders.Add(folder);
        await _settings.SaveAsync();
        Folders.Add(folder);
    }

    public async Task RenameFolderAsync(MeetingFolder folder, string newName)
    {
        folder.Name = newName;
        await _settings.SaveAsync();
        await InitialiseAsync();
    }

    public async Task DeleteFolderAsync(MeetingFolder folder)
    {
        _settings.Settings.Folders.Remove(folder);
        await _settings.SaveAsync();
        Folders.Remove(folder);
    }

    public async Task RefreshMeetingsAsync()
    {
        await LoadMeetingsForFolderAsync(SelectedFolderName == "All Meetings" ? null : SelectedFolderName);
    }
}

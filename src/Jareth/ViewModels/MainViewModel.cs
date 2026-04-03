using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Jareth.Core.Models;
using Jareth.Core.Services;

namespace Jareth.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly IStorageService _storageService;

    [ObservableProperty]
    private ObservableCollection<Folder> _folders = new();

    [ObservableProperty]
    private ObservableCollection<Meeting> _meetings = new();

    [ObservableProperty]
    private Folder? _selectedFolder;

    [ObservableProperty]
    private Meeting? _selectedMeeting;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Visibility _showEmptyState = Microsoft.UI.Xaml.Visibility.Visible;

    public MainViewModel(IDatabaseService databaseService, IStorageService storageService)
    {
        _databaseService = databaseService;
        _storageService = storageService;
    }

    public async Task InitializeAsync()
    {
        await LoadFoldersAsync();
        await LoadMeetingsAsync();
    }

    public async Task LoadFoldersAsync()
    {
        var folders = await _databaseService.GetFoldersAsync();
        Folders = new ObservableCollection<Folder>(folders);
    }

    public async Task LoadMeetingsAsync()
    {
        List<Meeting> meetings;

        if (SelectedFolder != null && SelectedFolder.Name != "All Meetings")
        {
            meetings = await _databaseService.GetMeetingsByFolderAsync(SelectedFolder.Name);
        }
        else
        {
            meetings = await _databaseService.GetAllMeetingsAsync();
        }

        Meetings = new ObservableCollection<Meeting>(meetings);
        ShowEmptyState = Meetings.Count == 0
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    public async Task CreateFolderAsync(string name)
    {
        await _databaseService.CreateFolderAsync(name);
        await LoadFoldersAsync();
    }

    public async Task RenameFolderAsync(int folderId, string newName)
    {
        await _databaseService.RenameFolderAsync(folderId, newName);
        await LoadFoldersAsync();
        await LoadMeetingsAsync();
    }

    public async Task DeleteFolderAsync(int folderId)
    {
        await _databaseService.DeleteFolderAsync(folderId);
        await LoadFoldersAsync();
        await LoadMeetingsAsync();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Jareth.Core.Services;
using Jareth.ViewModels;

namespace Jareth;

public partial class App : Application
{
    private Window? _window;
    private readonly IHost _host;

    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IStorageService>(sp =>
                {
                    var storage = new StorageService();
                    return storage;
                });
                services.AddSingleton<ISettingsService>(sp =>
                {
                    var storageService = sp.GetRequiredService<IStorageService>();
                    return new SettingsService(storageService.JarethFolderPath);
                });
                services.AddSingleton<IDatabaseService>(sp =>
                {
                    var storageService = sp.GetRequiredService<IStorageService>();
                    return new DatabaseService(storageService.JarethFolderPath);
                });
                services.AddSingleton<IAudioService, AudioService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<RecordingViewModel>();
                services.AddTransient<SettingsViewModel>();
            })
            .Build();

        Services = _host.Services;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize database
        var dbService = Services.GetRequiredService<IDatabaseService>();
        await dbService.InitializeAsync();

        // Rebuild index from file system on first launch
        var storageService = Services.GetRequiredService<IStorageService>();
        var meetings = await storageService.ListAllMeetingsAsync();
        await dbService.RebuildIndexAsync(meetings);

        _window = new MainWindow();
        _window.Activate();
    }
}

using System;
using Jareth.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace Jareth;

public partial class App : Application
{
    public static IHost? Host { get; private set; }

    public static T GetService<T>() where T : class
        => Host!.Services.GetRequiredService<T>();

    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IStorageService, StorageService>();
                services.AddSingleton<IAudioService, AudioService>();
                services.AddSingleton<IDatabaseService, DatabaseService>();

                // ViewModels
                services.AddTransient<ViewModels.MainViewModel>();
                services.AddTransient<ViewModels.RecordingViewModel>();
                services.AddTransient<ViewModels.MeetingsViewModel>();
                services.AddTransient<ViewModels.SettingsViewModel>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var settings = GetService<ISettingsService>();
        await settings.LoadAsync();

        var db = GetService<IDatabaseService>();
        await db.InitialiseAsync();

        var storage = GetService<IStorageService>();
        await db.RebuildIndexAsync(storage);

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}

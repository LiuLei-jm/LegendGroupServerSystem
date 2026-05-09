using CommunityToolkit.Mvvm.Messaging;
using LegendGroupServerSystem.WPf.Services.Implements;
using LegendGroupServerSystem.WPf.Services.Interfaces;
using LegendGroupServerSystem.WPf.ViewModels;
using LegendGroupServerSystem.WPf.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;
using Application = System.Windows.Application;

namespace legendGroupServerSystem.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost AppHost { get; private set; }

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "Logs/ClientLog-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30
            )
            .CreateLogger();
        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(
                (hostContext, services) =>
                {
                    services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                    services.AddSingleton<IConfigurationService, ConfigurationService>();
                    services.AddSingleton<IFileOperationService, FileOperationService>();
                    services.AddSingleton<ISignalRClientService, SignalRClientService>();
                    services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

                    services.AddSingleton<LogViewModel>();
                    services.AddTransient<SettingsViewModel>();

                    services.AddTransient<MainViewModel>();
                    services.AddTransient<MainWindow>();

                    services.AddSingleton(_ => Current.Dispatcher);
                }
            )
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await AppHost.StartAsync();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        await AppHost.StopAsync();
        AppHost.Dispose();
        Log.CloseAndFlush();
    }
}

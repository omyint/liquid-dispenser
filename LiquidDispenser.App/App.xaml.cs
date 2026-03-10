using LiquidDispenser.Simulator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;


namespace LiquidDispenser.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    // Register Simulator Core logic as Singleton so its state persists 
                    services.AddSingleton<Instrument>();

                    // Register ViewModels
                    services.AddTransient<LiquidDispenser.App.ViewModels.MainViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                })
            .Build();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();

        // Resolve MainWindow from DI container, which automatically injects MainViewModel
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

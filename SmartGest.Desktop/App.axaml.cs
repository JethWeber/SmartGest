using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using SmartGest.Desktop.ViewModels;
using SmartGest.Desktop.Views;
using LiveChartsCore; 
using LiveChartsCore.SkiaSharpView; 

namespace SmartGest.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
        );
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // ── 1. SPLASH SCREEN (Ecrã de Carregamento) ──────────────────────
            var splashVm = new SplashViewModel();
            var splash   = new SplashView { DataContext = splashVm };

            splashVm.LoadingCompleted += () =>
            {
                // ── 2. LOGIN SCREEN (Ecrã de Autenticação) ────────────────────
                var loginVm = new LoginViewModel();
                var login   = new LoginView { DataContext = loginVm };

                loginVm.LoginSucceeded += () =>
                {
                    // ── 3. SHELL PRINCIPAL (Carrega a Janela Mãe com a Sidebar Fixa) ──
                    // O MainWindowViewModel encarrega-se de instanciar o DashboardViewModel como página inicial.
                    var mainVm = new MainWindowViewModel();
                    var main   = new MainWindow { DataContext = mainVm };

                    // Define a MainWindow (com a barra lateral e ContentControl) como a janela principal
                    desktop.MainWindow = main;
                    
                    main.Show();   // Abre o ambiente de trabalho integrado
                    login.Close(); // Fecha em definitivo o ecrã de Login
                };

                desktop.MainWindow = login;
                login.Show();
                splash.Close();
            };

            desktop.MainWindow = splash;
            splash.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
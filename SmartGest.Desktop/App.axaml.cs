using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.IO;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SmartGest.Desktop.Services;
using SmartGest.Desktop.ViewModels;
using SmartGest.Desktop.Views;
using SmartGest.Application;
using SmartGest.Infrastructure;

namespace SmartGest.Desktop;

public partial class App : Avalonia.Application
{
    public static ServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LiveCharts.Configure(config =>
            config.AddSkiaSharp().AddDefaultMappers());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        RegisterServices(collection);
        Services = collection.BuildServiceProvider();

        // Inicializa a base local antes de abrir o primeiro ecrã.
        // O caminho fica no perfil do utilizador, nunca dentro da pasta da aplicação.
        Services.InitializeSmartGestDatabaseAsync().GetAwaiter().GetResult();
        Services.GetRequiredService<LocalDatabaseMaintenance>().CreateAutomaticBackupIfNeededAsync().GetAwaiter().GetResult();

        // Preferência visual é local à instalação/utilizador e fica em JSON.
        // Dados empresariais e financeiros continuam na BD.
        Services.GetRequiredService<AuditService>().EnsureSchemaAsync().GetAwaiter().GetResult();

        var themeService = Services.GetRequiredService<ThemeService>();
        themeService.Apply(themeService.LoadThemeIndex(), persist: false);

        var pluginsToRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();
        foreach (var p in pluginsToRemove)
            BindingPlugins.DataValidators.Remove(p);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            void ShowLogin()
            {
                var loginVm = Services.GetRequiredService<LoginViewModel>();
                var login = new LoginView { DataContext = loginVm };

                loginVm.LoginSucceeded += () =>
                {
                    var mainVm = Services.GetRequiredService<MainWindowViewModel>();
                    var main = new MainWindow { DataContext = mainVm };

                    mainVm.PedirAbrirNovoLancamento += async vm =>
                    {
                        var dialog = new NovoLancamentoView { DataContext = vm };
                        await dialog.ShowDialog(main);
                    };

                    mainVm.SessionExpiredRequested += () =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            if (desktop.MainWindow == main)
                            {
                                main.Close();
                                ShowLogin();
                            }
                        });
                    };

                    _ = Services.GetRequiredService<SessionSecurityService>().StartAsync();
                    desktop.MainWindow = main;
                    main.Show();
                    login.Close();

                };

                desktop.MainWindow = login;
                login.Show();
            }

            var splashVm = new SplashViewModel();
            var splash = new SplashView { DataContext = splashVm };
            splashVm.LoadingCompleted += () =>
            {
                ShowLogin();
                splash.Close();
            };

            desktop.MainWindow = splash;
            splash.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // ── Core da aplicação ────────────────────────────────────────────────
        services.AddSmartGestApplication();

        // ── Persistência local ────────────────────────────────────────────────
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartGest");

        var databasePath = Path.Combine(dataDirectory, "smartgest.db");
        services.AddSmartGestInfrastructure(databasePath);

        // ── Infraestrutura de sessão/API (compatibilidade durante a migração) ──
        services.AddSingleton<TokenStore>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<UiFeedbackService>();
        services.AddSingleton<AuditService>();
        services.AddSingleton<SessionSecurityService>();
        services.AddSingleton<FirstRunService>();
        services.AddSingleton<ApiClient>();

        // ── Serviços de API ───────────────────────────────────────────────────
        services.AddTransient<AuthService>();
        services.AddTransient<LancamentoService>();
        services.AddTransient<ContasBancariasService>();
        services.AddTransient<ContabilidadeService>();
        services.AddTransient<CategoriaService>();
        services.AddTransient<DashboardService>();
        services.AddTransient<ConfiguracoesService>();

        // ── ViewModels simples ────────────────────────────────────────────────
        services.AddTransient<LoginViewModel>();
        services.AddTransient<CaixaViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<BalanceteViewModel>();
        services.AddSingleton<BalancoViewModel>();
        services.AddSingleton<DreViewModel>();

        // SINGLETON: estado preservado entre navegações; construído uma única
        // vez após o login, quando o TokenStore já tem o JWT preenchido.
        services.AddSingleton<ContaseBancosViewModel>();

        // ── Factory de NovoLancamentoViewModel ────────────────────────────────
        services.AddSingleton<Func<NovoLancamentoViewModel>>(sp =>
            () => new NovoLancamentoViewModel(
                sp.GetRequiredService<LancamentoService>(),
                sp.GetRequiredService<ContasBancariasService>(),
                sp.GetRequiredService<CategoriaService>()));

        // ── MainWindowViewModel — Singleton, recebe tudo via DI ──────────────
        services.AddSingleton<MainWindowViewModel>();
    }
}
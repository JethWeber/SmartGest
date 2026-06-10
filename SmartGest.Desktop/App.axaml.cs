using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SmartGest.Desktop.Services;
using SmartGest.Desktop.ViewModels;
using SmartGest.Desktop.Views;

namespace SmartGest.Desktop;

public partial class App : Application
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

        var pluginsToRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();
        foreach (var p in pluginsToRemove)
            BindingPlugins.DataValidators.Remove(p);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splashVm = new SplashViewModel();
            var splash   = new SplashView { DataContext = splashVm };

            splashVm.LoadingCompleted += () =>
            {
                var loginVm = Services.GetRequiredService<LoginViewModel>();
                var login   = new LoginView { DataContext = loginVm };

                loginVm.LoginSucceeded += () =>
                {
                    var mainVm = Services.GetRequiredService<MainWindowViewModel>();
                    var main   = new MainWindow { DataContext = mainVm };

                    mainVm.PedirAbrirNovoLancamento += async vm =>
                    {
                        var dialog = new NovoLancamentoView { DataContext = vm };
                        await dialog.ShowDialog(main);
                    };

                    desktop.MainWindow = main;
                    main.Show();
                    login.Close();
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

    private static void RegisterServices(IServiceCollection services)
    {
        // ── Infraestrutura ────────────────────────────────────────────────────
        services.AddSingleton<TokenStore>();
        services.AddSingleton<ApiClient>();

        // ── Serviços de API ───────────────────────────────────────────────────
        services.AddTransient<AuthService>();
        services.AddTransient<LancamentoService>();
        services.AddTransient<ContasBancariasService>();
        services.AddTransient<ContabilidadeService>();
        services.AddTransient<CategoriaService>();
        services.AddTransient<DashboardService>();

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
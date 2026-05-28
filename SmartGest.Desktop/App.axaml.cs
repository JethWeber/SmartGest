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
        // ── DI ────────────────────────────────────────────────────────────────
        var collection = new ServiceCollection();
        RegisterServices(collection);
        Services = collection.BuildServiceProvider();

        // Remove validação duplicada do Avalonia
        var pluginsToRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();
        foreach (var p in pluginsToRemove)
            BindingPlugins.DataValidators.Remove(p);

        // ── Fluxo de janelas ──────────────────────────────────────────────────
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

                    // PedirAbrirNovoLancamento: cria o modal e abre-o.
                    // vm.OwnerWindow e DialogClosed são tratados pelo construtor
                    // de NovoLancamentoView — não precisamos de os repetir aqui.
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
        services.AddSingleton<TokenStore>();
        services.AddSingleton<ApiClient>();

        services.AddTransient<AuthService>();
        services.AddTransient<LancamentoService>();
        services.AddTransient<ContasBancariasService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<NovoLancamentoViewModel>();

        // Factory de Transient: cada chamada resolve uma instância nova com
        // o TokenStore/ApiClient singleton correctos (token já preenchido).
        services.AddSingleton<Func<NovoLancamentoViewModel>>(sp =>
            () => sp.GetRequiredService<NovoLancamentoViewModel>());

        services.AddSingleton<MainWindowViewModel>();
    }
}

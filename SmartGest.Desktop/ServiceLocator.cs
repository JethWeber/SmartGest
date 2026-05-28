using Microsoft.Extensions.DependencyInjection;
using SmartGest.Desktop.Services;
using SmartGest.Desktop.ViewModels;

namespace SmartGest.Desktop;

/// <summary>
/// Registo de serviços e ViewModels no contentor de DI.
///
/// COMO USAR em App.axaml.cs:
///
///   public override void Initialize()
///   {
///       AvaloniaXamlLoader.Load(this);
///   }
///
///   public override void OnFrameworkInitializationCompleted()
///   {
///       var services = new ServiceCollection();
///       ServiceLocator.Register(services);
///       var provider = services.BuildServiceProvider();
///       ServiceLocator.Provider = provider;
///
///       if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
///       {
///           desktop.MainWindow = new SplashWindow();   // ou LoginWindow
///       }
///       base.OnFrameworkInitializationCompleted();
///   }
/// </summary>
public static class ServiceLocator
{
    public static ServiceProvider? Provider { get; set; }

    public static T Get<T>() where T : notnull
        => Provider!.GetRequiredService<T>();

    public static void Register(IServiceCollection services)
    {
        // ── Infraestrutura ────────────────────────────────────────────────────
        services.AddSingleton<TokenStore>();
        services.AddSingleton<ApiClient>();

        // ── Serviços de API ───────────────────────────────────────────────────
        services.AddTransient<AuthService>();
        services.AddTransient<LancamentoService>();
        services.AddTransient<ContasBancariasService>();

        // ── ViewModels ────────────────────────────────────────────────────────
        services.AddTransient<LoginViewModel>();
        services.AddTransient<NovoLancamentoViewModel>();

        // Factory de NovoLancamentoViewModel: cria um Transient novo a cada
        // chamada, garantindo que recebe o ApiClient/TokenStore singletons
        // correctos (com o token JWT preenchido após login).
        services.AddSingleton<Func<NovoLancamentoViewModel>>(sp =>
            () => sp.GetRequiredService<NovoLancamentoViewModel>());

        services.AddSingleton<MainWindowViewModel>();
    }
}

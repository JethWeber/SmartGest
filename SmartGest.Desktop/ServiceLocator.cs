using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
        // ── Configuração (appsettings.json + env) ─────────────────────────────
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var apiBase = config["Api:BaseUrl"] ?? "http://localhost:8080/";

        // ── Infraestrutura ────────────────────────────────────────────────────
        services.AddSingleton<TokenStore>();

        // Regista ApiClient via HttpClientFactory usando o BaseUrl da config
        services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(apiBase));

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


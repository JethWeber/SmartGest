using Microsoft.Extensions.DependencyInjection;
using SmartGest.Application.Lancamentos;

namespace SmartGest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSmartGestApplication(this IServiceCollection services)
    {
        services.AddScoped<ILancamentoApplicationService, LancamentoApplicationService>();
        return services;
    }
}

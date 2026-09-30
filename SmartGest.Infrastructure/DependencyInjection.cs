using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartGest.Application.Abstractions;
using SmartGest.Infrastructure.Persistence;
using SmartGest.Infrastructure.Persistence.Repositories;

namespace SmartGest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSmartGestInfrastructure(
        this IServiceCollection services,
        string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("O caminho da base de dados é obrigatório.", nameof(databasePath));

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        services.AddDbContextFactory<SmartGestDbContext>(options =>
            options.UseSqlite($"Data Source={fullPath}"));
        services.AddScoped<SmartGestDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<SmartGestDbContext>>().CreateDbContext());

        services.AddScoped<ICategoriaContabilRepository, CategoriaContabilRepository>();
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton(sp => new LocalDatabaseMaintenance(
            sp.GetRequiredService<IDbContextFactory<SmartGestDbContext>>(), fullPath));

        return services;
    }

    public static async Task InitializeSmartGestDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartGestDbContext>();
        await DatabaseInitializer.InitializeAsync(db, cancellationToken);
    }
}

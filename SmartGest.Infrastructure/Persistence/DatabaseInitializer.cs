
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SmartGest.Core.Domain;

namespace SmartGest.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        SmartGestDbContext db,
        CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // Compatibilidade de primeira execução: a BD local precisa de um
        // utilizador administrador para funcionar sem API.
        if (!await db.Utilizadores.AnyAsync(cancellationToken))
        {
            var admin = new Utilizador(
                "Administrador SmartGest",
                "admin@smartgest.local",
                "999999999",
                BCrypt.HashPassword("admin123", workFactor: 12),
                "Administrador");

            db.Utilizadores.Add(admin);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
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
        var migrations = db.Database.GetMigrations().ToArray();

        if (migrations.Length == 0)
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            var existingSchema = await TabelaExisteAsync(db, "Utilizadores", cancellationToken);
            var applied = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();

            // A primeira versão foi inicialmente criada com EnsureCreated.
            // Fazemos o baseline uma única vez antes de começar a usar Migrations.
            if (existingSchema && applied.Length == 0)
            {
                await db.Database.MigrateAsync(migrations[0], cancellationToken);
                var productVersion = "10.0.0";
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ({migrations[0]}, {productVersion})",
                    cancellationToken);
            }

            await db.Database.MigrateAsync(cancellationToken);
        }

        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;", cancellationToken);

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

    private static async Task<bool> TabelaExisteAsync(
        SmartGestDbContext db,
        string nome,
        CancellationToken cancellationToken)
    {
        var result = await db.Database.SqlQueryRaw<int>(
            $"SELECT COUNT(*) AS Value FROM sqlite_master WHERE type='table' AND name='{nome.Replace("'", "''")}'")
            .FirstAsync(cancellationToken);

        return result > 0;
    }
}
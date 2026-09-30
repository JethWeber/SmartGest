using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public sealed class AuditService
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    private readonly TokenStore _store;
    private bool _initialized;

    public AuditService(IDbContextFactory<SmartGestDbContext> factory, TokenStore store)
    {
        _factory = factory;
        _store = store;
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AuditLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Utilizador TEXT NOT NULL,
                Acao TEXT NOT NULL,
                Entidade TEXT NULL,
                EntidadeId TEXT NULL,
                Detalhes TEXT NULL,
                DataUtc TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_AuditLog_DataUtc ON AuditLog(DataUtc);
            """, cancellationToken);
        _initialized = true;
    }

    public async Task RegistarAsync(string acao, string? entidade = null, string? entidadeId = null,
        string? detalhes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureSchemaAsync(cancellationToken);
            await using var db = await _factory.CreateDbContextAsync(cancellationToken);
            var utilizador = string.IsNullOrWhiteSpace(_store.Nome) ? "Sistema" : _store.Nome;
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO AuditLog (Utilizador, Acao, Entidade, EntidadeId, Detalhes, DataUtc) VALUES ({utilizador}, {acao}, {entidade}, {entidadeId}, {detalhes}, {DateTime.UtcNow:O})",
                cancellationToken);
        }
        catch (Exception ex)
        {
            AppLogService.Error("Falha ao registar auditoria.", ex);
        }
    }
}

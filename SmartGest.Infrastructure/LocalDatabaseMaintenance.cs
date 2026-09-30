
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Infrastructure;

public sealed class LocalDatabaseMaintenance
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    private readonly string _databasePath;

    public LocalDatabaseMaintenance(IDbContextFactory<SmartGestDbContext> factory, string databasePath)
    {
        _factory = factory;
        _databasePath = Path.GetFullPath(databasePath);
    }

    public string DatabasePath => _databasePath;

    public async Task<string> BackupAsync(string? destinationDirectory = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_databasePath))
            throw new FileNotFoundException("A base de dados local ainda não existe.", _databasePath);

        var directory = destinationDirectory ?? Path.Combine(
            Path.GetDirectoryName(_databasePath)!,
            "Backups");

        Directory.CreateDirectory(directory);

        var file = Path.Combine(directory, $"smartgest_{DateTime.Now:yyyyMMdd_HHmmss}.db");
        if (File.Exists(file))
            File.Delete(file);

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await db.Database.OpenConnectionAsync(cancellationToken);

        var escaped = file.Replace("'", "''");
        await db.Database.ExecuteSqlRawAsync($"VACUUM INTO '{escaped}'", cancellationToken);

        return file;
    }

    public async Task<string?> CreateAutomaticBackupIfNeededAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(Path.GetDirectoryName(_databasePath)!, "Backups");
        Directory.CreateDirectory(directory);
        var today = DateTime.Now.ToString("yyyyMMdd");
        var existing = Directory.GetFiles(directory, $"smartgest_{today}_*.db");
        if (existing.Length > 0)
            return existing.OrderByDescending(File.GetLastWriteTimeUtc).First();

        return await BackupAsync(directory, cancellationToken);
    }

    public async Task<bool> CheckIntegrityAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var result = await db.Database.SqlQueryRaw<string>("PRAGMA integrity_check").FirstOrDefaultAsync(cancellationToken);
        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    public Task<List<string>> ListBackupsAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(Path.GetDirectoryName(_databasePath)!, "Backups");
        var result = Directory.Exists(directory)
            ? Directory.GetFiles(directory, "smartgest_*.db")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .ToList()
            : new List<string>();

        return Task.FromResult(result);
    }

    public async Task RestoreAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("Backup não encontrado.", backupPath);

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await db.Database.CloseConnectionAsync();

        File.Copy(backupPath, _databasePath, overwrite: true);
    }
}
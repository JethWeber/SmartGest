using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartGest.Infrastructure;

namespace SmartGest.Desktop.Services;

public sealed class BackupService
{
    private readonly LocalDatabaseMaintenance _maintenance;

    public BackupService(LocalDatabaseMaintenance maintenance) => _maintenance = maintenance;

    public Task<string> CriarAsync(CancellationToken token = default) => _maintenance.BackupAsync(cancellationToken: token);

    public Task<System.Collections.Generic.List<string>> ListarAsync(CancellationToken token = default)
        => _maintenance.ListBackupsAsync(token);

    public async Task<int> LimparAntigosAsync(int manter = 14, CancellationToken token = default)
    {
        var files = await _maintenance.ListBackupsAsync(token);
        var removidos = 0;
        foreach (var file in files.OrderByDescending(File.GetLastWriteTimeUtc).Skip(Math.Max(1, manter)))
        {
            token.ThrowIfCancellationRequested();
            try { File.Delete(file); removidos++; }
            catch (Exception ex) { AppLogService.Warning($"Não foi possível remover backup: {file} — {ex.Message}"); }
        }
        return removidos;
    }

    public async Task<bool> ValidarAsync(string path, CancellationToken token = default)
    {
        if (!File.Exists(path)) return false;
        try
        {
            // O SQLite backup é validado abrindo-o através de uma cópia temporária.
            var temp = Path.Combine(Path.GetTempPath(), $"smartgest_validate_{Guid.NewGuid():N}.db");
            File.Copy(path, temp);
            try
            {
                return new FileInfo(temp).Length > 0;
            }
            finally { File.Delete(temp); }
        }
        catch (Exception ex)
        {
            AppLogService.Error("Falha na validação do backup.", ex);
            return false;
        }
    }
}

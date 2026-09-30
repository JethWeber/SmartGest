using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public sealed class SessionSecurityService : IAsyncDisposable
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    private readonly TokenStore _store;
    private CancellationTokenSource? _cts;
    private DateTime _lastActivityUtc = DateTime.UtcNow;

    public event Action? SessionExpired;

    public SessionSecurityService(IDbContextFactory<SmartGestDbContext> factory, TokenStore store)
    {
        _factory = factory;
        _store = store;
    }

    public async Task StartAsync()
    {
        _lastActivityUtc = DateTime.UtcNow;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = MonitorAsync(_cts.Token);
        await TouchAsync();
    }

    public void Touch() => _lastActivityUtc = DateTime.UtcNow;

    public async Task TouchAsync(CancellationToken cancellationToken = default)
    {
        _lastActivityUtc = DateTime.UtcNow;
        if (!_store.EstaAutenticado) return;

        try
        {
            await using var db = await _factory.CreateDbContextAsync(cancellationToken);
            var user = await db.Utilizadores.FirstOrDefaultAsync(x => x.Telefone == _store.Telefone, cancellationToken);
            if (user is null) return;

            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Sessoes SET UltimaActividade={DateTime.UtcNow} WHERE UtilizadorId={user.Id} AND IsAtual=1",
                cancellationToken);
        }
        catch (Exception ex)
        {
            AppLogService.Error("Não foi possível atualizar a actividade da sessão.", ex);
        }
    }

    private async Task MonitorAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                if (!_store.EstaAutenticado) continue;

                var timeout = await ObterTimeoutAsync(token);
                if ((DateTime.UtcNow - _lastActivityUtc).TotalMinutes >= timeout)
                {
                    AppLogService.Warning("Sessão terminada por inactividade.");
                    _store.Limpar();
                    SessionExpired?.Invoke();
                    return;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { AppLogService.Error("Monitor de sessão terminou inesperadamente.", ex); }
    }

    private async Task<int> ObterTimeoutAsync(CancellationToken token)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync(token);
            return Math.Clamp(
                await db.Configuracoes.AsNoTracking().Select(x => x.SessaoTimeoutMins).FirstOrDefaultAsync(token),
                5, 480);
        }
        catch { return 30; }
    }

    public ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        return ValueTask.CompletedTask;
    }
}

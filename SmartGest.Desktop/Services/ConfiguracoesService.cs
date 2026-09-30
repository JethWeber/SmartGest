using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public sealed class ConfiguracoesService
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    public ConfiguracoesService(IDbContextFactory<SmartGestDbContext> factory) => _factory = factory;

    public async Task<Snapshot> ObterAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var empresa = await db.Empresas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
        var config = await db.Configuracoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
        var users = await db.Utilizadores.AsNoTracking().OrderBy(x => x.Id).ToListAsync();

        return new(
            empresa?.Nome ?? "", empresa?.NIF ?? "", empresa?.Morada ?? "", empresa?.Cidade ?? "",
            empresa?.Pais ?? "Angola", empresa?.Telefone ?? "", empresa?.Email ?? "", empresa?.Website ?? "",
            empresa?.Capital ?? 0, empresa?.LogoPath ?? "",
            config?.TemaIndex ?? 0, config?.IdiomaIndex ?? 0, config?.MoedaIndex ?? 0,
            config?.DataFormatoIndex ?? 0, config?.MostrarSparklines ?? true,
            config?.AnimacoesAtivadas ?? true, config?.MostrarSaldosOcultos ?? false,
            config?.NotifEmail ?? true, config?.NotifApp ?? true, config?.NotifSaldoBaixo ?? true,
            config?.NotifLancamentos ?? true, config?.NotifRelatorios ?? false,
            config?.NotifErrosSistema ?? true, config?.NotifBackup ?? true,
            config?.EmailNotificacoes ?? "", config?.LimiarSaldoBaixo ?? 500000m,
            config?.DoisFatoresAtivo ?? false, config?.SessaoTimeoutMins ?? 30,
            config?.RegistarAuditoria ?? true,
            users.Select(x => new UserItem(x.Nome,x.Email,x.Perfil,x.Activo,x.Iniciais,x.CorAvatar)).ToList());
    }

    public async Task GuardarEmpresaAsync(string nome,string nif,string morada,string cidade,string pais,string telefone,string email,string website,decimal capital,string logoPath)
    {
        await using var db=await _factory.CreateDbContextAsync();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Empresas SET Nome={nome}, NIF={nif}, Morada={morada}, Cidade={cidade}, Pais={pais}, Telefone={telefone}, Email={email}, Website={website}, Capital={capital}, LogoPath={logoPath} WHERE Id=1");
    }

    public async Task GuardarConfiguracaoAsync(int tema,int idioma,int moeda,int formato,bool sparklines,bool animacoes,bool saldosOcultos,
        bool notifEmail,bool notifApp,bool notifSaldo,bool notifLanc,bool notifRel,bool notifErros,bool notifBackup,string emailNotificacoes,decimal limiar,bool doisFatores,int timeout,bool auditoria)
    {
        await using var db=await _factory.CreateDbContextAsync();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Configuracoes SET TemaIndex={tema}, IdiomaIndex={idioma}, MoedaIndex={moeda}, DataFormatoIndex={formato}, MostrarSparklines={sparklines}, AnimacoesAtivadas={animacoes}, MostrarSaldosOcultos={saldosOcultos}, NotifEmail={notifEmail}, NotifApp={notifApp}, NotifSaldoBaixo={notifSaldo}, NotifLancamentos={notifLanc}, NotifRelatorios={notifRel}, NotifErrosSistema={notifErros}, NotifBackup={notifBackup}, EmailNotificacoes={emailNotificacoes}, LimiarSaldoBaixo={limiar}, DoisFatoresAtivo={doisFatores}, SessaoTimeoutMins={timeout}, RegistarAuditoria={auditoria} WHERE Id=1");
    }

    public record Snapshot(string EmpresaNome,string EmpresaNif,string EmpresaMorada,string EmpresaCidade,string EmpresaPais,string EmpresaTelefone,string EmpresaEmail,string EmpresaWebsite,decimal EmpresaCapital,string LogoPath,
        int TemaIndex,int IdiomaIndex,int MoedaIndex,int DataFormatoIndex,bool MostrarSparklines,bool AnimacoesAtivadas,bool MostrarSaldosOcultos,
        bool NotifEmail,bool NotifApp,bool NotifSaldoBaixo,bool NotifLancamentos,bool NotifRelatorios,bool NotifErrosSistema,bool NotifBackup,
        string EmailNotificacoes,decimal LimiarSaldoBaixo,bool DoisFatoresAtivo,int SessaoTimeoutMins,bool RegistarAuditoria,List<UserItem> Utilizadores);
    public record UserItem(string Nome,string Email,string Perfil,bool Activo,string Iniciais,string CorAvatar);
}
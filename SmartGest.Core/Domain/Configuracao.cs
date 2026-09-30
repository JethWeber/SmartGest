namespace SmartGest.Core.Domain;

public sealed class Configuracao
{
    public int Id { get; private set; }
    public int TemaIndex { get; private set; } = 0;
    public int IdiomaIndex { get; private set; } = 0;
    public int MoedaIndex { get; private set; } = 0;
    public int DataFormatoIndex { get; private set; } = 0;
    public bool MostrarSparklines { get; private set; } = true;
    public bool AnimacoesAtivadas { get; private set; } = true;
    public bool MostrarSaldosOcultos { get; private set; }
    public bool NotifEmail { get; private set; } = true;
    public bool NotifApp { get; private set; } = true;
    public bool NotifSaldoBaixo { get; private set; } = true;
    public bool NotifLancamentos { get; private set; } = true;
    public bool NotifRelatorios { get; private set; }
    public bool NotifErrosSistema { get; private set; } = true;
    public bool NotifBackup { get; private set; } = true;
    public string EmailNotificacoes { get; private set; } = "";
    public decimal LimiarSaldoBaixo { get; private set; } = 500_000m;
    public bool DoisFatoresAtivo { get; private set; }
    public int SessaoTimeoutMins { get; private set; } = 30;
    public bool RegistarAuditoria { get; private set; } = true;
}

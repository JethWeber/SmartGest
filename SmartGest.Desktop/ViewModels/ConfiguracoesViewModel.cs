using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartGest.Desktop.ViewModels;

public partial class ConfiguracoesViewModel : ViewModelBase
{
    // ── Tab activa ────────────────────────────────────────────────────────────
    [ObservableProperty] private int _tabIndex = 0;

    // ════════════════════════════════════════════════════════════════════════
    // TAB 0 · PERFIL DA EMPRESA
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _empresaNome       = "SmartGest, Lda.";
    [ObservableProperty] private string _empresaNif        = "5417000001";
    [ObservableProperty] private string _empresaMorada     = "Rua da Missão, 42 · Luanda Sul";
    [ObservableProperty] private string _empresaCidade     = "Luanda";
    [ObservableProperty] private string _empresaPais       = "Angola";
    [ObservableProperty] private string _empresaTelefone   = "+244 923 000 000";
    [ObservableProperty] private string _empresaEmail      = "geral@smartgest.ao";
    [ObservableProperty] private string _empresaWebsite    = "www.smartgest.ao";
    [ObservableProperty] private string _empresaCapital    = "10.000.000";
    [ObservableProperty] private string _logoCaminho       = string.Empty;
    [ObservableProperty] private bool   _temLogo           = false;

    // ════════════════════════════════════════════════════════════════════════
    // TAB 1 · UTILIZADORES E PERMISSÕES
    // ════════════════════════════════════════════════════════════════════════

    public ObservableCollection<UtilizadorItem> Utilizadores { get; } = new()
    {
        new("Augusto Barbosa",  "augusto@smartgest.ao",  "Administrador", true,  "AB", "#1A2E5A"),
        new("Maria Fernandes",  "maria@smartgest.ao",    "Contabilista",  true,  "MF", "#2E7D32"),
        new("João Sebastião",   "joao@smartgest.ao",     "Operador",      true,  "JS", "#1565C0"),
        new("Carla Monteiro",   "carla@smartgest.ao",    "Visualizador",  false, "CM", "#6A1B9A"),
    };

    [ObservableProperty] private UtilizadorItem? _utilizadorSelecionado;

    // Painel de edição rápida
    [ObservableProperty] private string _novoUtilNome   = string.Empty;
    [ObservableProperty] private string _novoUtilEmail  = string.Empty;
    [ObservableProperty] private int    _novoUtilPerfilIndex = 2; // 0=Admin,1=Contabilista,2=Operador,3=Visualizador

    public ObservableCollection<string> Perfis { get; } = new()
    {
        "Administrador", "Contabilista", "Operador", "Visualizador"
    };

    // ════════════════════════════════════════════════════════════════════════
    // TAB 2 · APARÊNCIA
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private int  _temaIndex    = 0;   // 0=Claro, 1=Escuro, 2=Sistema
    [ObservableProperty] private int  _idiomaIndex  = 0;   // 0=Português, 1=Inglês
    [ObservableProperty] private int  _moedaIndex   = 0;   // 0=Kzs, 1=USD, 2=EUR
    [ObservableProperty] private int  _dataFormatoIndex = 0; // 0=dd/MM/yyyy, 1=MM/dd/yyyy
    [ObservableProperty] private bool _mostrarSparklines    = true;
    [ObservableProperty] private bool _animacoesAtivadas    = true;
    [ObservableProperty] private bool _mostrarSaldosOcultos = false;

    // ════════════════════════════════════════════════════════════════════════
    // TAB 3 · NOTIFICAÇÕES
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private bool _notifEmailAtivo     = true;
    [ObservableProperty] private bool _notifAppAtivo       = true;
    [ObservableProperty] private bool _notifSaldoBaixo     = true;
    [ObservableProperty] private bool _notifLancamentos    = true;
    [ObservableProperty] private bool _notifRelatorios     = false;
    [ObservableProperty] private bool _notifErrosSistema   = true;
    [ObservableProperty] private bool _notifBackup         = true;
    [ObservableProperty] private string _emailNotificacoes = "alertas@smartgest.ao";
    [ObservableProperty] private string _limiarSaldoBaixo  = "500.000";

    // ════════════════════════════════════════════════════════════════════════
    // TAB 4 · SEGURANÇA
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _senhaAtual        = string.Empty;
    [ObservableProperty] private string _senhaNova         = string.Empty;
    [ObservableProperty] private string _senhaConfirmacao  = string.Empty;
    [ObservableProperty] private bool   _doisFatoresAtivo  = false;
    [ObservableProperty] private string _sessaoTempomins   = "30";
    [ObservableProperty] private bool   _registarAuditoria = true;
    [ObservableProperty] private string _erroSeguranca     = string.Empty;
    [ObservableProperty] private bool   _temErroSeguranca  = false;
    [ObservableProperty] private string _sucessoSeguranca  = string.Empty;
    [ObservableProperty] private bool   _temSucessoSeguranca = false;

    public ObservableCollection<SessaoItem> SessoesActivas { get; } = new()
    {
        new("Chrome · Windows 11",    "Luanda, AO",  "Agora",            true),
        new("SmartGest Desktop",      "Luanda, AO",  "Há 2 horas",       false),
        new("Firefox · Ubuntu 22",    "Lisboa, PT",  "Há 3 dias",        false),
    };

    // ════════════════════════════════════════════════════════════════════════
    // TAB 5 · INTEGRAÇÃO / API
    // ════════════════════════════════════════════════════════════════════════

    [ObservableProperty] private string _apiBaseUrl     = "https://api.smartgest.ao/v1";
    [ObservableProperty] private string _apiKey         = "sg_live_●●●●●●●●●●●●●●●●●●●●●●●";
    [ObservableProperty] private bool   _apiKeyVisivel  = false;
    [ObservableProperty] private int    _timeoutIndex   = 1;   // 0=10s, 1=30s, 2=60s
    [ObservableProperty] private bool   _tlsAtivado     = true;
    [ObservableProperty] private bool   _retryAtivado   = true;
    [ObservableProperty] private string _estadoConexao  = "A verificar...";
    [ObservableProperty] private string _corEstadoConexao = "#9AA0AB";
    [ObservableProperty] private string _fundoEstadoConexao = "#F4F6FA";
    [ObservableProperty] private bool   _testandoConexao    = false;

    // Webhooks
    public ObservableCollection<WebhookItem> Webhooks { get; } = new()
    {
        new("Novo Lançamento",   "https://hooks.n8n.io/webhook/lancamento",  true),
        new("Relatório Gerado",  "https://hooks.n8n.io/webhook/relatorio",   false),
    };

    // ── Estado global de gravação ─────────────────────────────────────────────
    [ObservableProperty] private bool   _isLoading    = false;
    [ObservableProperty] private string _feedbackMsg  = string.Empty;
    [ObservableProperty] private bool   _temFeedback  = false;
    [ObservableProperty] private bool   _feedbackOk   = false;

    // ════════════════════════════════════════════════════════════════════════
    // COMANDOS
    // ════════════════════════════════════════════════════════════════════════

    // ── Perfil ────────────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task SalvarPerfilAsync()
    {
        await SimularGuardar("Perfil da empresa actualizado com sucesso.");
    }

    [RelayCommand]
    private void EscolherLogo()
    {
        // TODO: abrir file picker para imagem do logo
    }

    [RelayCommand]
    private void RemoverLogo()
    {
        LogoCaminho = string.Empty;
        TemLogo     = false;
    }

    // ── Utilizadores ─────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task ConvidarUtilizadorAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoUtilNome) || string.IsNullOrWhiteSpace(NovoUtilEmail))
            return;

        var initials = ObterIniciais(NovoUtilNome);
        var cores    = new[] { "#1A2E5A", "#2E7D32", "#1565C0", "#E65100", "#6A1B9A" };
        var cor      = cores[Utilizadores.Count % cores.Length];
        var perfil   = Perfis[NovoUtilPerfilIndex];

        Utilizadores.Add(new(NovoUtilNome, NovoUtilEmail, perfil, true, initials, cor));

        NovoUtilNome  = string.Empty;
        NovoUtilEmail = string.Empty;

        await SimularGuardar($"Convite enviado para {NovoUtilEmail}.");
    }

    [RelayCommand]
    private void ToggleUtilizador(UtilizadorItem? item)
    {
        if (item is null) return;
        var idx = Utilizadores.IndexOf(item);
        if (idx < 0) return;
        Utilizadores[idx] = item with { Activo = !item.Activo };
    }

    [RelayCommand]
    private void RemoverUtilizador(UtilizadorItem? item)
    {
        if (item is not null) Utilizadores.Remove(item);
    }

    // ── Aparência ─────────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task SalvarAparenciaAsync()
    {
        await SimularGuardar("Preferências de aparência guardadas.");
    }

    // ── Notificações ─────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task SalvarNotificacoesAsync()
    {
        await SimularGuardar("Configurações de notificações actualizadas.");
    }

    // ── Segurança ─────────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task AlterarSenhaAsync()
    {
        TemErroSeguranca     = false;
        TemSucessoSeguranca  = false;

        if (string.IsNullOrWhiteSpace(SenhaAtual))
        { ErroSeguranca = "Insira a senha actual."; TemErroSeguranca = true; return; }

        if (SenhaNova.Length < 8)
        { ErroSeguranca = "A nova senha deve ter pelo menos 8 caracteres."; TemErroSeguranca = true; return; }

        if (SenhaNova != SenhaConfirmacao)
        { ErroSeguranca = "As senhas não coincidem."; TemErroSeguranca = true; return; }

        IsLoading = true;
        await Task.Delay(900);
        IsLoading         = false;
        SenhaAtual        = string.Empty;
        SenhaNova         = string.Empty;
        SenhaConfirmacao  = string.Empty;
        SucessoSeguranca  = "Senha alterada com sucesso.";
        TemSucessoSeguranca = true;
    }

    [RelayCommand]
    private void EncerrarSessao(SessaoItem? item)
    {
        if (item is not null && !item.IsAtual)
            SessoesActivas.Remove(item);
    }

    [RelayCommand]
    private async Task SalvarSegurancaAsync()
    {
        await SimularGuardar("Configurações de segurança guardadas.");
    }

    // ── API / Integração ──────────────────────────────────────────────────────
    [RelayCommand]
    private void ToggleApiKey()
    {
        ApiKeyVisivel = !ApiKeyVisivel;
        ApiKey = ApiKeyVisivel
            ? "sg_live_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
            : "sg_live_●●●●●●●●●●●●●●●●●●●●●●●";
    }

    [RelayCommand]
    private async Task TestarConexaoAsync()
    {
        TestandoConexao  = true;
        EstadoConexao    = "A verificar...";
        CorEstadoConexao = "#9AA0AB";
        FundoEstadoConexao = "#F4F6FA";

        await Task.Delay(1400);

        TestandoConexao    = false;
        EstadoConexao      = "Conectado  ✓";
        CorEstadoConexao   = "#2E7D32";
        FundoEstadoConexao = "#E8F5E9";
    }

    [RelayCommand]
    private async Task SalvarApiAsync()
    {
        await SimularGuardar("Configurações de API guardadas.");
    }

    [RelayCommand]
    private void AdicionarWebhook()
    {
        Webhooks.Add(new("Novo Webhook", string.Empty, false));
    }

    [RelayCommand]
    private void RemoverWebhook(WebhookItem? item)
    {
        if (item is not null) Webhooks.Remove(item);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task SimularGuardar(string mensagem)
    {
        IsLoading   = true;
        TemFeedback = false;
        await Task.Delay(700);
        IsLoading    = false;
        FeedbackMsg  = mensagem;
        FeedbackOk   = true;
        TemFeedback  = true;

        await Task.Delay(3500);
        TemFeedback = false;
    }

    private static string ObterIniciais(string nome)
    {
        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length >= 2
            ? $"{partes[0][0]}{partes[^1][0]}"
            : nome.Length >= 2 ? nome[..2].ToUpper() : nome.ToUpper();
    }
}

// ── Records de suporte ────────────────────────────────────────────────────────

public record UtilizadorItem(
    string Nome,
    string Email,
    string Perfil,
    bool   Activo,
    string Iniciais,
    string CorAvatar)
{
    public string CorPerfilBadge => Perfil switch
    {
        "Administrador" => "#E53935",
        "Contabilista"  => "#2E7D32",
        "Operador"      => "#1565C0",
        _               => "#757575"
    };
    public string FundoPerfilBadge => Perfil switch
    {
        "Administrador" => "#FFEBEE",
        "Contabilista"  => "#E8F5E9",
        "Operador"      => "#E3F2FD",
        _               => "#F5F5F5"
    };
    public string TextoEstado => Activo ? "Activo"   : "Inactivo";
    public string CorEstado   => Activo ? "#43A047"  : "#9AA0AB";
    public string FundoEstado => Activo ? "#E8F5E9"  : "#F4F6FA";
}

public record SessaoItem(
    string Dispositivo,
    string Localizacao,
    string UltimaActividade,
    bool   IsAtual);

public record WebhookItem(string Evento, string Url, bool Activo);

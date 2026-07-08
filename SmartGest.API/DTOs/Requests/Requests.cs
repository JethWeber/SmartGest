namespace SmartGest.API.DTOs.Requests;

public record LoginRequest(string Telefone, string Password);
public record AlterarSenhaRequest(string SenhaAtual, string SenhaNova, string SenhaConfirmacao);

public record EmpresaRequest(
    string Nome, string NIF, string Morada, string Cidade, string Pais,
    string Telefone, string Email, string Website, decimal Capital);

public record AparenciaRequest(
    int TemaIndex, int IdiomaIndex, int MoedaIndex, int DataFormatoIndex,
    bool MostrarSparklines, bool AnimacoesAtivadas, bool MostrarSaldosOcultos);

public record NotificacoesRequest(
    bool NotifEmail, bool NotifApp, bool NotifSaldoBaixo, bool NotifLancamentos,
    bool NotifRelatorios, bool NotifErrosSistema, bool NotifBackup,
    string EmailNotificacoes, decimal LimiarSaldoBaixo);

public record SegurancaRequest(
    bool DoisFatoresAtivo, int SessaoTimeoutMins, bool RegistarAuditoria);

public record ApiIntegracaoRequest(
    string ApiBaseUrl, int TimeoutIndex, bool TlsAtivado, bool RetryAtivado);

public record CriarUtilizadorRequest(
    string Nome, string Email, string Telefone, string Perfil, string Password);

public record AtualizarUtilizadorRequest(
    string Nome, string Email, string Perfil, bool Activo);

public record ContaContabilRequest(
    string Codigo, string Nome, string Grupo, bool IsDevedora, bool? Corrente = null);

public record LancamentoRequest(
    DateTime Data,
    string   Descricao,
    string   Tipo,
    decimal  Valor,
    int      CategoriaId,          // obrigatório — não é mais nullable
    string?  Beneficiario,
    string?  MetodoPagamento,
    string?  CaminhoDocumento,
    string?  Observacoes,
    string?  CentroCusto,
    string?  ReferenciaInterna,
    int?     ContaBancariaId);

public record ContaBancariaRequest(
    string Banco, string NIB, string Tipo, string Moeda,
    decimal SaldoAtual, string Agencia, string Titular, string CorAccent,
    int? ContaContabilId = null,
    DateTime? DataAbertura = null);

public record MovimentoBancarioRequest(
    int ContaBancariaId, DateTime Data, string Descricao,
    string Referencia, string Tipo, decimal Valor);

public record WebhookRequest(string Evento, string Url, bool Activo);

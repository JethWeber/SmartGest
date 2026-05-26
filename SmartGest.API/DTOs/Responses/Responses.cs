namespace SmartGest.API.DTOs.Responses;

public record LoginResponse(string Token, string Nome, string Perfil, string Iniciais, string CorAvatar);

public record UtilizadorResponse(
    int Id, string Nome, string Email, string Telefone,
    string Perfil, bool Activo, string Iniciais, string CorAvatar, DateTime CriadoEm);

public record EmpresaResponse(
    int Id, string Nome, string NIF, string Morada, string Cidade, string Pais,
    string Telefone, string Email, string Website, decimal Capital, string? LogoPath);

public record ConfiguracaoResponse(
    int Id,
    int TemaIndex, int IdiomaIndex, int MoedaIndex, int DataFormatoIndex,
    bool MostrarSparklines, bool AnimacoesAtivadas, bool MostrarSaldosOcultos,
    bool NotifEmail, bool NotifApp, bool NotifSaldoBaixo, bool NotifLancamentos,
    bool NotifRelatorios, bool NotifErrosSistema, bool NotifBackup,
    string EmailNotificacoes, decimal LimiarSaldoBaixo,
    bool DoisFatoresAtivo, int SessaoTimeoutMins, bool RegistarAuditoria,
    string ApiBaseUrl, int TimeoutIndex, bool TlsAtivado, bool RetryAtivado);

public record ContaContabilResponse(
    int Id, string Codigo, string Nome, string Grupo, bool IsDevedora, bool Activa);

public record BalanceteItemResponse(
    string Codigo, string Nome, string Grupo,
    decimal SaldoAnteriorDebito, decimal SaldoAnteriorCredito,
    decimal MovDebito, decimal MovCredito,
    decimal SaldoFinalDebito, decimal SaldoFinalCredito);

public record BalancoResponse(
    List<BalancoLinhaResponse> AtivoCorrentes,
    List<BalancoLinhaResponse> AtivoNaoCorrentes,
    List<BalancoLinhaResponse> PassivosCorrentes,
    List<BalancoLinhaResponse> PassivosNaoCorrentes,
    List<BalancoLinhaResponse> CapitalProprio,
    decimal TotalAtivo, decimal TotalPassivo,
    decimal TotalCapitalProprio, decimal TotalPassivoMaisCapital);

public record BalancoLinhaResponse(string Descricao, decimal Valor, bool IsDeducao = false);

public record DreItemResponse(
    string Codigo, string Descricao, string Grupo,
    decimal ValorOrcado, decimal ValorRealizado, bool IsReceita, DateTime DataOrigem);

public record DreSumarioResponse(
    decimal TotalReceitas, decimal TotalCustos, decimal ResultadoLiquido,
    List<DreItemResponse> Linhas);

public record LancamentoResponse(
    int Id, DateTime Data, string Descricao, string Categoria,
    string Tipo, decimal Valor, string Beneficiario, string MetodoPagamento,
    string CaminhoDocumento, string Observacoes, string CentroCusto,
    string ReferenciaInterna, DateTime CriadoEm,
    int? ContaBancariaId, string? ContaBancariaNome);

public record DashboardResponse(
    decimal TotalReceita, decimal TotalDespesa, decimal LucroLiquido,
    List<FluxoMensalItem> FluxoMensal,
    List<LancamentoResponse> UltimasMovimentacoes);

public record FluxoMensalItem(string Mes, decimal Receita, decimal Despesa, decimal Lucro);

public record ContaBancariaResponse(
    int Id, string Banco, string NIB, string Tipo, string Moeda,
    decimal SaldoAtual, decimal SaldoOntem, string Agencia,
    string Titular, string CorAccent, bool Activa, string Iniciais);

public record ContasBancariasSumarioResponse(
    decimal SaldoConsolidado, int TotalContas, int MovimentosMes,
    List<ContaBancariaResponse> Contas);

public record MovimentoBancarioResponse(
    int Id, int ContaBancariaId, string Banco,
    DateTime Data, string Descricao, string Referencia,
    string Tipo, decimal Valor);

public record WebhookResponse(int Id, string Evento, string Url, bool Activo);

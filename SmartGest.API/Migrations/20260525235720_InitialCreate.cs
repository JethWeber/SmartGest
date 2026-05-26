using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartGest.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemaIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    IdiomaIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    MoedaIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    DataFormatoIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    MostrarSparklines = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnimacoesAtivadas = table.Column<bool>(type: "INTEGER", nullable: false),
                    MostrarSaldosOcultos = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifApp = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifSaldoBaixo = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifLancamentos = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifRelatorios = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifErrosSistema = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifBackup = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailNotificacoes = table.Column<string>(type: "TEXT", nullable: false),
                    LimiarSaldoBaixo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DoisFatoresAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessaoTimeoutMins = table.Column<int>(type: "INTEGER", nullable: false),
                    RegistarAuditoria = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    TimeoutIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    TlsAtivado = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetryAtivado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NIB = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Moeda = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoOntem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Agencia = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Titular = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CorAccent = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasBancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasContabeis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Grupo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsDevedora = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasContabeis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NIF = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Morada = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Capital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Perfil = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "Operador"),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Iniciais = table.Column<string>(type: "TEXT", nullable: false),
                    CorAvatar = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Evento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Beneficiario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MetodoPagamento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CaminhoDocumento = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CentroCusto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReferenciaInterna = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MovimentosBancarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentosBancarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentosBancarios_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UtilizadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dispositivo = table.Column<string>(type: "TEXT", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    UltimaActividade = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAtual = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessoes_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LancamentoDetalhes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LancamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaContabilId = table.Column<int>(type: "INTEGER", nullable: false),
                    Debito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credito = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentoDetalhes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentoDetalhes_ContasContabeis_ContaContabilId",
                        column: x => x.ContaContabilId,
                        principalTable: "ContasContabeis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentoDetalhes_Lancamentos_LancamentoId",
                        column: x => x.LancamentoId,
                        principalTable: "Lancamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Configuracoes",
                columns: new[] { "Id", "AnimacoesAtivadas", "ApiBaseUrl", "ApiKey", "DataFormatoIndex", "DoisFatoresAtivo", "EmailNotificacoes", "IdiomaIndex", "LimiarSaldoBaixo", "MoedaIndex", "MostrarSaldosOcultos", "MostrarSparklines", "NotifApp", "NotifBackup", "NotifEmail", "NotifErrosSistema", "NotifLancamentos", "NotifRelatorios", "NotifSaldoBaixo", "RegistarAuditoria", "RetryAtivado", "SessaoTimeoutMins", "TemaIndex", "TimeoutIndex", "TlsAtivado" },
                values: new object[] { 1, true, "", "", 0, false, "", 0, 500000m, 0, false, true, true, true, true, true, true, false, true, true, true, 30, 0, 1, true });

            migrationBuilder.InsertData(
                table: "ContasBancarias",
                columns: new[] { "Id", "Activa", "Agencia", "Banco", "CorAccent", "Moeda", "NIB", "SaldoAtual", "SaldoOntem", "Tipo", "Titular" },
                values: new object[,]
                {
                    { 1, true, "Luanda — Maianga", "Banco BIC", "#1A2E5A", "AOA", "AO06.0055.0000.1234.5678.9012.3", 4820000m, 4600000m, "Conta à Ordem", "SmartGest, Lda." },
                    { 2, true, "Luanda — Ingombota", "Banco BAI", "#0D47A1", "AOA", "AO06.0040.0000.9876.5432.1098.7", 3150000m, 3200000m, "Conta à Ordem", "SmartGest, Lda." },
                    { 3, true, "Luanda — Samba", "Banco BPC", "#1B5E20", "AOA", "AO06.0038.0000.1111.2222.3333.4", 2980000m, 2980000m, "Depósito a Prazo", "SmartGest, Lda." },
                    { 4, true, "Luanda — Talatona", "Banco ATL", "#E65100", "USD", "AO06.0006.0000.4444.5555.6666.7", 1530000m, 1490000m, "Conta à Ordem", "SmartGest, Lda." }
                });

            migrationBuilder.InsertData(
                table: "ContasContabeis",
                columns: new[] { "Id", "Activa", "Codigo", "Grupo", "IsDevedora", "Nome" },
                values: new object[,]
                {
                    { 1, true, "11", "Ativo", true, "Caixa e Equivalentes de Caixa" },
                    { 2, true, "12", "Ativo", true, "Clientes e Outras Contas a Receber" },
                    { 3, true, "13", "Ativo", true, "Inventários e Activos Biológicos" },
                    { 4, true, "14", "Ativo", true, "Activos Fixos Tangíveis" },
                    { 5, true, "15", "Ativo", true, "Activos Intangíveis" },
                    { 6, true, "21", "Passivo", false, "Fornecedores e Contas a Pagar" },
                    { 7, true, "22", "Passivo", false, "Empréstimos Bancários" },
                    { 8, true, "23", "Passivo", false, "Encargos sobre Remunerações" },
                    { 9, true, "24", "Passivo", false, "Imposto a Pagar (IRT / IVA)" },
                    { 10, true, "31", "Capital", false, "Capital Social" },
                    { 11, true, "32", "Capital", false, "Reservas Legais" },
                    { 12, true, "33", "Capital", false, "Resultados Transitados" },
                    { 13, true, "71", "Receita", false, "Vendas de Mercadorias" },
                    { 14, true, "72", "Receita", false, "Prestações de Serviços" },
                    { 15, true, "73", "Receita", false, "Outros Rendimentos Operacionais" },
                    { 16, true, "78", "Receita", false, "Proveitos e Ganhos Financeiros" },
                    { 17, true, "79", "Receita", false, "Proveitos Extraordinários" },
                    { 18, true, "61", "Despesa", true, "Custo das Mercadorias Vendidas" },
                    { 19, true, "62", "Despesa", true, "Fornecimentos e Serviços Externos" },
                    { 20, true, "63", "Despesa", true, "Gastos com Pessoal" },
                    { 21, true, "64", "Despesa", true, "Amortizações e Depreciações" },
                    { 22, true, "65", "Despesa", true, "Impostos e Taxas" },
                    { 23, true, "66", "Despesa", true, "Outros Custos Operacionais" },
                    { 24, true, "68", "Despesa", true, "Custos e Perdas Financeiras" },
                    { 25, true, "69", "Despesa", true, "Custos Extraordinários" }
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Capital", "Cidade", "Email", "LogoPath", "Morada", "NIF", "Nome", "Pais", "Telefone", "Website" },
                values: new object[] { 1, 10000000m, "Luanda", "geral@smartgest.ao", null, "Rua da Missão, 42 · Luanda Sul", "5417000001", "SmartGest, Lda.", "Angola", "+244 923 000 000", "www.smartgest.ao" });

            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Activo", "CorAvatar", "CriadoEm", "Email", "Iniciais", "Nome", "PasswordHash", "Perfil", "Telefone" },
                values: new object[] { 1, true, "#1A2E5A", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "augusto@smartgest.ao", "AB", "Augusto Barbosa", "$2a$11$W6k9CR/erDeuqTYNChrLiutX.MZ2kdzXydirBWQ82ihKjeJlSIw32", "Administrador", "900000000" });

            migrationBuilder.CreateIndex(
                name: "IX_ContasBancarias_NIB",
                table: "ContasBancarias",
                column: "NIB",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContasContabeis_Codigo",
                table: "ContasContabeis",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoDetalhes_ContaContabilId",
                table: "LancamentoDetalhes",
                column: "ContaContabilId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoDetalhes_LancamentoId",
                table: "LancamentoDetalhes",
                column: "LancamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosBancarios_ContaBancariaId",
                table: "MovimentosBancarios",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_UtilizadorId",
                table: "Sessoes",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Email",
                table: "Utilizadores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Telefone",
                table: "Utilizadores",
                column: "Telefone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "LancamentoDetalhes");

            migrationBuilder.DropTable(
                name: "MovimentosBancarios");

            migrationBuilder.DropTable(
                name: "Sessoes");

            migrationBuilder.DropTable(
                name: "Webhooks");

            migrationBuilder.DropTable(
                name: "ContasContabeis");

            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "ContasBancarias");
        }
    }
}

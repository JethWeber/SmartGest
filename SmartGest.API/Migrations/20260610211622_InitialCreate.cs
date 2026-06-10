using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "CategoriaContabeis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    ContaDebito = table.Column<string>(type: "text", nullable: false),
                    ContaCredito = table.Column<string>(type: "text", nullable: false),
                    GrupoDre = table.Column<string>(type: "text", nullable: false),
                    GrupoBalanco = table.Column<string>(type: "text", nullable: false),
                    GrupoFluxoCaixa = table.Column<string>(type: "text", nullable: false),
                    AplicaImpostoSelo = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaContabeis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemaIndex = table.Column<int>(type: "integer", nullable: false),
                    IdiomaIndex = table.Column<int>(type: "integer", nullable: false),
                    MoedaIndex = table.Column<int>(type: "integer", nullable: false),
                    DataFormatoIndex = table.Column<int>(type: "integer", nullable: false),
                    MostrarSparklines = table.Column<bool>(type: "boolean", nullable: false),
                    AnimacoesAtivadas = table.Column<bool>(type: "boolean", nullable: false),
                    MostrarSaldosOcultos = table.Column<bool>(type: "boolean", nullable: false),
                    NotifEmail = table.Column<bool>(type: "boolean", nullable: false),
                    NotifApp = table.Column<bool>(type: "boolean", nullable: false),
                    NotifSaldoBaixo = table.Column<bool>(type: "boolean", nullable: false),
                    NotifLancamentos = table.Column<bool>(type: "boolean", nullable: false),
                    NotifRelatorios = table.Column<bool>(type: "boolean", nullable: false),
                    NotifErrosSistema = table.Column<bool>(type: "boolean", nullable: false),
                    NotifBackup = table.Column<bool>(type: "boolean", nullable: false),
                    EmailNotificacoes = table.Column<string>(type: "text", nullable: false),
                    LimiarSaldoBaixo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DoisFatoresAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    SessaoTimeoutMins = table.Column<int>(type: "integer", nullable: false),
                    RegistarAuditoria = table.Column<bool>(type: "boolean", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "text", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    TimeoutIndex = table.Column<int>(type: "integer", nullable: false),
                    TlsAtivado = table.Column<bool>(type: "boolean", nullable: false),
                    RetryAtivado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasContabeis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Grupo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsDevedora = table.Column<bool>(type: "boolean", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasContabeis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NIF = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Morada = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LogoPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Perfil = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Operador"),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Iniciais = table.Column<string>(type: "text", nullable: false),
                    CorAvatar = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Evento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NIB = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Moeda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoOntem = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Agencia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Titular = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CorAccent = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    ContaContabilId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasBancarias_ContasContabeis_ContaContabilId",
                        column: x => x.ContaContabilId,
                        principalTable: "ContasContabeis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sessoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false),
                    Dispositivo = table.Column<string>(type: "text", nullable: false),
                    Localizacao = table.Column<string>(type: "text", nullable: false),
                    UltimaActividade = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsAtual = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CategoriaContabilId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Beneficiario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MetodoPagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CaminhoDocumento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CentroCusto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenciaInterna = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImpostoSelo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AanuladoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AanuladoPor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MotivoAnulacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_CategoriaContabeis_CategoriaContabilId",
                        column: x => x.CategoriaContabilId,
                        principalTable: "CategoriaContabeis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
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
                name: "LancamentoDetalhes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LancamentoId = table.Column<int>(type: "integer", nullable: false),
                    ContaContabilId = table.Column<int>(type: "integer", nullable: false),
                    Debito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Credito = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
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
                table: "CategoriaContabeis",
                columns: new[] { "Id", "AplicaImpostoSelo", "Ativo", "ContaCredito", "ContaDebito", "GrupoBalanco", "GrupoDre", "GrupoFluxoCaixa", "Nome", "Tipo" },
                values: new object[,]
                {
                    { 1, false, true, "71", "45", "", "Proveitos e Ganhos", "Operacional", "Venda de Mercadoria", "Entrada" },
                    { 2, false, true, "72", "45", "", "Proveitos e Ganhos", "Operacional", "Prestação de Serviço", "Entrada" },
                    { 3, false, true, "31", "45", "", "", "Operacional", "Recebimento de Cliente", "Entrada" },
                    { 4, false, true, "73", "45", "", "Proveitos e Ganhos", "Operacional", "Outros Rendimentos", "Entrada" },
                    { 5, false, true, "51", "45", "", "", "Financiamento", "Capital Inicial", "Entrada" },
                    { 6, false, true, "33", "45", "", "", "Financiamento", "Empréstimo Bancário", "Entrada" },
                    { 7, false, true, "78", "45", "", "Proveitos e Ganhos", "Financiamento", "Juros Recebidos", "Entrada" },
                    { 8, false, true, "79", "45", "", "Proveitos e Ganhos", "Investimento", "Venda de Ativo", "Entrada" },
                    { 9, false, true, "72", "45", "", "Proveitos e Ganhos", "Operacional", "Comissão Recebida", "Entrada" },
                    { 10, false, true, "73", "45", "", "Proveitos e Ganhos", "Operacional", "Outras Entradas", "Entrada" },
                    { 11, false, true, "45", "61", "", "Custos e Perdas", "Operacional", "Compra de Mercadoria", "Saída" },
                    { 12, false, true, "45", "32", "", "", "Operacional", "Pagamento a Fornecedor", "Saída" },
                    { 13, false, true, "45", "63", "", "Custos e Perdas", "Operacional", "Salários", "Saída" },
                    { 14, false, true, "36", "63", "", "Custos e Perdas", "Operacional", "INSS", "Saída" },
                    { 15, false, true, "34", "65", "", "Custos e Perdas", "Operacional", "IRT", "Saída" },
                    { 16, false, true, "34", "65", "", "Custos e Perdas", "Operacional", "IVA", "Saída" },
                    { 17, false, true, "34", "65", "", "Custos e Perdas", "Operacional", "Impostos e Taxas", "Saída" },
                    { 18, false, true, "45", "62", "", "Custos e Perdas", "Operacional", "Despesa Administrativa", "Saída" },
                    { 19, false, true, "45", "62", "", "Custos e Perdas", "Operacional", "Energia / Água / Internet", "Saída" },
                    { 20, false, true, "45", "62", "", "Custos e Perdas", "Operacional", "Aluguer", "Saída" },
                    { 21, false, true, "45", "62", "", "Custos e Perdas", "Operacional", "Combustível / Transportes", "Saída" },
                    { 22, false, true, "45", "11", "", "", "Investimento", "Compra de Equipamento", "Saída" },
                    { 23, false, true, "45", "68", "", "Custos e Perdas", "Financiamento", "Juros Bancários", "Saída" },
                    { 24, false, true, "45", "33", "", "", "Financiamento", "Amortização de Empréstimo", "Saída" },
                    { 25, false, true, "45", "66", "", "Custos e Perdas", "Operacional", "Outras Despesas", "Saída" }
                });

            migrationBuilder.InsertData(
                table: "Configuracoes",
                columns: new[] { "Id", "AnimacoesAtivadas", "ApiBaseUrl", "ApiKey", "DataFormatoIndex", "DoisFatoresAtivo", "EmailNotificacoes", "IdiomaIndex", "LimiarSaldoBaixo", "MoedaIndex", "MostrarSaldosOcultos", "MostrarSparklines", "NotifApp", "NotifBackup", "NotifEmail", "NotifErrosSistema", "NotifLancamentos", "NotifRelatorios", "NotifSaldoBaixo", "RegistarAuditoria", "RetryAtivado", "SessaoTimeoutMins", "TemaIndex", "TimeoutIndex", "TlsAtivado" },
                values: new object[] { 1, true, "", "", 0, false, "", 0, 500000m, 0, false, true, true, true, true, true, true, false, true, true, true, 30, 0, 1, true });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Capital", "Cidade", "Email", "LogoPath", "Morada", "NIF", "Nome", "Pais", "Telefone", "Website" },
                values: new object[] { 1, 10000000m, "Luanda", "geral@smartgest.ao", null, "Rua da Missão, 42 · Luanda Sul", "5417000001", "SmartGest, Lda.", "Angola", "+244 923 000 000", "www.smartgest.ao" });

            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Activo", "CorAvatar", "CriadoEm", "Email", "Iniciais", "Nome", "PasswordHash", "Perfil", "Telefone" },
                values: new object[] { 1, true, "#1A2E5A", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "weber@smartgest.ao", "JW", "Jeth Weber", "$2b$11$a.TPqwNMTgPENS9Up.20f.CzgkVrSgpLt0KGfl3vWEWKeq/hEK7Rq", "Administrador", "900000000" });

            migrationBuilder.CreateIndex(
                name: "IX_ContasBancarias_ContaContabilId",
                table: "ContasBancarias",
                column: "ContaContabilId");

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
                name: "IX_LancamentoDetalhe_ContaContabilId_Debito_Credito",
                table: "LancamentoDetalhes",
                columns: new[] { "ContaContabilId", "Debito", "Credito" });

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoDetalhes_LancamentoId",
                table: "LancamentoDetalhes",
                column: "LancamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamento_Anulado",
                table: "Lancamentos",
                column: "Anulado");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamento_Data",
                table: "Lancamentos",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CategoriaContabilId",
                table: "Lancamentos",
                column: "CategoriaContabilId");

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
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "CategoriaContabeis");

            migrationBuilder.DropTable(
                name: "ContasBancarias");

            migrationBuilder.DropTable(
                name: "ContasContabeis");
        }
    }
}

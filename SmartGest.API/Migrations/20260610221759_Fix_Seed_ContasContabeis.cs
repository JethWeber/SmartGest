using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartGest.API.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Seed_ContasContabeis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContasContabeis",
                columns: new[] { "Id", "Activa", "Codigo", "Grupo", "IsDevedora", "Nome" },
                values: new object[,]
                {
                    { 1, true, "11", "Ativo", true, "Activos Fixos Tangíveis" },
                    { 2, true, "12", "Ativo", true, "Activos Intangíveis" },
                    { 3, true, "13", "Ativo", true, "Investimentos Financeiros" },
                    { 4, true, "18", "Ativo", false, "Amortizações Acumuladas" },
                    { 5, true, "22", "Ativo", true, "Mercadorias" },
                    { 6, true, "26", "Ativo", true, "Matérias-Primas e Subsidiárias" },
                    { 7, true, "31", "Ativo", true, "Clientes" },
                    { 8, true, "32", "Passivo", false, "Fornecedores" },
                    { 9, true, "33", "Passivo", false, "Empréstimos Obtidos" },
                    { 10, true, "34", "Passivo", false, "Estado e Outros Entes Públicos" },
                    { 11, true, "36", "Passivo", false, "Pessoal" },
                    { 12, true, "43", "Ativo", true, "Caixa" },
                    { 13, true, "45", "Ativo", true, "Depósitos Bancários" },
                    { 14, true, "51", "Capital", false, "Capital Social" },
                    { 15, true, "55", "Capital", false, "Reservas Legais" },
                    { 16, true, "59", "Capital", false, "Resultados Transitados" },
                    { 17, true, "61", "Despesa", true, "Custo das Mercadorias Vendidas" },
                    { 18, true, "62", "Despesa", true, "Fornecimentos e Serviços de Terceiros" },
                    { 19, true, "63", "Despesa", true, "Gastos com Pessoal" },
                    { 20, true, "64", "Despesa", true, "Amortizações e Depreciações do Exercício" },
                    { 21, true, "65", "Despesa", true, "Impostos e Taxas" },
                    { 22, true, "66", "Despesa", true, "Outros Custos e Perdas Operacionais" },
                    { 23, true, "68", "Despesa", true, "Custos e Perdas Financeiras" },
                    { 24, true, "69", "Despesa", true, "Custos e Perdas Extraordinárias" },
                    { 25, true, "71", "Receita", false, "Vendas de Mercadorias e Produtos Acabados" },
                    { 26, true, "72", "Receita", false, "Prestações de Serviços" },
                    { 27, true, "73", "Receita", false, "Outros Proveitos e Ganhos Operacionais" },
                    { 28, true, "78", "Receita", false, "Proveitos e Ganhos Financeiros" },
                    { 29, true, "79", "Receita", false, "Proveitos e Ganhos Extraordinários" },
                    { 30, true, "88", "Capital", false, "Resultado Líquido do Exercício" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ContasContabeis",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}

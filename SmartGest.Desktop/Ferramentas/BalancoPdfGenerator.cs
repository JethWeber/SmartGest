using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.Ferramentas;

public static class BalancoPdfGenerator
{
    public static void Generate(BalancoApiResponse data, string periodoTexto, string destinationPath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Column(header =>
                {
                    header.Item().Text("Balanço Patrimonial").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    header.Item().Text(periodoTexto).FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Total Ativo").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalAtivo.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                        });

                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Total Passivo").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalPassivo.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.DeepOrange.Medium);
                        });

                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Capital Próprio").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalCapitalProprio.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.Green.Medium);
                        });

                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Passivo + Capital").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalPassivoMaisCapital.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.Purple.Medium);
                        });
                    });

                    AddSection(column, "Ativo Correntes", data.AtivoCorrentes);
                    AddSection(column, "Ativo Não Correntes", data.AtivoNaoCorrentes);
                    AddSection(column, "Passivos Correntes", data.PassivosCorrentes);
                    AddSection(column, "Passivos Não Correntes", data.PassivosNaoCorrentes);
                    AddSection(column, "Capital Próprio", data.CapitalProprio);
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Gerado em ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf(destinationPath);
    }

    private static void AddSection(ColumnDescriptor container, string title, IEnumerable<BalancoLinhaResponse> items)
    {
        container.Item().Text(title).FontSize(12).SemiBold().FontColor(Colors.Black);
        container.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(100);
            });

            table.Header(header =>
            {
                header.Cell().Text("Descrição").SemiBold();
                header.Cell().AlignRight().Text("Valor").SemiBold();
            });

            foreach (var item in items)
            {
                table.Cell().Text(item.Descricao).FontColor(Colors.Black);
                table.Cell().AlignRight().Text(FormatValue(item)).FontColor(item.IsDeducao ? Colors.Red.Medium : Colors.Black);
            }
        });
    }

    private static string FormatValue(BalancoLinhaResponse item)
    {
        var valor = item.Valor;
        var texto = Math.Abs(valor).ToString("N0", CultureInfo.InvariantCulture) + " Kzs";
        return item.IsDeducao ? $"({texto})" : texto;
    }
}

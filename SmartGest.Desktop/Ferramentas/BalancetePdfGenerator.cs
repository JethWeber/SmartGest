using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.Ferramentas;

public static class BalancetePdfGenerator
{
    public static void Generate(BalanceteApiResponse data, string destinationPath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Text("Balancete").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text($"Período: {data.Periodo.Inicio:dd/MM/yyyy} - {data.Periodo.Fim:dd/MM/yyyy}")
                          .FontSize(9)
                          .FontColor(Colors.Grey.Medium);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Total Débitos").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(data.TotalDebitos.ToString("N0", CultureInfo.InvariantCulture) + " Kzs")
                                .FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Total Créditos").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(data.TotalCreditos.ToString("N0", CultureInfo.InvariantCulture) + " Kzs")
                                .FontSize(12).SemiBold().FontColor(Colors.Green.Medium);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Diferença").FontSize(9).FontColor(Colors.Grey.Medium);
                            var dif = data.TotalDebitos - data.TotalCreditos;
                            col.Item().Text((dif > 0 ? "+" : string.Empty) + dif.ToString("N0", CultureInfo.InvariantCulture) + " Kzs")
                                .FontSize(12).SemiBold().FontColor(dif >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        });
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                            columns.ConstantColumn(65);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Cód.").SemiBold();
                            header.Cell().Element(CellStyle).Text("Conta").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("S. Ant. Déb.").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("S. Ant. Cre.").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Mov. Déb.").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Mov. Cre.").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("S. Final Déb.").SemiBold();
                            header.Cell().Element(CellStyle).AlignRight().Text("S. Final Cre.").SemiBold();
                        });

                        foreach (var item in data.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.Codigo).SemiBold().FontColor(Colors.Blue.Medium);
                            table.Cell().Element(CellStyle).Text(item.Nome).FontColor(Colors.Black);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.SaldoAnteriorDebito > 0 ? item.SaldoAnteriorDebito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.SaldoAnteriorCredito > 0 ? item.SaldoAnteriorCredito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.MovDebito > 0 ? item.MovDebito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.MovCredito > 0 ? item.MovCredito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.SaldoFinalDebito > 0 ? item.SaldoFinalDebito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.SaldoFinalCredito > 0 ? item.SaldoFinalCredito.ToString("N0", CultureInfo.InvariantCulture) : "–");
                        }
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Gerado em ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf(destinationPath);
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);
    }
}

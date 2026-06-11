using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.Ferramentas;

public static class DrePdfGenerator
{
    public static void Generate(DreSumarioApiResponse data, DateTime inicio, DateTime fim, string destinationPath)
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
                    header.Item().Text("Demonstrativo de Resultados (DRE)").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    header.Item().Text($"Período: {inicio:dd/MM/yyyy} - {fim:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Total Receitas").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalReceitas.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.Green.Medium);
                        });

                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Total Custos").FontSize(9).FontColor(Colors.Grey.Medium);
                            card.Item().Text(data.TotalCustos.ToString("N0", CultureInfo.InvariantCulture) + " Kzs").FontSize(12).SemiBold().FontColor(Colors.Red.Medium);
                        });

                        row.RelativeItem().Column(card =>
                        {
                            card.Item().Text("Resultado Líquido").FontSize(9).FontColor(Colors.Grey.Medium);
                            var resultado = data.ResultadoLiquido;
                            card.Item().Text(FormatResult(resultado)).FontSize(12).SemiBold().FontColor(resultado >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        });
                    });

                    column.Item().Text("Linhas da DRE").FontSize(12).SemiBold().FontColor(Colors.Black);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Cód.").SemiBold();
                            header.Cell().Text("Descrição").SemiBold();
                            header.Cell().AlignRight().Text("Orçado").SemiBold();
                            header.Cell().AlignRight().Text("Realizado").SemiBold();
                            header.Cell().AlignRight().Text("Desvio").SemiBold();
                        });

                        foreach (var item in data.Linhas)
                        {
                            table.Cell().Text(item.Codigo).FontColor(Colors.Blue.Medium);
                            table.Cell().Text(item.Descricao).FontColor(Colors.Black);
                            table.Cell().AlignRight().Text(FormatMoney(item.ValorOrcado)).FontColor(Colors.Black);
                            table.Cell().AlignRight().Text(FormatMoney(item.ValorRealizado, item.IsReceita)).FontColor(item.IsReceita ? Colors.Green.Medium : Colors.Red.Medium);
                            table.Cell().AlignRight().Text(FormatMoney(item.ValorRealizado - item.ValorOrcado, item.IsReceita)).FontColor((item.ValorRealizado - item.ValorOrcado) >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        }
                    });

                    if (data.FluxoMensal is { Count: > 0 })
                    {
                        column.Item().Text("Fluxo Mensal").FontSize(12).SemiBold().FontColor(Colors.Black);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Mês").SemiBold();
                                header.Cell().AlignRight().Text("Receita").SemiBold();
                                header.Cell().AlignRight().Text("Despesa").SemiBold();
                                header.Cell().AlignRight().Text("Lucro").SemiBold();
                            });

                            foreach (var item in data.FluxoMensal)
                            {
                                table.Cell().Text(item.Mes).FontColor(Colors.Black);
                                table.Cell().AlignRight().Text(FormatMoney(item.Receita, true)).FontColor(Colors.Green.Medium);
                                table.Cell().AlignRight().Text(FormatMoney(item.Despesa, false)).FontColor(Colors.Red.Medium);
                                table.Cell().AlignRight().Text(FormatMoney(item.Lucro, item.Lucro >= 0)).FontColor(item.Lucro >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                            }
                        });
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Gerado em ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf(destinationPath);
    }

    private static string FormatMoney(decimal value, bool positiveIsGreen = true)
    {
        var texto = Math.Abs(value).ToString("N0", CultureInfo.InvariantCulture) + " Kzs";
        if (value == 0)
            return "–";

        var sinal = value > 0 ? "+" : "-";
        return sinal + texto;
    }

    private static string FormatResult(decimal value)
    {
        if (value == 0)
            return "0 Kzs";

        var prefix = value > 0 ? "+" : "-";
        return prefix + Math.Abs(value).ToString("N0", CultureInfo.InvariantCulture) + " Kzs";
    }
}

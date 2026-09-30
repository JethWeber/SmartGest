using System;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Gera dicas de contexto para botões sem obrigar cada View a repetir a mesma configuração.
/// </summary>
public sealed class ButtonTooltipConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
    {
        var label = ExtrairTexto(value);
        if (string.IsNullOrWhiteSpace(label))
            return "Executar esta ação.";

        var normalized = label.Trim().ToLowerInvariant();
        return normalized switch
        {
            "guardar" or "guardar perfil" => "Guardar as alterações efectuadas nesta área.",
            "salvar" or "salvar alterações" => "Guardar as alterações efectuadas nesta área.",
            "cancelar" => "Cancelar a operação e fechar esta janela.",
            "fechar" => "Fechar esta janela.",
            "sair" or "sair do sistema" => "Terminar a sessão e sair do SmartGest.",
            "novo" or "novo lançamento" => "Criar um novo registo.",
            "editar" => "Editar o registo seleccionado.",
            "eliminar" or "remover" => "Remover o registo seleccionado.",
            "actualizar" or "atualizar" => "Actualizar os dados apresentados.",
            "pesquisar" or "buscar" => "Pesquisar dados.",
            "configuracoes" or "configurações" => "Abrir as configurações do SmartGest.",
            "testar conexão" or "testar conexao" => "Testar a ligação ao serviço configurado.",
            "adicionar" => "Adicionar um novo item.",
            "remover logo" => "Remover o logótipo configurado.",
            "escolher logo" => "Escolher o logótipo da empresa.",
            _ => $"Executar: {label}."
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
        => AvaloniaProperty.UnsetValue;

    private static string ExtrairTexto(object? value)
    {
        if (value is string text) return text;

        if (value is TextBlock textBlock)
            return textBlock.Text ?? string.Empty;

        if (value is ContentControl contentControl)
            return ExtrairTexto(contentControl.Content);

        if (value is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                var text = ExtrairTexto(child);
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }

        return string.Empty;
    }
}

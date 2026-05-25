using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartGest.Desktop.ViewModels;

/// <summary>
/// Retorna "#2196F3" (azul activo) quando TabIndex == ConverterParameter, "#F4F6FA" caso contrário.
/// Usado no BorderBrush das tabs para simular o underline activo.
/// </summary>
public class TabIndexConverter : IValueConverter
{
    public static readonly TabIndexConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int current && parameter is string paramStr && int.TryParse(paramStr, out int target))
            return current == target ? "#2196F3" : "Transparent";
        return "Transparent";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Retorna true quando TabIndex == ConverterParameter.
/// Usado em IsVisible para mostrar/ocultar o painel de cada tab.
/// </summary>
public class TabEqualConverter : IValueConverter
{
    public static readonly TabEqualConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int current && parameter is string paramStr && int.TryParse(paramStr, out int target))
            return current == target;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Adiciona o comando MudarTab ao ViewModel via partial class
public partial class ConfiguracoesViewModel
{
    [RelayCommand]
    private void MudarTab(string indexStr)
    {
        if (int.TryParse(indexStr, out int idx))
            TabIndex = idx;
    }
}

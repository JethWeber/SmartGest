using Avalonia;
using Avalonia.Styling;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Centraliza os quatro modos visuais do SmartGest:
/// Claro, Escuro, Sistema e SG Tema.
/// </summary>
public sealed class ThemeService
{
    public static readonly ThemeVariant SmartGest = new("SmartGest", ThemeVariant.Dark);

    public void Apply(int temaIndex)
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = temaIndex switch
        {
            0 => ThemeVariant.Light,
            1 => ThemeVariant.Dark,
            2 => ThemeVariant.Default,
            3 => SmartGest,
            _ => SmartGest
        };
    }
}

using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Styling;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Preferências locais da interface. Não contém dados empresariais ou financeiros.
/// </summary>
public sealed class ThemeService
{
    public static readonly ThemeVariant SmartGest = new("SmartGest", ThemeVariant.Dark);

    private readonly string _settingsPath;

    public ThemeService()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsPath = Path.Combine(root, "SmartGest", "settings.json");
    }

    public int LoadThemeIndex(int fallback = 3)
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return fallback;

            var settings = JsonSerializer.Deserialize<UiSettings>(File.ReadAllText(_settingsPath));
            return settings?.ThemeIndex is >= 0 and <= 3 ? settings.ThemeIndex : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    public void Apply(int temaIndex, bool persist = true)
    {
        if (global::Avalonia.Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = temaIndex switch
        {
            0 => ThemeVariant.Light,
            1 => ThemeVariant.Dark,
            2 => ThemeVariant.Default,
            3 => SmartGest,
            _ => SmartGest
        };

        if (persist)
            SaveThemeIndex(temaIndex);
    }

    private void SaveThemeIndex(int temaIndex)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);

            var settings = new UiSettings { ThemeIndex = temaIndex };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // A preferência visual não deve impedir a aplicação de arrancar.
        }
    }

    private sealed class UiSettings
    {
        public int ThemeIndex { get; set; } = 3;
    }
}

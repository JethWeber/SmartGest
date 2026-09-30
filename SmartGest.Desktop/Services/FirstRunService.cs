using System;
using System.IO;
using System.Text.Json;

namespace SmartGest.Desktop.Services;

public sealed class FirstRunService
{
    private readonly string _path;

    public FirstRunService()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _path = Path.Combine(root, "SmartGest", "onboarding.json");
    }

    public bool IsCompleted
    {
        get
        {
            try
            {
                if (!File.Exists(_path)) return false;
                var state = JsonSerializer.Deserialize<State>(File.ReadAllText(_path));
                return state?.Completed == true;
            }
            catch { return false; }
        }
    }

    public void MarkCompleted()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(new State(true), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { AppLogService.Error("Não foi possível guardar o estado de configuração inicial.", ex); }
    }

    private sealed record State(bool Completed);
}

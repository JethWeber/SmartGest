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

    public bool IsSetupCompleted
    {
        get
        {
            try
            {
                if (!File.Exists(_path)) return false;
                var state = JsonSerializer.Deserialize<State>(File.ReadAllText(_path));
                return state?.SetupCompleted == true;
            }
            catch { return false; }
        }
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

    public void Reset() => MarkCompleted(false);

    public void MarkSetupCompleted()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(ReadState() with { SetupCompleted = true }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { AppLogService.Error("Não foi possível guardar a configuração inicial.", ex); }
    }

    public void MarkCompleted() => MarkCompleted(true);

    private State ReadState()
    {
        try
        {
            if (!File.Exists(_path)) return new State(false, false);
            return JsonSerializer.Deserialize<State>(File.ReadAllText(_path)) ?? new State(false, false);
        }
        catch { return new State(false, false); }
    }

    private void MarkCompleted(bool completed)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(ReadState() with { Completed = completed }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { AppLogService.Error("Não foi possível guardar o estado de configuração inicial.", ex); }
    }

    private sealed record State(bool Completed, bool SetupCompleted = false);
}

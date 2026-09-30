using System;
using System.IO;

namespace SmartGest.Desktop.Services;

public static class AppLogService
{
    private static readonly object Sync = new();

    private static string DirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartGest", "Logs");

    private static string FilePath => Path.Combine(DirectoryPath, $"smartgest_{DateTime.Now:yyyyMMdd}.log");

    public static void Info(string message) => Write("INFO", message, null);
    public static void Warning(string message) => Write("WARN", message, null);
    public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            if (exception is not null)
                line += Environment.NewLine + exception;
            lock (Sync) File.AppendAllText(FilePath, line + Environment.NewLine);
        }
        catch
        {
            // O logging nunca deve impedir a aplicação de funcionar.
        }
    }
}

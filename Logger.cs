using System.Reflection;

namespace EasySpire;

public static class Logger
{
    private static string? _logPath;
    private static readonly object Lock = new();

    public static void Initialize()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(assemblyDir)) return;

        _logPath = Path.Combine(assemblyDir, "EasySpire.log");

        // Clear old log on startup
        try { File.WriteAllText(_logPath, ""); } catch { }

        Log("========================================");
        Log($"EasySpire v1.1.1 initialized");
        Log($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Log($"OS: {Environment.OSVersion}");
        Log($"Runtime: {Environment.Version}");
        Log($"Mod dir: {assemblyDir}");
        Log("========================================");
    }

    public static void Log(string message)
    {
        if (_logPath == null) return;
        lock (Lock)
        {
            try
            {
                File.AppendAllText(_logPath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch { }
        }
    }

    public static void LogPatchResult(string patchName, bool success, string? detail = null)
    {
        var status = success ? "OK" : "FAIL";
        var msg = $"[{status}] {patchName}";
        if (detail != null) msg += $" - {detail}";
        Log(msg);
    }

    public static void LogError(string patchName, Exception ex)
    {
        Log($"[ERROR] {patchName}: {ex.GetType().Name}: {ex.Message}");
        Log($"        Stack: {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim()}");
    }
}

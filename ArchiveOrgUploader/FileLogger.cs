namespace ArchiveOrgUploader;

/// <summary>
/// Appends timestamped lines to a plain-text logfile. Kept separate from ArchiveOrgLog.csv,
/// which only tracks per-row attempt/success timestamps — this captures the full narrative of
/// a run (requests, full response bodies, warnings, errors) for troubleshooting later.
/// </summary>
public class FileLogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public FileLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        var dir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public string LogFilePath => _logFilePath;

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level,-5}] {message}";
        lock (_lock)
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
    }
}
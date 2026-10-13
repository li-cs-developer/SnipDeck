using System.IO;

namespace SnipDeck.Core.Services;

/// <summary>
/// Appends exception details to %APPDATA%\SnipDeck\crash.log.
/// Thread-safe. Never throws — logging must never break the caller.
/// </summary>
public static class CrashLog
{
    private static readonly string Directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SnipDeck");

    private static readonly string LogPath = Path.Combine(Directory, "crash.log");

    private static readonly object Gate = new();

    /// <summary>
    /// Write an entry to crash.log. <paramref name="context"/> should be a
    /// short tag ("JsonHistoryStore.Save", "ClipboardMonitor.OnTick") so the
    /// log is greppable. <paramref name="note"/> is optional human text.
    /// </summary>
    public static void Write(string context, Exception ex, string? note = null)
    {
        try
        {
            var line = $"[{DateTime.Now:O}] [{context}] " +
                       (string.IsNullOrEmpty(note) ? "" : note + " — ") +
                       ex + Environment.NewLine + Environment.NewLine;

            lock (Gate)
            {
                System.IO.Directory.CreateDirectory(Directory);
                File.AppendAllText(LogPath, line);
            }
        }
        catch
        {
            // Logging must never throw. If we can't write to the log,
            // there's nowhere left to report it. Silent by design.
        }
    }

    /// <summary>Convenience for a bare message without an exception.</summary>
    public static void Write(string context, string message)
    {
        try
        {
            var line = $"[{DateTime.Now:O}] [{context}] {message}{Environment.NewLine}{Environment.NewLine}";

            lock (Gate)
            {
                System.IO.Directory.CreateDirectory(Directory);
                File.AppendAllText(LogPath, line);
            }
        }
        catch
        {
            // Same reasoning as above.
        }
    }
}
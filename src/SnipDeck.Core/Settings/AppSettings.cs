namespace SnipDeck.Core.Settings;

public sealed class AppSettings
{
    public static readonly int[] AllowedHistorySizes = { 50, 100, 200, 400, 800 };
    public const int DefaultHistorySize = 50;

    private int _maxHistoryItems = DefaultHistorySize;

    public int MaxHistoryItems
    {
        get => _maxHistoryItems;
        set => _maxHistoryItems = ClampToAllowed(value);
    }

    public uint HotkeyModifiers { get; set; } = 0x0002 | 0x0004;
    public uint HotkeyKey { get; set; } = 0x56;
    public bool StartWithWindows { get; set; } = false;
    public string Theme { get; set; } = "System";
    public bool ShowToasts { get; set; } = true;
    public bool UseFuzzySearch { get; set; } = true;

    /// <summary>"en-CA" or "fr-CA".</summary>
    public string Language { get; set; } = "en-CA";

    public static int ClampToAllowed(int value)
    {
        if (value <= 0) return DefaultHistorySize;

        var best = AllowedHistorySizes[0];
        var bestDist = Math.Abs(value - best);
        foreach (var tier in AllowedHistorySizes)
        {
            var dist = Math.Abs(value - tier);
            if (dist < bestDist)
            {
                best = tier;
                bestDist = dist;
            }
        }
        return best;
    }
}
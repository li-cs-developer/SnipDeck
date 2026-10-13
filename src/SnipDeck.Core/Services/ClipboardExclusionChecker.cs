using System.Windows;

namespace SnipDeck.Core.Services;

/// <summary>
/// Checks whether the current clipboard content should be excluded from
/// history. Password managers (KeePass, 1Password, Bitwarden, etc.) set a
/// custom clipboard format to signal "don't monitor this."
/// </summary>
public static class ClipboardExclusionChecker
{
    // KeePass convention; also honored by several other tools.
    private const string ExcludeFormat = "ExcludeClipboardContentFromMonitorProcessing";
    private const string CanIncludeFormat = "CanIncludeInClipboardHistory";

    public static bool ShouldExclude()
    {
        try
        {
            var data = Clipboard.GetDataObject();
            if (data is null) return false;

            // If the exclude flag is present at all → exclude.
            if (data.GetDataPresent(ExcludeFormat))
                return true;

            // Some apps (Windows 10+) set this to 0 to opt out.
            if (data.GetDataPresent(CanIncludeFormat))
            {
                var value = data.GetData(CanIncludeFormat);
                if (value is int i && i == 0) return true;
                if (value is byte b && b == 0) return true;
            }

            return false;
        }
        catch
        {
            // If we can't inspect, err on the side of excluding — safer.
            return true;
        }
    }
}
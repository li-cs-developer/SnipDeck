using System.Globalization;
using System.Resources;

namespace SnipDeck.Localization;

/// <summary>
/// Thin wrapper over the resx ResourceManager. Kept tiny so tests can call
/// Strings.Get directly without a WPF application host.
/// </summary>
public static class Strings
{
    private static readonly ResourceManager Manager = new(
        "SnipDeck.Localization.Strings",
        typeof(Strings).Assembly);

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    public static string Get(string key)
    {
        try { return Manager.GetString(key, Culture) ?? key; }
        catch { return key; }
    }

    public static string Format(string key, params object[] args)
    {
        var template = Get(key);
        try { return string.Format(Culture, template, args); }
        catch { return template; }
    }
}
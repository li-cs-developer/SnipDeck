using System.Globalization;
using System.Windows;

namespace SnipDeck.Localization;

public enum AppLanguage { EnCa, FrCa }

public static class Loc
{
    /// <summary>Raised when the language changes. XAML markup extensions subscribe to re-read.</summary>
    public static event EventHandler? LanguageChanged;

    private static AppLanguage _current = AppLanguage.EnCa;

    public static AppLanguage Current
    {
        get => _current;
        private set
        {
            _current = value;
            Strings.Culture = value switch
            {
                AppLanguage.FrCa => new CultureInfo("fr-CA"),
                _ => new CultureInfo("en-CA")
            };
        }
    }

    static Loc()
    {
        Current = AppLanguage.EnCa;
    }

    /// <summary>Apply a language tag ("en-CA" or "fr-CA"). No-op if unchanged.</summary>
    public static void Apply(string lang)
    {
        var next = string.Equals(lang, "fr-CA", StringComparison.OrdinalIgnoreCase)
            ? AppLanguage.FrCa
            : AppLanguage.EnCa;

        if (next == _current) return;

        Current = next;

        // Raise on the UI thread so binding updates happen safely.
        var app = Application.Current;
        if (app is null) { LanguageChanged?.Invoke(null, EventArgs.Empty); return; }
        app.Dispatcher.BeginInvoke(new Action(() =>
        {
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }));
    }

    /// <summary>Translate a key.</summary>
    public static string T(string key) => Strings.Get(key);

    /// <summary>Translate a key and apply <see cref="string.Format(string, object[])"/>.</summary>
    public static string T(string key, params object[] args) => Strings.Format(key, args);
}
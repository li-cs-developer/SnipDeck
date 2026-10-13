using System.ComponentModel;

namespace SnipDeck.Localization;

/// <summary>
/// Source object for XAML indexer bindings: <c>Source={x:Static LocSource.Instance}, Path=[Key]</c>.
/// Notifies WPF to re-read values when the language changes.
/// </summary>
public sealed class LocSource : INotifyPropertyChanged
{
    public static LocSource Instance { get; } = new();

    private LocSource()
    {
        Loc.LanguageChanged += (_, _) =>
        {
            // "Item[]" tells WPF the entire indexer's values are stale.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        };
    }

    public string this[string key] => Loc.T(key);

    public event PropertyChangedEventHandler? PropertyChanged;
}
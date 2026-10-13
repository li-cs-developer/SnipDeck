using SnipDeck.Core.Settings;
using SnipDeck.Localization;

namespace SnipDeck.App.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;

    public SettingsViewModel(ISettingsService settings)
    {
        _settings = settings;
    }

    public event EventHandler<bool>? FuzzySearchChanged;

    public int MaxHistoryItems
    {
        get => _settings.Current.MaxHistoryItems;
        set
        {
            var clamped = AppSettings.ClampToAllowed(value);
            if (_settings.Current.MaxHistoryItems == clamped) return;
            _settings.Current.MaxHistoryItems = clamped;
            OnPropertyChanged();
        }
    }

    public static int[] AvailableSizes => AppSettings.AllowedHistorySizes;

    public bool StartWithWindows
    {
        get => _settings.Current.StartWithWindows;
        set
        {
            if (_settings.Current.StartWithWindows == value) return;
            _settings.Current.StartWithWindows = value;
            OnPropertyChanged();
        }
    }

    public string Theme
    {
        get => _settings.Current.Theme;
        set
        {
            if (_settings.Current.Theme == value) return;
            _settings.Current.Theme = value;
            OnPropertyChanged();
        }
    }

    public bool ShowToasts
    {
        get => _settings.Current.ShowToasts;
        set
        {
            if (_settings.Current.ShowToasts == value) return;
            _settings.Current.ShowToasts = value;
            OnPropertyChanged();
        }
    }

    public bool UseFuzzySearch
    {
        get => _settings.Current.UseFuzzySearch;
        set
        {
            if (_settings.Current.UseFuzzySearch == value) return;
            _settings.Current.UseFuzzySearch = value;
            OnPropertyChanged();
            FuzzySearchChanged?.Invoke(this, value);
        }
    }

    public string Language
    {
        get => _settings.Current.Language;
        set
        {
            if (_settings.Current.Language == value) return;
            _settings.Current.Language = value;

            // Apply immediately — Loc.LanguageChanged fires, and all
            // {loc:Loc ...} bindings re-read.
            Loc.Apply(value);

            OnPropertyChanged();
        }
    }
}
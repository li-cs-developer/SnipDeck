namespace SnipDeck.Core.Settings;

public interface ISettingsService
{
    AppSettings Current { get; }
    void Load();
    void Save();
}
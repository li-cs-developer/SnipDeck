using System.Windows;
using Microsoft.Win32;

namespace SnipDeck.App.Themes;

public enum AppTheme { Light, Dark, System }

public static class ThemeManager
{
    private const string LightUri = "Themes/Light.xaml";
    private const string DarkUri = "Themes/Dark.xaml";

    public static AppTheme Current { get; private set; } = AppTheme.System;

    public static void Apply(AppTheme theme)
    {
        Current = theme;
        var effective = theme == AppTheme.System ? GetSystemTheme() : theme;
        var uri = effective == AppTheme.Dark ? DarkUri : LightUri;

        var dict = new ResourceDictionary
        {
            Source = new Uri(uri, UriKind.Relative)
        };

        var app = Application.Current;
        var merged = app.Resources.MergedDictionaries;

        int index = -1;
        for (int i = 0; i < merged.Count; i++)
        {
            var src = merged[i].Source?.OriginalString ?? string.Empty;
            if (src.EndsWith("Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                src.EndsWith("Dark.xaml", StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
            merged[index] = dict;
        else
            merged.Insert(0, dict);
    }

    public static AppTheme GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int i)
                return i == 0 ? AppTheme.Dark : AppTheme.Light;
        }
        catch
        {
            // Registry key missing or access denied — normal on some systems.
            // Fall through to the default.
        }
        return AppTheme.Light;
    }
}
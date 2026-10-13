using System.Windows;
using SnipDeck.App.Services;
using SnipDeck.App.Themes;
using SnipDeck.App.Tray;
using SnipDeck.App.ViewModels;
using SnipDeck.App.Views;
using SnipDeck.Core.Interop;
using SnipDeck.Core.Search;
using SnipDeck.Core.Services;
using SnipDeck.Core.Settings;
using SnipDeck.Localization;

namespace SnipDeck.App;

public partial class App : Application
{
    private const string MutexName = "Global\\SnipDeck.SingleInstance";

    private Mutex? _singleInstanceMutex;
    private ClipboardMonitor? _monitor;
    private TrayIconHost? _tray;
    private HotkeyManager? _hotkey;
    private JsonHistoryStore? _store;
    private JsonSettingsService? _settings;
    private MainWindow? _mainWindow;
    private MainViewModel? _mainViewModel;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            DispatcherUnhandledException += (_, args) =>
            {
                CrashLog.Write("App.DispatcherUnhandledException", args.Exception);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    CrashLog.Write("App.AppDomain.UnhandledException", ex);
                else
                    CrashLog.Write("App.AppDomain.UnhandledException",
                        args.ExceptionObject?.ToString() ?? "unknown error object");
            };

            _singleInstanceMutex = new Mutex(true, MutexName, out var createdNew);
            if (!createdNew)
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);

            _settings = new JsonSettingsService();
            _settings.Load();

            // Apply language before any UI is built.
            Loc.Apply(_settings.Current.Language);

            AutoStartHelper.SetEnabled(_settings.Current.StartWithWindows);
            ToastWindow.ToastsEnabled = _settings.Current.ShowToasts;

            var theme = _settings.Current.Theme switch
            {
                "Dark" => AppTheme.Dark,
                "Light" => AppTheme.Light,
                _ => AppTheme.System
            };
            ThemeManager.Apply(theme);

            _store = new JsonHistoryStore();
            _store.SetMaxItems(_settings.Current.MaxHistoryItems);
            _store.Load();

            _monitor = new ClipboardMonitor();
            var search = CreateSearchService(_settings.Current.UseFuzzySearch);
            _mainViewModel = new MainViewModel(_monitor, _store, search);

            _mainWindow = new MainWindow { DataContext = _mainViewModel };
            _mainWindow.SettingsRequested += (_, _) => OpenSettings();

            _tray = new TrayIconHost();
            _tray.AttachWindow(_mainWindow);
            _tray.SettingsRequested += (_, _) => OpenSettings();

            _mainViewModel.ItemCountChanged += (_, count) => UpdateTrayTooltip(count);
            UpdateTrayTooltip(_mainViewModel.TotalItemCount);

            // Rebuild tray menu labels when the language changes.
            Loc.LanguageChanged += (_, _) => _tray.RefreshMenuLabels();

            _hotkey = new HotkeyManager();
            if (!_hotkey.Register(_mainWindow, _settings.Current))
            {
                MessageBox.Show(
                    Loc.T("HotkeyRegisterFailedBody"),
                    Loc.T("HotkeyRegisterFailedTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            _hotkey.Pressed += (_, _) => _tray.ShowWindow();

            _monitor.Start();
        }
        catch (Exception ex)
        {
            CrashLog.Write("App.OnStartup", ex, "startup failed");
            MessageBox.Show(
                Loc.T("StartupFailedBodyFormat", ex.Message),
                Loc.T("StartupFailedTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static ISearchService CreateSearchService(bool useFuzzy) =>
        useFuzzy ? new FuzzySearchService() : new SimpleSearchService();

    private void UpdateTrayTooltip(int count)
    {
        if (_tray is null) return;
        var text = count == 1
            ? Loc.T("TrayTooltipSingular")
            : Loc.T("TrayTooltipFormat", count);
        _tray.SetTooltip(text);
    }

    private void OpenSettings()
    {
        if (_settings is null) return;

        try
        {
            var vm = new SettingsViewModel(_settings);

            vm.FuzzySearchChanged += (_, useFuzzy) =>
            {
                _mainViewModel?.SetSearchService(CreateSearchService(useFuzzy));
            };

            var win = new SettingsWindow { DataContext = vm };

            win.Closed += (_, _) =>
            {
                _settings.Save();
                _store?.SetMaxItems(_settings.Current.MaxHistoryItems);
                AutoStartHelper.SetEnabled(_settings.Current.StartWithWindows);
                ToastWindow.ToastsEnabled = _settings.Current.ShowToasts;
            };

            win.ShowDialog();
        }
        catch (Exception ex)
        {
            CrashLog.Write("App.OpenSettings", ex);
            MessageBox.Show(
                Loc.T("OpenSettingsFailedBodyFormat", ex.Message),
                Loc.T("OpenSettingsFailedTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        _monitor?.Dispose();
        _tray?.Dispose();
        _store?.Save();
        _settings?.Save();

        _mainWindow?.AllowRealClose();
        _mainWindow?.Close();

        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using SnipDeck.Localization;

namespace SnipDeck.App.Tray;

public sealed class TrayIconHost : IDisposable
{
    private readonly TaskbarIcon _icon;
    private readonly MenuItem _showItem;
    private readonly MenuItem _settingsItem;
    private readonly MenuItem _exitItem;
    private Window? _window;

    public event EventHandler? SettingsRequested;

    public TrayIconHost()
    {
        _icon = new TaskbarIcon
        {
            Icon = LoadAppIcon(),
            ToolTipText = Loc.T("AppTitle")
        };

        var menu = new ContextMenu();

        _showItem = new MenuItem { Header = Loc.T("TrayShow") };
        _showItem.Click += (_, _) => ShowWindow();

        _settingsItem = new MenuItem { Header = Loc.T("TraySettings") };
        _settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);

        _exitItem = new MenuItem { Header = Loc.T("TrayExit") };
        _exitItem.Click += (_, _) => Application.Current.Shutdown();

        menu.Items.Add(_showItem);
        menu.Items.Add(_settingsItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(_exitItem);

        _icon.ContextMenu = menu;
        _icon.TrayMouseDoubleClick += (_, _) => ShowWindow();
    }

    /// <summary>
    /// Load the tray icon from the embedded app.ico resource.
    /// Falls back to the Windows default if anything goes wrong.
    /// </summary>
    private static System.Drawing.Icon LoadAppIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/app.ico");
            var resource = Application.GetResourceStream(uri);
            if (resource is not null)
                return new System.Drawing.Icon(resource.Stream);
        }
        catch
        {
            // Fall through to the default.
        }
        return System.Drawing.SystemIcons.Application;
    }

    public void RefreshMenuLabels()
    {
        _showItem.Header = Loc.T("TrayShow");
        _settingsItem.Header = Loc.T("TraySettings");
        _exitItem.Header = Loc.T("TrayExit");
    }

    public void AttachWindow(Window window)
    {
        _window = window;
        _window.Hide();
    }

    public void SetTooltip(string text)
    {
        if (text.Length > 63) text = text[..60] + "...";
        _icon.ToolTipText = text;
    }

    public void ShowWindow()
    {
        if (_window is null) return;

        try
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        }
        catch (InvalidOperationException) { }
    }

    public void Dispose() => _icon.Dispose();
}
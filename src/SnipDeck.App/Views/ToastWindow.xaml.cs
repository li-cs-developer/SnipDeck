using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace SnipDeck.App.Views;

public partial class ToastWindow : Window
{
    private static ToastWindow? _current;
    private readonly DispatcherTimer _timer;

    public static bool ToastsEnabled { get; set; } = true;

    public ToastWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.6) };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };
    }

    public static void ShowToast(string message)
    {
        if (!ToastsEnabled) return;

        try
        {
            _current?.Close();
        }
        catch
        {
            // Previous toast was already closing — nothing to do.
        }
        _current = null;

        var toast = new ToastWindow();
        toast.MessageText.Text = message;

        toast.ContentRendered += (_, _) =>
        {
            var wa = SystemParameters.WorkArea;
            var w = toast.ActualWidth;
            var h = toast.ActualHeight;

            var left = wa.Right - w - 24;
            var top = wa.Bottom - h - 24;
            if (left < wa.Left) left = wa.Left + 12;
            if (top < wa.Top) top = wa.Top + 12;

            toast.Left = left;
            toast.Top = top;
        };

        toast.Closed += (_, _) =>
        {
            if (ReferenceEquals(_current, toast)) _current = null;
        };

        _current = toast;
        toast.Show();
        toast._timer.Start();
    }
}
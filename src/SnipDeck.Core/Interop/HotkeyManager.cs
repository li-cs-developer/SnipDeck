using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SnipDeck.Core.Settings;

namespace SnipDeck.Core.Interop;

/// <summary>
/// Registers a global hotkey against a WPF window and raises an event when pressed.
/// Registration happens on SourceInitialized so the HWND is stable and won't be
/// recreated by a later Show() call.
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    private const int HotkeyId = 0x5ECD;
    private HwndSource? _source;
    private IntPtr _handle;
    private bool _registered;
    private Window? _window;

    public event EventHandler? Pressed;

    public bool Register(Window window, AppSettings settings)
    {
        _window = window;

        // If the window already has an HWND (shown or EnsureHandle'd), register now.
        // Otherwise wait for SourceInitialized.
        var helper = new WindowInteropHelper(window);
        if (helper.Handle != IntPtr.Zero)
        {
            return RegisterCore(helper.Handle, settings);
        }

        window.SourceInitialized += (_, _) =>
        {
            var h = new WindowInteropHelper(window).Handle;
            RegisterCore(h, settings);
        };
        return true;
    }

    private bool RegisterCore(IntPtr handle, AppSettings settings)
    {
        _handle = handle;
        _source = HwndSource.FromHwnd(handle);
        if (_source is null)
        {
            System.Diagnostics.Debug.WriteLine($"[Hotkey] HwndSource is null for handle {handle}");
            return false;
        }

        _source.AddHook(WndProc);

        _registered = NativeMethods.RegisterHotKey(
            handle, HotkeyId, settings.HotkeyModifiers, settings.HotkeyKey);

        System.Diagnostics.Debug.WriteLine(
            $"[Hotkey] handle={handle}, registered={_registered}, lastErr={Marshal.GetLastWin32Error()}");

        return _registered;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            System.Diagnostics.Debug.WriteLine("[Hotkey] WM_HOTKEY received");
            Pressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_registered && _handle != IntPtr.Zero)
        {
            NativeMethods.UnregisterHotKey(_handle, HotkeyId);
            _registered = false;
        }
        _source?.RemoveHook(WndProc);
        _source = null;
    }
}
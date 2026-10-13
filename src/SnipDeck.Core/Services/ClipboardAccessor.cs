using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnipDeck.Core.Services;

/// <summary>
/// Wraps WPF Clipboard access with retry-on-COMException logic.
/// The clipboard is a shared OS resource; other apps may hold a lock
/// for a few milliseconds. We retry a handful of times with a short delay.
/// </summary>
public static class ClipboardAccessor
{
    private const int MaxRetries = 5;
    private const int RetryDelayMs = 50;

    // ---------- Readers ----------

    public static bool TryGetText(out string? text)
    {
        string? captured = null;
        var ok = TryRun(() =>
        {
            if (!Clipboard.ContainsText()) return false;
            captured = Clipboard.GetText();
            return true;
        });
        text = captured;
        return ok;
    }

    public static bool TryGetImage(out BitmapSource? image)
    {
        BitmapSource? captured = null;
        var ok = TryRun(() =>
        {
            if (!Clipboard.ContainsImage()) return false;
            captured = Clipboard.GetImage();
            return true;
        });
        image = captured;
        return ok;
    }

    public static bool TryGetFileDropList(out string[]? paths)
    {
        string[]? captured = null;
        var ok = TryRun(() =>
        {
            if (!Clipboard.ContainsFileDropList()) return false;
            var list = Clipboard.GetFileDropList();
            captured = list.Cast<string>().ToArray();
            return true;
        });
        paths = captured;
        return ok;
    }

    public static bool TryGetRichText(out string? text, out string? rtf, out string? html)
    {
        string? capturedText = null;
        string? capturedRtf = null;
        string? capturedHtml = null;

        var ok = TryRun(() =>
        {
            if (!Clipboard.ContainsText()) return false;
            capturedText = Clipboard.GetText();

            if (Clipboard.ContainsData(DataFormats.Rtf))
                capturedRtf = Clipboard.GetData(DataFormats.Rtf) as string;

            if (Clipboard.ContainsData(DataFormats.Html))
                capturedHtml = Clipboard.GetData(DataFormats.Html) as string;

            return true;
        });

        text = capturedText;
        rtf = capturedRtf;
        html = capturedHtml;
        return ok;
    }

    // ---------- Writers ----------

    public static bool TrySetText(string text) =>
        TryRun(() => { Clipboard.SetText(text); return true; });

    public static bool TrySetImage(BitmapSource image) =>
        TryRun(() => { Clipboard.SetImage(image); return true; });

    public static bool TrySetFileDropList(IEnumerable<string> paths) =>
        TryRun(() =>
        {
            var collection = new System.Collections.Specialized.StringCollection();
            foreach (var p in paths) collection.Add(p);
            Clipboard.SetFileDropList(collection);
            return true;
        });

    public static bool TrySetRichText(string text, string? rtf, string? html) =>
        TryRun(() =>
        {
            var data = new DataObject();
            data.SetData(DataFormats.UnicodeText, text);
            if (!string.IsNullOrEmpty(rtf))
                data.SetData(DataFormats.Rtf, rtf);
            if (!string.IsNullOrEmpty(html))
                data.SetData(DataFormats.Html, html);
            Clipboard.SetDataObject(data, true);
            return true;
        });

    // ---------- Retry helper ----------

    private static bool TryRun(Func<bool> action)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return action();
            }
            catch (COMException)
            {
                Thread.Sleep(RetryDelayMs);
            }
            catch (Exception)
            {
                return false;
            }
        }
        return false;
    }
}
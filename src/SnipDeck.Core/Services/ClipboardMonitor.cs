using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

public sealed class ClipboardMonitor : IClipboardMonitor, IDisposable
{
    private readonly DispatcherTimer _timer;
    private string? _lastTextHash;
    private int _lastImageHash;
    private string? _lastFilesHash;

    public event EventHandler<ClipboardItem>? ItemCaptured;

    public ClipboardMonitor()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTick;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void OnTick(object? sender, EventArgs e)
    {
        try
        {
            if (ClipboardExclusionChecker.ShouldExclude()) return;

            if (TryCaptureFiles()) return;
            if (TryCaptureRichText()) return;
            if (TryCaptureImage()) return;
            TryCaptureText();
        }
        catch (Exception ex)
        {
            // Clipboard reads are best-effort; transient failures are common
            // (another app holds the clipboard lock). Log and keep polling.
            CrashLog.Write("ClipboardMonitor.OnTick", ex);
        }
    }

    private bool TryCaptureFiles()
    {
        if (!ClipboardAccessor.TryGetFileDropList(out var paths) || paths is null || paths.Length == 0)
            return false;

        var hash = string.Join("|", paths);
        if (hash == _lastFilesHash) return true;

        _lastFilesHash = hash;
        _lastTextHash = null;
        _lastImageHash = 0;

        ItemCaptured?.Invoke(this, new FileClipboardItem(paths));
        return true;
    }

    private bool TryCaptureRichText()
    {
        bool hasRtf = false, hasHtml = false;
        try
        {
            hasRtf = Clipboard.ContainsData(DataFormats.Rtf);
            hasHtml = Clipboard.ContainsData(DataFormats.Html);
        }
        catch
        {
            // Probe failed — treat as "no rich text available" and move on.
            return false;
        }

        if (!hasRtf && !hasHtml) return false;

        if (!ClipboardAccessor.TryGetRichText(out var text, out var rtf, out var html) || text is null)
            return false;

        if (string.IsNullOrWhiteSpace(text)) return false;

        var hash = "RT:" + text.GetHashCode() + ":" + (rtf?.Length ?? 0) + ":" + (html?.Length ?? 0);
        if (hash == _lastTextHash) return true;

        _lastTextHash = hash;
        _lastImageHash = 0;
        _lastFilesHash = null;

        ItemCaptured?.Invoke(this, new RichTextClipboardItem(text, rtf, html));
        return true;
    }

    private bool TryCaptureImage()
    {
        if (!ClipboardAccessor.TryGetImage(out var image) || image is null)
            return false;

        var hash = ComputeImageHash(image);
        if (hash == _lastImageHash) return true;

        _lastImageHash = hash;
        _lastTextHash = null;
        _lastFilesHash = null;

        ItemCaptured?.Invoke(this, new ImageClipboardItem(image));
        return true;
    }

    private void TryCaptureText()
    {
        if (!ClipboardAccessor.TryGetText(out var text)) return;
        if (string.IsNullOrWhiteSpace(text)) return;

        var hash = text.GetHashCode().ToString();
        if (hash == _lastTextHash) return;

        _lastTextHash = hash;
        _lastImageHash = 0;
        _lastFilesHash = null;

        ItemCaptured?.Invoke(this, new TextClipboardItem(text));
    }

    private static int ComputeImageHash(BitmapSource image)
    {
        unchecked
        {
            int hash = image.PixelWidth * 397 ^ image.PixelHeight;
            hash = hash * 31 + image.Format.GetHashCode();
            return hash;
        }
    }

    public void Dispose() => _timer.Stop();
}
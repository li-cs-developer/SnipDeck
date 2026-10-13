using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

/// <summary>
/// Pure functions that push a ClipboardItem onto the OS clipboard.
/// No UI dependency — safe to unit test.
/// </summary>
public static class ClipboardActionService
{
    /// <summary>Copy a single item. Returns true on success.</summary>
    public static bool Copy(ClipboardItem item)
    {
        return item switch
        {
            TextClipboardItem t => ClipboardAccessor.TrySetText(t.Text),
            RichTextClipboardItem r => ClipboardAccessor.TrySetRichText(r.Text, r.Rtf, r.Html),
            ImageClipboardItem i => i.FullImage is { } full
                ? ClipboardAccessor.TrySetImage(full)
                : false,
            FileClipboardItem f => ClipboardAccessor.TrySetFileDropList(f.Paths),
            _ => false
        };
    }

    /// <summary>
    /// Copy multiple items joined by newlines. Only text and rich-text items
    /// are included. Returns (success, count) where count is how many were
    /// actually joined.
    /// </summary>
    public static (bool Success, int Count) CopyMany(IEnumerable<ClipboardItem> items)
    {
        var pieces = new List<string>();
        foreach (var item in items)
        {
            switch (item)
            {
                case TextClipboardItem t:
                    pieces.Add(t.Text);
                    break;
                case RichTextClipboardItem r:
                    pieces.Add(r.Text);
                    break;
            }
        }

        if (pieces.Count == 0) return (false, 0);

        var joined = string.Join(Environment.NewLine, pieces);
        return (ClipboardAccessor.TrySetText(joined), pieces.Count);
    }
}
using SnipDeck.Core.Models;
using SnipDeck.Localization;

namespace SnipDeck.App.Services;

public static class ToastDescription
{
    public static string For(ClipboardItem item) => item switch
    {
        TextClipboardItem t => Truncate(t.Text),
        RichTextClipboardItem r => Truncate(r.Text),
        FileClipboardItem f => f.DisplayText,
        ImageClipboardItem i => Loc.T("ImageDimensionsFormat", i.PixelWidth, i.PixelHeight),
        _ => "Item"
    };

    private static string Truncate(string s) =>
        s.Length > 40 ? s[..40] + "…" : s;
}
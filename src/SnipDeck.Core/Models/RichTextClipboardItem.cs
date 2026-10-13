namespace SnipDeck.Core.Models;

public sealed class RichTextClipboardItem : ClipboardItem
{
    /// <summary>Plain-text fallback.</summary>
    public string Text { get; }

    /// <summary>RTF source, if the clipboard provided it.</summary>
    public string? Rtf { get; }

    /// <summary>Raw HTML clipboard fragment, if provided.</summary>
    public string? Html { get; }

    public RichTextClipboardItem(string text, string? rtf = null, string? html = null)
    {
        Text = text;
        Rtf = rtf;
        Html = html;
    }

    public RichTextClipboardItem(Guid id, DateTime ts, string text, string? rtf, string? html)
        : base(id, ts)
    {
        Text = text;
        Rtf = rtf;
        Html = html;
    }

    public override ClipboardItemType Type => ClipboardItemType.RichText;
}
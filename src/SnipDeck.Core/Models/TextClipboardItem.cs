namespace SnipDeck.Core.Models;

public sealed class TextClipboardItem : ClipboardItem
{
    public string Text { get; }

    public TextClipboardItem(string text)
    {
        Text = text;
    }

    /// <summary>For deserialization from persisted history.</summary>
    public TextClipboardItem(Guid id, DateTime ts, string text)
        : base(id, ts)
    {
        Text = text;
    }

    public override ClipboardItemType Type => ClipboardItemType.Text;
}
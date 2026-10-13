namespace SnipDeck.Core.Models;

public abstract class ClipboardItem
{
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public abstract ClipboardItemType Type { get; }

    /// <summary>User-defined tags. Persisted.</summary>
    public List<string> Tags { get; } = new();

    /// <summary>Count of times this item was copied back to the clipboard.</summary>
    public int UseCount { get; set; }

    protected ClipboardItem()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.Now;
    }

    protected ClipboardItem(Guid id, DateTime timestamp)
    {
        Id = id;
        Timestamp = timestamp;
    }
}
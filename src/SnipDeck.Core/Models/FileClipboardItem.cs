namespace SnipDeck.Core.Models;

public sealed class FileClipboardItem : ClipboardItem
{
    public IReadOnlyList<string> Paths { get; }

    public FileClipboardItem(IEnumerable<string> paths)
    {
        Paths = paths.ToList();
    }

    // For deserialization.
    public FileClipboardItem(Guid id, DateTime timestamp, IEnumerable<string> paths)
        : base(id, timestamp)
    {
        Paths = paths.ToList();
    }

    public override ClipboardItemType Type => ClipboardItemType.File;

    public string DisplayText =>
        Paths.Count == 1
            ? System.IO.Path.GetFileName(Paths[0])
            : $"{Paths.Count} files";
}
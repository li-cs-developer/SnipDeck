using System.Collections.ObjectModel;
using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

public sealed class InMemoryHistoryStore : IHistoryStore
{
    private const int MaxItems = 50;
    private readonly ObservableCollection<ClipboardItem> _items = new();

    public IReadOnlyList<ClipboardItem> Items => _items;
    public ObservableCollection<ClipboardItem> ObservableItems => _items;

    public void Add(ClipboardItem item)
    {
        if (_items.Count > 0 && IsDuplicate(_items[0], item)) return;

        _items.Insert(0, item);
        while (_items.Count > MaxItems)
        {
            var last = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            DeleteFileIfImage(last);
        }
    }

    public void Remove(ClipboardItem item)
    {
        if (_items.Remove(item))
            DeleteFileIfImage(item);
    }

    public void RemoveMany(IEnumerable<ClipboardItem> items)
    {
        foreach (var item in items.ToList())
        {
            if (_items.Remove(item))
                DeleteFileIfImage(item);
        }
    }

    public void Clear()
    {
        foreach (var item in _items)
            DeleteFileIfImage(item);
        _items.Clear();
    }

    public void Load() { }
    public void Save() { }

    private static void DeleteFileIfImage(ClipboardItem item)
    {
        if (item is ImageClipboardItem img && !string.IsNullOrEmpty(img.FileName))
            ImageFileStore.Delete(img.FileName);
    }

    private static bool IsDuplicate(ClipboardItem a, ClipboardItem b) =>
        a switch
        {
            TextClipboardItem ta when b is TextClipboardItem tb => ta.Text == tb.Text,
            RichTextClipboardItem ra when b is RichTextClipboardItem rb => ra.Text == rb.Text,
            FileClipboardItem fa when b is FileClipboardItem fb => fa.Paths.SequenceEqual(fb.Paths),
            _ => false
        };
}
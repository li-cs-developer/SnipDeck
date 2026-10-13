using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

public interface IHistoryStore
{
    IReadOnlyList<ClipboardItem> Items { get; }
    void Add(ClipboardItem item);
    void Remove(ClipboardItem item);
    void RemoveMany(IEnumerable<ClipboardItem> items);
    void Clear();
    void Load();
    void Save();
}
using SnipDeck.Core.Models;

namespace SnipDeck.Core.Search;

public sealed class SearchFilter
{
    public string? Query { get; set; }
    public SortMode Sort { get; set; } = SortMode.NewestFirst;
}

public enum SortMode
{
    NewestFirst,
    OldestFirst,
    MostUsed
}

public interface ISearchService
{
    IEnumerable<ClipboardItem> Filter(IEnumerable<ClipboardItem> items, SearchFilter filter);
}
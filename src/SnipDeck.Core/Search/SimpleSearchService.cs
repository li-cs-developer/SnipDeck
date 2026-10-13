using SnipDeck.Core.Models;

namespace SnipDeck.Core.Search;

public sealed class SimpleSearchService : ISearchService
{
    public IEnumerable<ClipboardItem> Filter(IEnumerable<ClipboardItem> items, SearchFilter filter)
    {
        var query = filter.Query?.Trim();
        var result = items.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            result = result.Where(item => item switch
            {
                TextClipboardItem t =>
                    t.Text.Contains(query, StringComparison.OrdinalIgnoreCase),
                RichTextClipboardItem r =>
                    r.Text.Contains(query, StringComparison.OrdinalIgnoreCase),
                FileClipboardItem f =>
                    f.Paths.Any(p => p.Contains(query, StringComparison.OrdinalIgnoreCase)),
                _ => false
            });
        }

        result = filter.Sort switch
        {
            SortMode.OldestFirst => result.OrderBy(i => i.Timestamp),
            SortMode.MostUsed => result.OrderByDescending(i => i.UseCount)
                .ThenByDescending(i => i.Timestamp),
            _ => result.OrderByDescending(i => i.Timestamp)
        };

        return result;
    }
}
using SnipDeck.Core.Models;

namespace SnipDeck.Core.Search;

/// <summary>
/// Fuzzy subsequence search. Ranks matches by:
/// - consecutive-character runs
/// - word-boundary hits
/// - shorter target strings (higher density of query chars)
/// Falls back to plain Contains for very short queries (1-2 chars), where
/// subsequence matching is too permissive.
/// </summary>
public sealed class FuzzySearchService : ISearchService
{
    public IEnumerable<ClipboardItem> Filter(IEnumerable<ClipboardItem> items, SearchFilter filter)
    {
        var query = filter.Query?.Trim();
        var result = items.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            if (query.Length <= 2)
            {
                // Too short for subsequence — use substring match.
                result = result.Where(item => ExtractText(item)
                    .Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var scored = result
                    .Select(item => (item, score: ScoreItem(item, query)))
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Select(x => x.item);

                if (filter.Sort == SortMode.NewestFirst || filter.Sort == SortMode.OldestFirst)
                {
                    // Re-sort by timestamp afterwards, but keep score as a tiebreaker.
                    var ordered = filter.Sort == SortMode.OldestFirst
                        ? scored.OrderBy(GetTimestamp)
                        : scored.OrderByDescending(GetTimestamp);
                    result = ordered;
                }
                else
                {
                    // Most-used: score dominates, then use-count.
                    result = scored
                        .OrderByDescending(i => i.UseCount)
                        .ThenByDescending(GetTimestamp);
                }
            }
        }

        result = filter.Sort switch
        {
            SortMode.OldestFirst => result.OrderBy(GetTimestamp),
            SortMode.MostUsed => result.OrderByDescending(i => i.UseCount).ThenByDescending(GetTimestamp),
            _ => result.OrderByDescending(GetTimestamp)
        };

        return result;
    }

    private static DateTime GetTimestamp(ClipboardItem item) => item.Timestamp;

    private static IEnumerable<string> ExtractText(ClipboardItem item) => item switch
    {
        TextClipboardItem t => new[] { t.Text },
        RichTextClipboardItem r => new[] { r.Text },
        FileClipboardItem f => f.Paths,
        ImageClipboardItem => Array.Empty<string>(),
        _ => Array.Empty<string>()
    };

    private static int ScoreItem(ClipboardItem item, string query)
    {
        int best = 0;

        foreach (var text in ExtractText(item))
            best = Math.Max(best, ScoreText(text, query));

        // Bonus: matches a tag.
        foreach (var tag in item.Tags)
        {
            if (tag.Contains(query, StringComparison.OrdinalIgnoreCase))
                best += 50;
        }

        return best;
    }

    /// <summary>
    /// Score a fuzzy match. Returns 0 if no match. Higher is better.
    /// </summary>
    private static int ScoreText(string text, string query)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int score = 0;
        int textIdx = 0;
        int consecutive = 0;
        bool prevMatched = false;

        foreach (var qc in query)
        {
            bool found = false;
            while (textIdx < text.Length)
            {
                var tc = text[textIdx];
                if (char.ToLowerInvariant(tc) == char.ToLowerInvariant(qc))
                {
                    score += 10;

                    if (prevMatched) { consecutive++; score += consecutive * 5; }
                    else consecutive = 0;

                    // Word-boundary bonus: match at start, or after a space/punct.
                    if (textIdx == 0 || !char.IsLetterOrDigit(text[textIdx - 1]))
                        score += 8;

                    prevMatched = true;
                    textIdx++;
                    found = true;
                    break;
                }
                else
                {
                    prevMatched = false;
                }
                textIdx++;
            }

            if (!found) return 0; // query char not found in order
        }

        // Prefer shorter strings (matches feel more relevant).
        score += Math.Max(0, 40 - text.Length / 4);

        return score;
    }
}
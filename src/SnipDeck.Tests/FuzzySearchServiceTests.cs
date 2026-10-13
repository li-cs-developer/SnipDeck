using SnipDeck.Core.Models;
using SnipDeck.Core.Search;
using Xunit;

namespace SnipDeck.Tests;

public sealed class FuzzySearchServiceTests
{
    private static List<ClipboardItem> Items(params string[] texts) =>
        texts.Select(t => (ClipboardItem)new TextClipboardItem(t)).ToList();

    [Fact]
    public void EmptyQuery_ReturnsEverything()
    {
        var svc = new FuzzySearchService();
        var items = Items("alpha", "beta", "gamma");
        var result = svc.Filter(items, new SearchFilter { Query = null }).ToList();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Subsequence_Matches()
    {
        var svc = new FuzzySearchService();
        var items = Items("hello world", "goodbye");
        var result = svc.Filter(items, new SearchFilter { Query = "hlw" }).ToList();
        Assert.Single(result);
        Assert.Equal("hello world", ((TextClipboardItem)result[0]).Text);
    }

    [Fact]
    public void Subsequence_NoMatch_WhenOutOfOrder()
    {
        var svc = new FuzzySearchService();
        var items = Items("hello world");
        var result = svc.Filter(items, new SearchFilter { Query = "wlh" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void ShortQuery_UsesSubstring()
    {
        var svc = new FuzzySearchService();
        var items = Items("alpha", "beta");
        var result = svc.Filter(items, new SearchFilter { Query = "al" }).ToList();
        Assert.Single(result);
    }

    [Fact]
    public void CaseInsensitive()
    {
        var svc = new FuzzySearchService();
        var items = Items("Hello World");
        var result = svc.Filter(items, new SearchFilter { Query = "hElLo" }).ToList();
        Assert.Single(result);
    }

    [Fact]
    public void TagMatch_ReturnsItem()
    {
        var svc = new FuzzySearchService();
        var item = new TextClipboardItem("unrelated text");
        item.Tags.Add("important");

        var result = svc.Filter(new[] { (ClipboardItem)item },
                                new SearchFilter { Query = "important" }).ToList();
        Assert.Single(result);
    }

    [Fact]
    public void SortByOldestFirst_OrdersByTimestamp()
    {
        var svc = new FuzzySearchService();
        var a = new TextClipboardItem("aaa");
        var b = new TextClipboardItem("bbb");
        var result = svc.Filter(new[] { (ClipboardItem)a, (ClipboardItem)b },
                                new SearchFilter { Sort = SortMode.OldestFirst }).ToList();
        Assert.Equal(2, result.Count);
    }
}
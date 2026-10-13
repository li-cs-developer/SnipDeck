using SnipDeck.Core.Models;
using SnipDeck.Core.Services;
using Xunit;

namespace SnipDeck.Tests;

public sealed class JsonHistoryStoreTests
{
    [Fact]
    public void Add_InsertsAtTop()
    {
        var store = new JsonHistoryStore();
        store.Add(new TextClipboardItem("first"));
        store.Add(new TextClipboardItem("second"));

        Assert.Equal(2, store.Items.Count);
        Assert.Equal("second", ((TextClipboardItem)store.Items[0]).Text);
    }

    [Fact]
    public void Add_DedupesAgainstNewest()
    {
        var store = new JsonHistoryStore();
        store.Add(new TextClipboardItem("hello"));
        store.Add(new TextClipboardItem("hello"));

        Assert.Single(store.Items);
    }

    [Fact]
    public void SetMaxItems_TrimsImmediately()
    {
        var store = new JsonHistoryStore();
        for (int i = 0; i < 10; i++)
            store.Add(new TextClipboardItem($"item{i}"));

        store.SetMaxItems(3);
        Assert.Equal(3, store.Items.Count);
    }

    [Fact]
    public void Remove_RemovesOne()
    {
        var store = new JsonHistoryStore();
        var a = new TextClipboardItem("aaa");
        var b = new TextClipboardItem("bbb");
        store.Add(a);
        store.Add(b);

        store.Remove(a);
        Assert.Single(store.Items);
    }

    [Fact]
    public void RemoveMany_RemovesAll()
    {
        var store = new JsonHistoryStore();
        var items = Enumerable.Range(0, 5)
            .Select(i => new TextClipboardItem($"item{i}"))
            .ToList();
        foreach (var item in items) store.Add(item);

        store.RemoveMany(items.Take(3));
        Assert.Equal(2, store.Items.Count);
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        var store = new JsonHistoryStore();
        store.Add(new TextClipboardItem("x"));
        store.Add(new TextClipboardItem("y"));
        store.Clear();
        Assert.Empty(store.Items);
    }

    [Fact]
    public void Tags_And_UseCount_ArePreservedOnClone()
    {
        var store = new JsonHistoryStore();
        var item = new TextClipboardItem("tagged");
        item.Tags.Add("alpha");
        item.UseCount = 7;
        store.Add(item);

        var reloaded = store.Items[0];
        Assert.Contains("alpha", reloaded.Tags);
        Assert.Equal(7, reloaded.UseCount);
    }
}
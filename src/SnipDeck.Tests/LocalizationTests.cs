using SnipDeck.Localization;
using Xunit;

namespace SnipDeck.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void EnCa_HasHeading()
    {
        Loc.Apply("en-CA");
        Assert.Equal("Clipboard History", Loc.T("ClipboardHistoryHeading"));
    }

    [Fact]
    public void FrCa_HasHeading()
    {
        Loc.Apply("fr-CA");
        Assert.Equal("Historique du presse-papiers", Loc.T("ClipboardHistoryHeading"));
    }

    [Fact]
    public void FrCa_HasClearAll()
    {
        Loc.Apply("fr-CA");
        Assert.Equal("Tout effacer", Loc.T("ClearAllButton"));
    }

    [Fact]
    public void MissingKey_ReturnsKey()
    {
        Loc.Apply("en-CA");
        Assert.Equal("ThisKeyDoesNotExist", Loc.T("ThisKeyDoesNotExist"));
    }

    [Fact]
    public void Format_AppliesArguments_EnCa()
    {
        Loc.Apply("en-CA");
        Assert.Equal("3 items copied", Loc.T("ToastItemsCopiedFormat", 3));
    }

    [Fact]
    public void Format_AppliesArguments_FrCa()
    {
        Loc.Apply("fr-CA");
        Assert.Equal("3 éléments copiés", Loc.T("ToastItemsCopiedFormat", 3));
    }
}
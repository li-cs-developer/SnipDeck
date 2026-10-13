using SnipDeck.Core.Settings;
using Xunit;

namespace SnipDeck.Tests;

public sealed class AppSettingsTests
{
    [Theory]
    [InlineData(0, 50)]       // <= 0 -> default
    [InlineData(-1, 50)]
    [InlineData(1, 50)]       // closer to 50 than anything
    [InlineData(49, 50)]
    [InlineData(50, 50)]      // exact
    [InlineData(75, 50)]      // tie between 50 and 100 -> smaller wins
    [InlineData(76, 100)]     // closer to 100
    [InlineData(150, 100)]    // tie between 100 and 200 -> smaller wins
    [InlineData(300, 200)]    // tie between 200 and 400 -> smaller wins
    [InlineData(500, 400)]    // closer to 400
    [InlineData(999, 800)]    // closer to 800
    [InlineData(9999, 800)]   // way beyond max -> clamps to largest
    public void ClampToAllowed_ReturnsExpectedTier(int input, int expected)
    {
        Assert.Equal(expected, AppSettings.ClampToAllowed(input));
    }

    [Fact]
    public void MaxHistoryItems_ClampsOnSet()
    {
        var s = new AppSettings();
        s.MaxHistoryItems = 9999;
        Assert.Equal(800, s.MaxHistoryItems);
    }

    [Fact]
    public void Defaults_AreSensible()
    {
        var s = new AppSettings();
        Assert.Equal(50, s.MaxHistoryItems);
        Assert.Equal("System", s.Theme);
        Assert.Equal("en-CA", s.Language);
        Assert.True(s.ShowToasts);
        Assert.True(s.UseFuzzySearch);
        Assert.False(s.StartWithWindows);
    }
}
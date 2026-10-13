using SnipDeck.App.ViewModels;
using SnipDeck.Core.Search;
using Xunit;

namespace SnipDeck.Tests;

public sealed class FilterViewModelTests
{
    [Fact]
    public void EmptyQuery_ProducesNullQuery()
    {
        var vm = new FilterViewModel { Query = "" };
        Assert.Null(vm.ToSearchFilter().Query);

        vm.Query = "   ";
        Assert.Null(vm.ToSearchFilter().Query);
    }

    [Fact]
    public void NonEmptyQuery_Preserved()
    {
        var vm = new FilterViewModel { Query = "hello" };
        Assert.Equal("hello", vm.ToSearchFilter().Query);
    }

    [Theory]
    [InlineData("Newest first", SortMode.NewestFirst)]
    [InlineData("Oldest first", SortMode.OldestFirst)]
    [InlineData("Most used", SortMode.MostUsed)]
    public void SortLabel_MapsToEnum(string label, SortMode expected)
    {
        var vm = new FilterViewModel { Sort = label };
        Assert.Equal(expected, vm.ToSearchFilter().Sort);
    }

    [Fact]
    public void UnknownSortLabel_FallsBackToNewestFirst()
    {
        var vm = new FilterViewModel { Sort = "garbage" };
        Assert.Equal(SortMode.NewestFirst, vm.ToSearchFilter().Sort);
    }

    [Fact]
    public void ChangedEvent_FiresOnPropertyChange()
    {
        var vm = new FilterViewModel();
        int fired = 0;
        vm.Changed += (_, _) => fired++;

        vm.Query = "a";
        vm.Sort = "Oldest first";

        Assert.Equal(2, fired);
    }

    [Fact]
    public void ChangedEvent_DoesNotFireOnSameValue()
    {
        var vm = new FilterViewModel { Query = "abc" };
        int fired = 0;
        vm.Changed += (_, _) => fired++;

        vm.Query = "abc";

        Assert.Equal(0, fired);
    }
}
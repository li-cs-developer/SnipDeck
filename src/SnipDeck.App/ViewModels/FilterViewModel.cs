using SnipDeck.Core.Search;

namespace SnipDeck.App.ViewModels;

public sealed class FilterViewModel : ViewModelBase
{
    private string _query = string.Empty;
    private string _sort = "Newest first";

    public string Query
    {
        get => _query;
        set { if (SetProperty(ref _query, value)) Changed?.Invoke(this, EventArgs.Empty); }
    }

    public string Sort
    {
        get => _sort;
        set { if (SetProperty(ref _sort, value)) Changed?.Invoke(this, EventArgs.Empty); }
    }

    public event EventHandler? Changed;

    public SearchFilter ToSearchFilter() => new()
    {
        Query = string.IsNullOrWhiteSpace(Query) ? null : Query,
        Sort = Sort switch
        {
            "Oldest first" => SortMode.OldestFirst,
            "Most used" => SortMode.MostUsed,
            _ => SortMode.NewestFirst
        }
    };
}
using System.Collections.ObjectModel;
using System.Windows;
using SnipDeck.App.Services;
using SnipDeck.Core.Models;
using SnipDeck.Core.Search;
using SnipDeck.Core.Services;
using SnipDeck.Localization;

namespace SnipDeck.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IHistoryStore _store;
    private readonly IClipboardMonitor _monitor;
    private ISearchService _search;
    private ClipboardItemViewModel? _selectedItem;

    public ObservableCollection<ClipboardItemViewModel> Items { get; } = new();

    public FilterViewModel Filter { get; } = new();
    public PreviewViewModel Preview { get; } = new();

    public event EventHandler<string>? Copied;
    public event EventHandler<int>? ItemCountChanged;

    public int TotalItemCount => _store.Items.Count;

    public ClipboardItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
                Preview.Show(value);
        }
    }

    public MainViewModel(IClipboardMonitor monitor, IHistoryStore store, ISearchService search)
    {
        _monitor = monitor;
        _store = store;
        _search = search;

        Filter.Changed += (_, _) => RefreshFilter();

        _monitor.ItemCaptured += (_, item) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _store.Add(item);
                RefreshFilter();
            });
        };

        RefreshFilter();
    }

    public void SetSearchService(ISearchService search)
    {
        _search = search;
        RefreshFilter();
    }

    public bool CopyToClipboard(ClipboardItemViewModel vm)
    {
        if (!ClipboardActionService.Copy(vm.Model)) return false;

        vm.Model.UseCount++;
        RefreshFilter();
        Copied?.Invoke(this, ToastDescription.For(vm.Model));
        return true;
    }

    public bool CopyManyToClipboard(IEnumerable<ClipboardItemViewModel> vms)
    {
        var (ok, count) = ClipboardActionService.CopyMany(vms.Select(v => v.Model));
        if (!ok) return false;

        Copied?.Invoke(this, Loc.T("ToastItemsCopiedFormat", count));
        return true;
    }

    public void Delete(ClipboardItemViewModel vm)
    {
        _store.Remove(vm.Model);
        if (SelectedItem == vm) SelectedItem = null;
        RefreshFilter();
    }

    public void DeleteMany(IEnumerable<ClipboardItemViewModel> vms)
    {
        var models = vms.Select(v => v.Model).ToList();
        if (models.Count == 0) return;

        _store.RemoveMany(models);

        if (SelectedItem is not null && models.Contains(SelectedItem.Model))
            SelectedItem = null;

        RefreshFilter();
    }

    public void ClearAll()
    {
        _store.Clear();
        SelectedItem = null;
        RefreshFilter();
    }

    public void AddTag(string tag)
    {
        if (SelectedItem is null || string.IsNullOrWhiteSpace(tag)) return;
        var trimmed = tag.Trim().TrimStart('#');
        if (string.IsNullOrEmpty(trimmed)) return;
        if (SelectedItem.Model.Tags.Contains(trimmed, StringComparer.OrdinalIgnoreCase)) return;

        SelectedItem.Model.Tags.Add(trimmed);
        SelectedItem.RefreshTags();
        Preview.Show(SelectedItem);
        RefreshFilter();
    }

    public void RemoveTag(string tag)
    {
        if (SelectedItem is null) return;
        SelectedItem.Model.Tags.RemoveAll(t =>
            t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        SelectedItem.RefreshTags();
        Preview.Show(SelectedItem);
        RefreshFilter();
    }

    private void RefreshFilter()
    {
        var filter = Filter.ToSearchFilter();
        var filtered = _search.Filter(_store.Items, filter).ToList();

        var selectedModel = SelectedItem?.Model;

        Items.Clear();
        foreach (var item in filtered)
            Items.Add(new ClipboardItemViewModel(item));

        if (selectedModel is not null)
        {
            SelectedItem = Items.FirstOrDefault(v => v.Model.Id == selectedModel.Id);
        }

        ItemCountChanged?.Invoke(this, _store.Items.Count);
    }
}
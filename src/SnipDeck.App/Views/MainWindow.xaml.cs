using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using SnipDeck.App.ViewModels;
using SnipDeck.Core.Interop;
using SnipDeck.Core.Models;

namespace SnipDeck.App.Views;

public partial class MainWindow : Window
{
    private bool _reallyClosing;
    private ClipboardItemViewModel? _lastClickedItem;

    public event EventHandler? SettingsRequested;

    public MainWindow()
    {
        InitializeComponent();

        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();
        var exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
        exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
        exStyle &= ~NativeMethods.WS_EX_APPWINDOW;
        NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle);

        DataContextChanged += OnDataContextChanged;
    }

    public void AllowRealClose() => _reallyClosing = true;

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
            oldVm.Copied -= OnCopied;

        if (e.NewValue is MainViewModel newVm)
            newVm.Copied += OnCopied;
    }

    private void OnCopied(object? sender, string description)
    {
        // Every copy path in the app funnels through MainViewModel.Copied.
        // No suppression logic — if toasts are enabled, this shows.
        ToastWindow.ShowToast(description);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_reallyClosing)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }

    // ---- Selection tracking ----

    private void HistoryList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox list) return;

        var element = list.InputHitTest(e.GetPosition(list)) as DependencyObject;
        while (element is not null and not ListBoxItem)
            element = System.Windows.Media.VisualTreeHelper.GetParent(element);

        if (element is ListBoxItem lbi &&
            lbi.DataContext is ClipboardItemViewModel clicked)
        {
            _lastClickedItem = clicked;
        }
    }

    private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        if (HistoryList.SelectedItems.Count == 1 &&
            HistoryList.SelectedItems[0] is ClipboardItemViewModel single)
        {
            _lastClickedItem = single;
            vm.SelectedItem = single;
            return;
        }

        if (_lastClickedItem is not null &&
            HistoryList.SelectedItems.Contains(_lastClickedItem))
        {
            vm.SelectedItem = _lastClickedItem;
            return;
        }

        if (HistoryList.SelectedItems.Count > 0 &&
            HistoryList.SelectedItems[0] is ClipboardItemViewModel first)
        {
            _lastClickedItem = first;
            vm.SelectedItem = first;
        }
        else
        {
            _lastClickedItem = null;
            vm.SelectedItem = null;
        }
    }

    // ---- Mouse ----

    private void HistoryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel vm &&
            HistoryList.SelectedItem is ClipboardItemViewModel item)
        {
            vm.CopyToClipboard(item);
        }
    }

    // ---- Commands ----

    private void Delete_Executed(object sender, ExecutedRoutedEventArgs e) => DeleteSelectedInternal();

    private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = HistoryList.SelectedItems.Count > 0;

    private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        HistoryList.SelectAll();
    }

    // ---- Context menu ----

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm &&
            HistoryList.SelectedItem is ClipboardItemViewModel item)
        {
            vm.Delete(item);
        }
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e) => DeleteSelectedInternal();

    private void CopySelected_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        var selected = HistoryList.SelectedItems
            .OfType<ClipboardItemViewModel>()
            .ToList();

        if (selected.Count == 0) return;

        if (selected.Count == 1)
        {
            vm.CopyToClipboard(selected[0]);
            return;
        }

        vm.CopyManyToClipboard(selected);
    }

    // ---- Other ----

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ClearAll();
    }

    private void AddTag_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        vm.AddTag(TagInput.Text);
        TagInput.Clear();
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Filter.Query = string.Empty;
    }

    private void DeleteSelectedInternal()
    {
        if (DataContext is not MainViewModel vm) return;

        var selected = HistoryList.SelectedItems
            .OfType<ClipboardItemViewModel>()
            .ToList();

        if (selected.Count == 0) return;

        vm.DeleteMany(selected);
    }
}
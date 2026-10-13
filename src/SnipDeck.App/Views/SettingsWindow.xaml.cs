using System.Windows;
using SnipDeck.App.Themes;
using SnipDeck.App.ViewModels;

namespace SnipDeck.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        foreach (var size in SettingsViewModel.AvailableSizes)
            HistorySizeCombo.Items.Add(size);

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is SettingsViewModel vm)
        {
            // Initialize the static flag from the current setting.
            ToastWindow.ToastsEnabled = vm.ShowToasts;

            vm.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(SettingsViewModel.Theme):
                        var theme = vm.Theme switch
                        {
                            "Dark" => AppTheme.Dark,
                            "Light" => AppTheme.Light,
                            _ => AppTheme.System
                        };
                        ThemeManager.Apply(theme);
                        break;

                    case nameof(SettingsViewModel.ShowToasts):
                        ToastWindow.ToastsEnabled = vm.ShowToasts;
                        break;
                }
            };
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
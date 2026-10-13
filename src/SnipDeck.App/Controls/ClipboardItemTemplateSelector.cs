using System.Windows;
using System.Windows.Controls;
using SnipDeck.App.ViewModels;
using SnipDeck.Core.Models;

namespace SnipDeck.App.Controls;

public sealed class ClipboardItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? ImageTemplate { get; set; }
    public DataTemplate? FileTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not ClipboardItemViewModel vm) return base.SelectTemplate(item, container);

        return vm.Type switch
        {
            ClipboardItemType.Text => TextTemplate,
            ClipboardItemType.Image => ImageTemplate,
            ClipboardItemType.File => FileTemplate,
            _ => TextTemplate
        };
    }
}
using System.Windows.Media.Imaging;
using SnipDeck.Core.Models;

namespace SnipDeck.App.ViewModels;

public sealed class ClipboardItemViewModel : ViewModelBase
{
    public ClipboardItem Model { get; }

    public ClipboardItemViewModel(ClipboardItem model)
    {
        Model = model;
    }

    public ClipboardItemType Type => Model.Type;

    public string DisplayText => Model switch
    {
        TextClipboardItem t => t.Text,
        RichTextClipboardItem r => r.Text,
        FileClipboardItem f => f.DisplayText,
        ImageClipboardItem i => $"{i.PixelWidth}×{i.PixelHeight}",
        _ => Model.Type.ToString()
    };

    public string TimestampText => Model.Timestamp.ToString("HH:mm:ss");

    public BitmapSource? Thumbnail => Model is ImageClipboardItem i ? i.Thumbnail : null;

    public string? FilePath => Model is FileClipboardItem f && f.Paths.Count == 1
        ? f.Paths[0]
        : null;

    public string TagsText => Model.Tags.Count > 0
        ? string.Join("  ", Model.Tags.Select(t => "#" + t))
        : string.Empty;

    public bool HasTags => Model.Tags.Count > 0;

    public void RefreshTags()
    {
        OnPropertyChanged(nameof(TagsText));
        OnPropertyChanged(nameof(HasTags));
    }
}
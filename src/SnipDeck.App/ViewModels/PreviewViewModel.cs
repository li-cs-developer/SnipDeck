using System.Windows.Media.Imaging;
using SnipDeck.Core.Models;
using SnipDeck.Core.Services;

namespace SnipDeck.App.ViewModels;

public sealed class PreviewViewModel : ViewModelBase
{
    private ClipboardItemViewModel? _item;

    public ClipboardItemViewModel? Item
    {
        get => _item;
        private set
        {
            if (SetProperty(ref _item, value))
                NotifyDerived();
        }
    }

    public bool HasItem => _item is not null;
    public bool IsText => _item?.Model is TextClipboardItem or RichTextClipboardItem;
    public bool IsImage => _item?.Model is ImageClipboardItem;
    public bool IsFile => _item?.Model is FileClipboardItem;

    public string FullText => _item?.Model switch
    {
        TextClipboardItem t => t.Text,
        RichTextClipboardItem r => r.Text,
        _ => string.Empty
    };

    public BitmapSource? FullImage
    {
        get
        {
            if (_item?.Model is not ImageClipboardItem i) return null;
            var full = i.FullImage;
            if (full is null) return null;
            return ImageThumbnailer.Downscale(full, ImageThumbnailer.PreviewThumbnailMax);
        }
    }

    public string FullPath => _item?.Model is FileClipboardItem f
        ? string.Join(Environment.NewLine, f.Paths)
        : string.Empty;

    public string Header => _item is null
        ? string.Empty
        : $"{_item.Model.Type}  •  {_item.Model.Timestamp:yyyy-MM-dd HH:mm:ss}";

    public string TagLine => _item?.Model.Tags.Count > 0
        ? string.Join("  ", _item.Model.Tags.Select(t => "#" + t))
        : string.Empty;

    public void Show(ClipboardItemViewModel? vm) => Item = vm;

    public void Clear() => Item = null;

    private void NotifyDerived()
    {
        OnPropertyChanged(nameof(HasItem));
        OnPropertyChanged(nameof(IsText));
        OnPropertyChanged(nameof(IsImage));
        OnPropertyChanged(nameof(IsFile));
        OnPropertyChanged(nameof(FullText));
        OnPropertyChanged(nameof(FullImage));
        OnPropertyChanged(nameof(FullPath));
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(TagLine));
    }
}
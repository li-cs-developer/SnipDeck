using System.Windows.Media.Imaging;
using SnipDeck.Core.Services;

namespace SnipDeck.Core.Models;

public sealed class ImageClipboardItem : ClipboardItem
{
    private BitmapSource? _thumbnail;
    private BitmapSource? _fullImage;

    /// <summary>File name (not full path) of the persisted PNG, or null.</summary>
    public string? FileName { get; private set; }

    public int PixelWidth { get; private set; }
    public int PixelHeight { get; private set; }

    /// <summary>Full-resolution image. Lazily loaded from disk if not in memory.</summary>
    public BitmapSource? FullImage
    {
        get
        {
            if (_fullImage is not null) return _fullImage;
            if (FileName is null) return null;

            var loaded = ImageFileStore.Read(FileName);
            if (loaded is not null)
                _fullImage = loaded;
            return _fullImage;
        }
    }

    /// <summary>Small frozen thumbnail for list display.</summary>
    public BitmapSource? Thumbnail
    {
        get
        {
            if (_thumbnail is not null) return _thumbnail;

            var full = FullImage;
            if (full is null) return null;

            _thumbnail = ImageThumbnailer.Downscale(full, ImageThumbnailer.ListThumbnailMax);
            return _thumbnail;
        }
    }

    public ImageClipboardItem(BitmapSource source)
    {
        var frozen = ImageThumbnailer.Freeze(source);
        _fullImage = frozen;
        _thumbnail = ImageThumbnailer.Downscale(frozen, ImageThumbnailer.ListThumbnailMax);
        PixelWidth = frozen.PixelWidth;
        PixelHeight = frozen.PixelHeight;

        // Persist to disk immediately so we can reload next session.
        FileName = ImageFileStore.Write(frozen);
    }

    /// <summary>For deserialization from history.json.</summary>
    public ImageClipboardItem(Guid id, DateTime ts, string fileName, int pixelWidth, int pixelHeight)
        : base(id, ts)
    {
        FileName = fileName;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
    }

    public override ClipboardItemType Type => ClipboardItemType.Image;
}
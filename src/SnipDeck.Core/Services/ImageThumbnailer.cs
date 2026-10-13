using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnipDeck.Core.Services;

/// <summary>
/// Produces frozen, downscaled copies of images for display.
/// Freezing makes them immutable, cheap to render, and cross-thread-safe.
/// </summary>
public static class ImageThumbnailer
{
    public const int ListThumbnailMax = 200;
    public const int PreviewThumbnailMax = 800;

    /// <summary>Downscale to fit within <paramref name="maxDimension"/>, preserving aspect ratio.</summary>
    public static BitmapSource Downscale(BitmapSource source, int maxDimension)
    {
        if (source.PixelWidth <= maxDimension && source.PixelHeight <= maxDimension)
        {
            if (source.CanFreeze && !source.IsFrozen)
                source.Freeze();
            return source;
        }

        var scale = maxDimension / (double)Math.Max(source.PixelWidth, source.PixelHeight);
        var transform = new ScaleTransform(scale, scale);

        var thumbnail = new TransformedBitmap(source, transform);
        if (thumbnail.CanFreeze)
            thumbnail.Freeze();
        return thumbnail;
    }

    /// <summary>Freeze a bitmap in place if it isn't already.</summary>
    public static BitmapSource Freeze(BitmapSource source)
    {
        if (source.CanFreeze && !source.IsFrozen)
            source.Freeze();
        return source;
    }
}
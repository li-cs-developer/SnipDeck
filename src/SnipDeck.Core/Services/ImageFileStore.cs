using System.IO;
using System.Windows.Media.Imaging;

namespace SnipDeck.Core.Services;

/// <summary>
/// Manages PNG files that back image history items.
/// Files live in %APPDATA%\SnipDeck\images\ and are named {guid}.png.
/// </summary>
public static class ImageFileStore
{
    private static string _imageDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SnipDeck", "images");

    /// <summary>
    /// The directory where image files are stored. Defaults to the user's
    /// AppData folder. Tests can set this to a temp directory; production
    /// code never needs to.
    /// </summary>
    public static string Directory
    {
        get => _imageDir;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Image directory cannot be empty.", nameof(value));
            _imageDir = value;
        }
    }

    /// <summary>
    /// Restore the directory to its production default. Test cleanup helper.
    /// </summary>
    public static void ResetDirectory()
    {
        _imageDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SnipDeck", "images");
    }

    /// <summary>Write a bitmap to a new PNG file and return the file name (not full path).</summary>
    public static string? Write(BitmapSource source)
    {
        try
        {
            System.IO.Directory.CreateDirectory(_imageDir);

            var fileName = Guid.NewGuid().ToString("N") + ".png";
            var fullPath = Path.Combine(_imageDir, fileName);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(stream);

            return fileName;
        }
        catch (Exception ex)
        {
            CrashLog.Write("ImageFileStore.Write", ex,
                "failed to persist image — item will be session-only");
            return null;
        }
    }

    /// <summary>Load a previously written image. Returns null on failure.</summary>
    public static BitmapSource? Read(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(_imageDir, fileName);
            if (!File.Exists(fullPath)) return null;

            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = new PngBitmapDecoder(stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var frame = decoder.Frames[0];
            if (frame.CanFreeze) frame.Freeze();
            return frame;
        }
        catch (Exception ex)
        {
            CrashLog.Write("ImageFileStore.Read", ex,
                $"failed to load image '{fileName}'");
            return null;
        }
    }

    /// <summary>Delete a single file. Safe to call if the file is missing.</summary>
    public static void Delete(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            var fullPath = Path.Combine(_imageDir, fileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch
        {
            // Best-effort cleanup. Failure (e.g. file locked by another
            // process) is not worth reporting — the orphan scan will get
            // it on next launch.
        }
    }

    /// <summary>
    /// Delete any .png files that aren't referenced by the given set of keep names.
    /// </summary>
    public static void CleanOrphans(IEnumerable<string> keepFileNames)
    {
        try
        {
            if (!System.IO.Directory.Exists(_imageDir)) return;

            var keep = new HashSet<string>(keepFileNames, StringComparer.OrdinalIgnoreCase);

            foreach (var path in System.IO.Directory.EnumerateFiles(_imageDir, "*.png"))
            {
                var name = Path.GetFileName(path);
                if (keep.Contains(name)) continue;

                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // File may be locked by another process. Skip it.
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("ImageFileStore.CleanOrphans", ex,
                "orphan scan failed — old image files may accumulate");
        }
    }
}
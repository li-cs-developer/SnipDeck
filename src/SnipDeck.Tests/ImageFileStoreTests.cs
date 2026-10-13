using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SnipDeck.Core.Services;
using Xunit;

namespace SnipDeck.Tests;

/// <summary>
/// These tests redirect ImageFileStore at a temp directory so they don't
/// touch the developer's real %APPDATA%\SnipDeck\images.
/// ImageFileStore.Directory is a static, so tests run serially within this
/// class via the fixture (xUnit runs tests in the same class sequentially
/// by default, so that's fine).
/// </summary>
public sealed class ImageFileStoreTests : IDisposable
{
    private readonly string _tempDir;

    public ImageFileStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(),
            "SnipDeckTests", Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(_tempDir);
        ImageFileStore.Directory = _tempDir;
    }

    public void Dispose()
    {
        ImageFileStore.ResetDirectory();
        try { System.IO.Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private static BitmapSource MakeBitmap(int w, int h)
    {
        // 32bpp BGRA, simple solid-color bitmap.
        var stride = w * 4;
        var pixels = new byte[stride * h];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 0x80;     // B
            pixels[i + 1] = 0x40; // G
            pixels[i + 2] = 0x20; // R
            pixels[i + 3] = 0xFF; // A
        }
        var bmp = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        bmp.Freeze();
        return bmp;
    }

    [Fact]
    public void Write_CreatesFile()
    {
        var bmp = MakeBitmap(4, 4);
        var name = ImageFileStore.Write(bmp);

        Assert.NotNull(name);
        Assert.True(File.Exists(Path.Combine(_tempDir, name!)));
    }

    [Fact]
    public void Write_ReturnsUniqueNames()
    {
        var bmp = MakeBitmap(4, 4);
        var a = ImageFileStore.Write(bmp);
        var b = ImageFileStore.Write(bmp);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Read_RoundtripsBitmap()
    {
        var bmp = MakeBitmap(8, 6);
        var name = ImageFileStore.Write(bmp)!;

        var loaded = ImageFileStore.Read(name);

        Assert.NotNull(loaded);
        Assert.Equal(8, loaded!.PixelWidth);
        Assert.Equal(6, loaded.PixelHeight);
    }

    [Fact]
    public void Read_MissingFile_ReturnsNull()
    {
        var result = ImageFileStore.Read("does-not-exist.png");
        Assert.Null(result);
    }

    [Fact]
    public void Read_EmptyFileName_ReturnsNull()
    {
        var result = ImageFileStore.Read(string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void Delete_RemovesFile()
    {
        var bmp = MakeBitmap(4, 4);
        var name = ImageFileStore.Write(bmp)!;
        var full = Path.Combine(_tempDir, name);

        ImageFileStore.Delete(name);

        Assert.False(File.Exists(full));
    }

    [Fact]
    public void Delete_MissingFile_DoesNotThrow()
    {
        ImageFileStore.Delete("nonexistent.png");
        ImageFileStore.Delete(string.Empty);
        ImageFileStore.Delete(null!);
    }

    [Fact]
    public void CleanOrphans_RemovesUnreferencedFiles()
    {
        var keepName = ImageFileStore.Write(MakeBitmap(4, 4))!;
        var orphanA = ImageFileStore.Write(MakeBitmap(4, 4))!;
        var orphanB = ImageFileStore.Write(MakeBitmap(4, 4))!;

        ImageFileStore.CleanOrphans(new[] { keepName });

        Assert.True(File.Exists(Path.Combine(_tempDir, keepName)));
        Assert.False(File.Exists(Path.Combine(_tempDir, orphanA)));
        Assert.False(File.Exists(Path.Combine(_tempDir, orphanB)));
    }

    [Fact]
    public void CleanOrphans_EmptyKeepSet_RemovesAll()
    {
        ImageFileStore.Write(MakeBitmap(4, 4));
        ImageFileStore.Write(MakeBitmap(4, 4));

        ImageFileStore.CleanOrphans(Array.Empty<string>());

        var remaining = System.IO.Directory.GetFiles(_tempDir, "*.png");
        Assert.Empty(remaining);
    }

    [Fact]
    public void CleanOrphans_MissingDirectory_DoesNotThrow()
    {
        ImageFileStore.Directory = Path.Combine(_tempDir, "nested", "does-not-exist");
        ImageFileStore.CleanOrphans(new[] { "anything.png" });
    }

    [Fact]
    public void Directory_RejectsEmptyValue()
    {
        Assert.Throws<ArgumentException>(() => ImageFileStore.Directory = "");
        Assert.Throws<ArgumentException>(() => ImageFileStore.Directory = "   ");
    }

    [Fact]
    public void Directory_RoundTripsValue()
    {
        var original = ImageFileStore.Directory;
        var custom = Path.Combine(_tempDir, "custom");

        ImageFileStore.Directory = custom;
        Assert.Equal(custom, ImageFileStore.Directory);

        ImageFileStore.Directory = original;
        Assert.Equal(original, ImageFileStore.Directory);
    }
}
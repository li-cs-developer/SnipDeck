using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

public sealed class JsonHistoryStore : IHistoryStore
{
    private static readonly string DataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnipDeck");

    private static readonly string DataPath = Path.Combine(DataDir, "history.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ObservableCollection<ClipboardItem> _items = new();
    private int _maxItems = 50;

    public IReadOnlyList<ClipboardItem> Items => _items;
    public ObservableCollection<ClipboardItem> ObservableItems => _items;

    public void SetMaxItems(int max)
    {
        if (max < 1) max = 50;
        _maxItems = max;
        TrimToLimit();
    }

    public void Add(ClipboardItem item)
    {
        if (_items.Count > 0 && IsDuplicate(_items[0], item)) return;

        _items.Insert(0, item);
        TrimToLimit();
    }

    public void Remove(ClipboardItem item)
    {
        if (_items.Remove(item))
            DeleteFileIfImage(item);
    }

    public void RemoveMany(IEnumerable<ClipboardItem> items)
    {
        foreach (var item in items.ToList())
        {
            if (_items.Remove(item))
                DeleteFileIfImage(item);
        }
    }

    public void Clear()
    {
        foreach (var item in _items)
            DeleteFileIfImage(item);
        _items.Clear();
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(DataPath)) return;

            var json = File.ReadAllText(DataPath);
            var dtos = JsonSerializer.Deserialize<List<HistoryDto>>(json, JsonOptions);
            if (dtos is null) return;

            foreach (var dto in dtos)
            {
                var item = dto.ToModel();
                if (item is not null) _items.Add(item);
            }

            TrimToLimit();

            var referenced = _items
                .OfType<ImageClipboardItem>()
                .Select(i => i.FileName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => n!);
            ImageFileStore.CleanOrphans(referenced);
        }
        catch (Exception ex)
        {
            CrashLog.Write("JsonHistoryStore.Load", ex,
                "history load failed — starting with empty history");
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(DataDir);

            var dtos = _items.Select(HistoryDto.FromModel).ToList();
            var json = JsonSerializer.Serialize(dtos, JsonOptions);
            File.WriteAllText(DataPath, json);
        }
        catch (Exception ex)
        {
            CrashLog.Write("JsonHistoryStore.Save", ex, "history save failed");
        }
    }

    private void TrimToLimit()
    {
        while (_items.Count > _maxItems)
        {
            var last = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            DeleteFileIfImage(last);
        }
    }

    private static void DeleteFileIfImage(ClipboardItem item)
    {
        if (item is ImageClipboardItem img && !string.IsNullOrEmpty(img.FileName))
            ImageFileStore.Delete(img.FileName);
    }

    private static bool IsDuplicate(ClipboardItem a, ClipboardItem b) =>
        a switch
        {
            TextClipboardItem ta when b is TextClipboardItem tb => ta.Text == tb.Text,
            RichTextClipboardItem ra when b is RichTextClipboardItem rb => ra.Text == rb.Text,
            FileClipboardItem fa when b is FileClipboardItem fb => fa.Paths.SequenceEqual(fb.Paths),
            _ => false
        };

    // ---- DTOs ----

    private sealed class HistoryDto
    {
        public string Type { get; set; } = "Text";
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Text { get; set; }
        public string[]? Paths { get; set; }
        public string? Rtf { get; set; }
        public string? Html { get; set; }
        public List<string>? Tags { get; set; }
        public int UseCount { get; set; }

        public string? FileName { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        public static HistoryDto FromModel(ClipboardItem item)
        {
            var dto = new HistoryDto
            {
                Id = item.Id,
                Timestamp = item.Timestamp,
                Tags = item.Tags.Count > 0 ? new List<string>(item.Tags) : null,
                UseCount = item.UseCount
            };

            switch (item)
            {
                case TextClipboardItem t:
                    dto.Type = "Text";
                    dto.Text = t.Text;
                    break;
                case RichTextClipboardItem rt:
                    dto.Type = "RichText";
                    dto.Text = rt.Text;
                    dto.Rtf = rt.Rtf;
                    dto.Html = rt.Html;
                    break;
                case FileClipboardItem f:
                    dto.Type = "File";
                    dto.Paths = f.Paths.ToArray();
                    break;
                case ImageClipboardItem img:
                    dto.Type = "Image";
                    dto.FileName = img.FileName;
                    dto.PixelWidth = img.PixelWidth;
                    dto.PixelHeight = img.PixelHeight;
                    break;
            }

            return dto;
        }

        public ClipboardItem? ToModel()
        {
            ClipboardItem? item = Type switch
            {
                "Text" when Text is not null =>
                    new TextClipboardItem(Id, Timestamp, Text),
                "RichText" when Text is not null =>
                    new RichTextClipboardItem(Id, Timestamp, Text, Rtf, Html),
                "File" when Paths is { Length: > 0 } =>
                    new FileClipboardItem(Id, Timestamp, Paths),
                "Image" when FileName is not null =>
                    new ImageClipboardItem(Id, Timestamp, FileName, PixelWidth, PixelHeight),
                _ => null
            };

            if (item is not null)
            {
                if (Tags is not null) item.Tags.AddRange(Tags);
                item.UseCount = UseCount;
            }

            return item;
        }
    }
}
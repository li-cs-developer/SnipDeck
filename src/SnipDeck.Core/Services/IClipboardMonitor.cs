using SnipDeck.Core.Models;

namespace SnipDeck.Core.Services;

public interface IClipboardMonitor
{
    event EventHandler<ClipboardItem>? ItemCaptured;
    void Start();
    void Stop();
}
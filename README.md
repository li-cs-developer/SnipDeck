# 📋 SnipDeck

> 💡 **A Windows clipboard manager that stores, searches, and restores your copy history — offline, private, and fast.**

SnipDeck is a **tray application** that keeps a searchable history of everything you copy: plain text, rich text, images, and file drop lists. It runs quietly in the background, respects password-manager exclusions, and **never connects to the internet**. Built on **.NET 10 + WPF**, it demonstrates MVVM architecture, four-project layering, live theming, bilingual localization, and a full xUnit test suite.

---

## ✨ Features

### 📋 Capture
- 📝 **Plain text** — every copy captured
- 🎨 **Rich text** — RTF and HTML formats preserved alongside plain text
- 🖼️ **Images** — screenshots and copied pictures
- 📄 **File drop lists** — paths of files copied from File Explorer
- 🛡️ **Password-manager exclusions** — respects `ExcludeClipboardContentFromMonitorProcessing` and `CanIncludeInClipboardHistory`
- 🚫 **Duplicate suppression** — consecutive identical copies don't create duplicate history entries

### 🔍 Find & Restore
- 🔎 **Fuzzy search** — subsequence matching (`hlw` matches `hello world`), or plain substring — your choice
- ↕️ **Sort** — newest first, oldest first, or most-used first
- 🏷️ **Tags** — user-defined labels on any item, persisted
- 👁️ **Preview pane** — full text, full image, or full file path
- ☑️ **Multi-select** — Ctrl-click, Shift-click, Ctrl+A
- 📤 **Multi-copy** — newline-joined text from multiple selected items

### 🖥️ Desktop Integration
- ⌨️ **Global hotkey** — `Ctrl+Shift+V` pops the picker from any application
- 🖥️ **System tray icon** — Show / Settings / Exit menu
- 🔢 **Live tray tooltip** — shows current item count
- 🔔 **Optional toasts** — small confirmation on copy (theme-aware)
- 🚀 **Start with Windows** — optional
- 🔒 **Single-instance enforcement** — one copy runs at a time
- 🙈 **Hidden from Alt-Tab** — clean, tool-window behavior

### 🎨 Polish
- 🌗 **Themes** — Light, Dark, or follow-system; live-switchable
- 🌐 **Localization** — Canadian English + Canadian French, live-switchable
- ♿ **Accessibility** — Narrator labels on every interactive element
- 💾 **Persistent history** — survives restart
- 📊 **Configurable history size** — 50 / 100 / 200 / 400 / 800 items (tiered)
- 🧹 **Automatic image cleanup** — PNGs deleted when their item is evicted

---

## 🔗 Project Links

| | |
|---|---|
| 📖 **Confluence** | [SnipDeck Documentation](https://li-cs-developer.atlassian.net/wiki/x/AwAY) |
| 🎫 **Jira** | [SNIP Project Board](https://li-cs-developer.atlassian.net/jira/software/projects/SNIP/list) |
| 🐙 **Repository** | [github.com/li-cs-developer/SnipDeck](https://github.com/li-cs-developer/SnipDeck) |
| 📦 **Releases** | [github.com/li-cs-developer/SnipDeck/releases](https://github.com/li-cs-developer/SnipDeck/releases) |


---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **UI** | WPF, MVVM, XAML |
| **Runtime** | .NET 10 (`net10.0-windows`) |
| **Tray icon** | `Hardcodet.NotifyIcon.Wpf` 2.0.1 |
| **Persistence** | JSON + PNG files in `%APPDATA%\SnipDeck\` |
| **Localization** | `.resx` resources + custom `{loc:Loc}` markup extension |
| **Win32 interop** | P/Invoke for `RegisterHotKey` and window styles |
| **Testing** | xUnit 2.9.2, 54 tests |
| **Build** | MSBuild, `dotnet` CLI |

---

## 🏗️ Architecture

SnipDeck is a **four-project solution** with a strict one-way dependency graph.

```
SnipDeck.slnx
└── src/
    ├── 🎨 SnipDeck.Core/            ← domain logic, no UI dependency
    ├── 🌐 SnipDeck.Localization/    ← resx dictionaries, no Core dependency
    ├── 🖥️ SnipDeck.App/              ← WPF executable, references both
    └── 🧪 SnipDeck.Tests/            ← xUnit, references all three
```

### Dependency graph

```
        SnipDeck.App ────► SnipDeck.Localization
              │                             │
              └──────► SnipDeck.Core ◄──────┘
                             ▲
                             │
                      SnipDeck.Tests
```

The only way to make this cleaner would be to remove the arrow from `App → Localization`, which would require inverting the localization dependency. For a small app, the current shape is the right trade-off.

### Data flow — clipboard capture

```
        ┌──────────────────────────┐
        │   Windows OS Clipboard   │
        └────────────┬─────────────┘
                     │ 500 ms DispatcherTimer tick
                     ▼
        ┌──────────────────────────┐         ┌──────────────────────────────┐
        │  📋 ClipboardMonitor     │────────►│  🛡️ ClipboardExclusionChecker│
        └────────────┬─────────────┘         └──────────────────────────────┘
                     │ reads via
                     ▼
        ┌──────────────────────────┐
        │  🔄 ClipboardAccessor    │  (retry-on-COMException)
        └────────────┬─────────────┘
                     │ produces
                     ▼
        ┌────────────────────────────────────────────────────────┐
        │  📝 Text  /  🎨 RichText  /  🖼️ Image  /  📄 File       │
        └────────────┬───────────────────────────────────────────┘
                     │ ItemCaptured event
                     ▼
        ┌──────────────────────────┐         ┌──────────────────────────┐
        │  🧠 MainViewModel        │────────►│  💾 JsonHistoryStore     │
        │     (SnipDeck.App)       │  add    │   └─► 🖼️ ImageFileStore  │
        └────────────┬─────────────┘         └──────────────────────────┘
                     │ RefreshFilter
                     ▼
        ┌──────────────────────────┐
        │  📚 ObservableCollection │
        └────────────┬─────────────┘
                     │ DataBinding
                     ▼
        ┌──────────────────────────┐
        │  🖼️ WPF ListBox          │
        └──────────────────────────┘
```

### Data flow — clipboard restore

```
        Double-click a list item
                     │
                     ▼
        🧠 MainViewModel.CopyToClipboard(vm)
                     │
                     ▼
        📤 ClipboardActionService.Copy(model)
                     │
                     ▼
        🔄 ClipboardAccessor.TrySet*
                     │ on success
                     ▼
        🎉 Copied event → 💬 ToastWindow.ShowToast()
```

---

## 📁 Project Structure

```
SnipDeck/
├── assets/                               ← source assets (logo, etc.)
├── build/                                ← build & packaging scripts
│   ├── version.props                     ← central version number
│   ├── publish.ps1                       ← publish script
│   ├── installer.iss                     ← Inno Setup installer script
│   └── output/                           ← git-ignored publish output
├── docs/                                 ← long-form documentation
├── src/
│   ├── SnipDeck.Core/                    ← class library, no UI
│   │   ├── Models/                       ← ClipboardItem hierarchy
│   │   ├── Services/                     ← Monitor, Accessor, Stores, CrashLog
│   │   ├── Search/                       ← Fuzzy + Simple search implementations
│   │   ├── Settings/                     ← AppSettings + JsonSettingsService
│   │   └── Interop/                      ← NativeMethods, HotkeyManager
│   │
│   ├── SnipDeck.Localization/            ← class library, resources
│   │   ├── Strings.resx                  ← en-CA strings
│   │   ├── Strings.fr-CA.resx            ← fr-CA strings
│   │   ├── Loc.cs                        ← Loc.T(key) API + LanguageChanged event
│   │   ├── LocSource.cs                  ← INotifyPropertyChanged indexer for XAML
│   │   └── LocExtension.cs               ← {loc:Loc Key} markup extension
│   │
│   ├── SnipDeck.App/                     ← WPF executable
│   │   ├── Views/                        ← MainWindow, SettingsWindow, ToastWindow
│   │   ├── ViewModels/                   ← MVVM view models
│   │   ├── Controls/                     ← Template selectors
│   │   ├── Converters/                   ← Value converters
│   │   ├── Themes/                       ← Shared / Light / Dark + ThemeManager
│   │   ├── Tray/                         ← TrayIconHost
│   │   ├── Services/                     ← AutoStartHelper, ToastDescription
│   │   ├── Resources/                    ← app.ico
│   │   └── app.manifest                  ← DPI awareness, no UAC
│   │
│   └── SnipDeck.Tests/                   ← xUnit test project
│       ├── AppSettingsTests.cs
│       ├── FilterViewModelTests.cs
│       ├── FuzzySearchServiceTests.cs
│       ├── ImageFileStoreTests.cs
│       ├── JsonHistoryStoreTests.cs
│       └── LocalizationTests.cs
│
├── .editorconfig
├── .gitattributes
├── .gitignore
├── LICENSE
├── PRIVACY.md
├── README.md
└── SnipDeck.slnx
```

---

## 🚦 Getting Started

### 📋 Prerequisites

- 🪟 **Windows 10 build 19041+** or **Windows 11**
- 🟣 **.NET 10 SDK** — [download](https://dotnet.microsoft.com/download)
- 🧑💻 An editor: **Rider** (recommended), **Visual Studio 2022**, or **VS Code** + C# extension

### 🐳 Install from Release (recommended for users)

1. Download `SnipDeck-portable.zip` from the [Releases page](https://github.com/li-cs-developer/SnipDeck/releases).
2. Extract anywhere.
3. Run `SnipDeck.exe`.
4. Look for the tray icon (check the `^` overflow if you don't see it).

### 💻 Build from Source

```powershell
git clone https://github.com/li-cs-developer/SnipDeck
cd SnipDeck
dotnet build -c Release
dotnet run --project src/SnipDeck.App
```

The app launches **hidden in the system tray**.

### 🔨 Building the Installer

```powershell
# Publish both framework-dependent and portable builds
pwsh build\publish.ps1

# Build the installer (requires Inno Setup 6)
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" build\installer.iss
```

---

## 🎮 Usage

| Action | How |
|---|---|
| **Show the picker** | Double-click the tray icon, or press `Ctrl+Shift+V` |
| **Copy an item back** | Double-click it, or right-click → **Copy to clipboard** |
| **Search** | Type in the search box (200 ms debounce) |
| **Delete one item** | Select, press `Delete` |
| **Delete many** | Ctrl-click to select, press `Delete` |
| **Copy many** | Ctrl-click to select, right-click → **Copy to clipboard** |
| **Add a tag** | Select an item, type in the tag box, click **Add Tag** |
| **Open settings** | ⚙ **Settings** button, or tray → **Settings…** |
| **Exit** | Tray → **Exit** |

### Keyboard shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+Shift+V` | Global — show the picker from any application |
| `Ctrl+A` | Select all items in the list |
| `Delete` | Delete selected items |
| `Esc` | Close Settings window |

---

## ⚙️ Settings Reference

| Setting | Default | Description |
|---|---|---|
| **Max history items** | 50 | 50 / 100 / 200 / 400 / 800 — tiered |
| **Theme** | System | Light / Dark / System |
| **Show toast notifications** | On | Toast popup on copy |
| **Use fuzzy search** | On | Subsequence matching vs. substring |
| **Start with Windows** | Off | Registry `Run` key |
| **Language** | English (Canada) | en-CA / fr-CA |

---

## 🧠 Engineering Practices

### 🏛️ Clean Architecture

The solution enforces a strict one-way dependency graph. `SnipDeck.Core` has **no dependency on WPF presentation types** — only framework primitives like `BitmapSource`, `DispatcherTimer`, and `Clipboard`. This means the clipboard-monitoring and history-storage logic could be reused in a console app or a different UI framework without rewriting anything.

| Principle | Where it shows up |
|---|---|
| **Single Responsibility** | Monitor reads clipboard; store owns history; view model orchestrates; view renders. Each layer knows only its job. |
| **Dependency Inversion** | `IHistoryStore`, `IClipboardMonitor`, `ISearchService`, `ISettingsService` — all four abstract a concrete concern, enabling swapping and testing. |
| **Interface Segregation** | Small, focused interfaces. `IClipboardMonitor` has three members. `ISettingsService` has three. |
| **Encapsulation** | `ClipboardItem` hierarchy validates in constructors. `ImageClipboardItem.FullImage` lazy-loads from disk. |
| **Composition over inheritance** | The `App` class wires up services directly — no DI container, no reflection, no ceremony. |
| **Testability** | 54 xUnit tests. The test suite can run headless (no WPF dispatcher needed for the pure-logic tests). |
| **Fail-safe by default** | Startup wrapped in try/catch with visible errors. Every catch block either logs to `CrashLog` or is silent with a comment. |

### 🧪 Test Coverage

54 tests across six classes:

| Test class | Coverage |
|---|---|
| `AppSettingsTests` | Clamping behavior, defaults |
| `FilterViewModelTests` | Sort-label mapping, change-event firing |
| `FuzzySearchServiceTests` | Subsequence matching, tag matching, tie-breaking |
| `ImageFileStoreTests` | File lifecycle with directory injection |
| `JsonHistoryStoreTests` | Add / dedup / trim / remove / clear |
| `LocalizationTests` | en-CA and fr-CA resolution, missing-key fallback, format arguments |

```powershell
dotnet test
```

---

## 🏛️ Architecture Decisions

### Why WPF over WinForms?
Data binding, MVVM tooling, and modern theming are substantially easier in WPF. The tray icon library is the only `System.Drawing` dependency, and it's an implementation detail.

### Why clipboard polling instead of `WM_CLIPBOARDUPDATE`?
The Win32 `AddClipboardFormatListener` API is more efficient, but WPF's clipboard API surface is easier to use and a 500 ms poll is invisible in practice. Cost: one `Clipboard.ContainsText()` every 500 ms — negligible.

### Why rich text before images in the capture order?
Most modern apps put an HTML fragment on the clipboard alongside plain text when you copy formatted content. If we checked images first, we'd miss the rich text. Precedence: **Files → Rich Text → Image → Text**.

### Why store images as PNG files, not base64 in JSON?
A single 1920×1080 screenshot is ~8 MB as raw pixels. Base64 in JSON would be ~11 MB, and `history.json` would balloon to hundreds of megabytes within a session. PNG files on disk with a filename in JSON avoids this entirely.

### Why does SnipDeck not paste directly into the focused application?
`SendInput`-based direct paste was designed and then removed. It required careful window-focus management to avoid pasting into the wrong window. Users paste manually with `Ctrl+V` after selecting an item — more predictable, matches Windows clipboard history behavior.

### Why no DI container?
The app has fewer than 20 services, all wired in `App.OnStartup`. A container would add ceremony (registration, lifetimes, disposal) without benefit at this scale.

### Why tiered history sizes (50/100/200/400/800)?
A free-form number input invites users to type `10000` — which would ship a broken experience (memory bloat, slow startup). A dropdown of five reasonable choices gives choice within a sane constraint. Values are clamped to the nearest tier on load, so a hand-edited `settings.json` can't break the app.

---

## 🔒 Privacy

SnipDeck stores everything locally in `%APPDATA%\SnipDeck\`. **No network access. No telemetry. No accounts.** See [PRIVACY.md](PRIVACY.md) for details.

### What lives where

| File | Contents |
|---|---|
| `%APPDATA%\SnipDeck\history.json` | Clipboard history (text, rich text, file paths) |
| `%APPDATA%\SnipDeck\settings.json` | User preferences |
| `%APPDATA%\SnipDeck\images\*.png` | Captured images |
| `%APPDATA%\SnipDeck\crash.log` | Error log (only on unexpected failures) |

### Reset everything

```powershell
Remove-Item -Recurse "$env:APPDATA\SnipDeck"
```

---

## 🤖 AI-Assisted Development

AI tools (Copilot, Gemini, ChatGPT) were used throughout development to accelerate mechanical work and support learning. Effective uses included:

- **Architecture scaffolding** — laying out the four-project split, wiring MVVM layers
- **Test-first drafts** — producing initial failing tests for `FuzzySearchService`, `ImageFileStore`, and others
- **Refactoring support** — extracting `ClipboardActionService` from `MainViewModel`, eliminating empty catches, adding centralized `CrashLog`
- **Localization pass** — translating en-CA strings to fr-CA and structuring the `Loc` markup extension
- **Documentation** — drafting the README, PRIVACY, and Confluence documentation
- **Debugging** — diagnosing a recurring HWND-recreation bug in the global hotkey registration, and a theme-resource lookup failure that crashed the Settings window

---

## 🖥️ Compatibility

| | |
|---|---|
| ✅ **Supported** | Windows 10 (build 19041+) and Windows 11, x64 |
| ⚠️ **Not tested** | Windows 10 ARM64, Windows Server |
| 🟣 **Runtime** | .NET 10 Desktop Runtime (bundled in portable build) |

---

## 🐛 Troubleshooting

### Tray icon doesn't appear
Check the `^` overflow near the system clock — Windows 11 hides overflow icons by default. Drag SnipDeck onto the visible taskbar if you want it always shown.

### Global hotkey `Ctrl+Shift+V` doesn't work
Another app owns the shortcut. Common culprits: Ditto, CopyQ, Windows Terminal. Change the binding in `%APPDATA%\SnipDeck\settings.json`:

```json
{
  "HotkeyModifiers": 6,
  "HotkeyKey": 86
}
```

`6` is `Ctrl+Shift`, `86` is `V`. Change the key to something unused, e.g. `0x7A` for `F11`.

### Images don't appear after restart
Check `%APPDATA%\SnipDeck\images\` — PNGs should be there. If they're missing, the image was captured before image persistence was implemented, or the file was manually deleted. Orphan cleanup on startup is idempotent — files that don't match a live history item are deleted.

### Settings crashes on open
Check `%APPDATA%\SnipDeck\crash.log`. This was a known issue with theme resource lookup; if it recurs, paste the log into the Jira tracker.

### Clear All doesn't persist
Exit via **tray → Exit**, not Task Manager. History and settings are written on graceful shutdown. Killing the process loses uncommitted changes.

### How do I reset everything?
```powershell
Remove-Item -Recurse "$env:APPDATA\SnipDeck"
```

---

## 📄 License

MIT — see [LICENSE](LICENSE).

# 🔒 Privacy Policy — SnipDeck

> 💡 **Short version:** SnipDeck stores everything locally. It never connects to the internet.

## What SnipDeck collects

SnipDeck captures items you copy to the Windows clipboard:

- 📝 **Text** — plain text
- 🎨 **Rich text** — RTF and HTML fragments alongside plain text
- 🖼️ **Images** — screenshots and copied pictures
- 📄 **File drop lists** — paths of files copied from File Explorer

## Where that data is stored

Everything lives in `%APPDATA%\SnipDeck\` on your own machine:

| File | Contents |
|---|---|
| `history.json` | Your clipboard history (text, rich text, file paths) |
| `settings.json` | Your preferences (theme, language, history cap, etc.) |
| `images\*.png` | PNG files for captured images |
| `crash.log` | Local error log, only written when something unexpected happens |

## What SnipDeck does NOT do

- ❌ **No network access.** SnipDeck makes zero outbound connections. There is no server, no cloud, no sync.
- ❌ **No telemetry.** No usage statistics, no analytics, no crash reporting to any remote service.
- ❌ **No accounts.** You don't sign in. There's nothing to sign into.
- ❌ **No sharing.** Your history is never sent to anyone, anywhere.
- ❌ **No encryption.** History is stored as plain text and PNGs. If someone has access to your Windows user profile, they can read your history.

## Password managers and sensitive content

SnipDeck respects the `ExcludeClipboardContentFromMonitorProcessing` clipboard flag set by password managers (KeePass, 1Password, Bitwarden, and others). Content marked with that flag is not captured.

Additionally, any content marked with `CanIncludeInClipboardHistory = 0` is skipped. This is the Windows 10+ privacy convention for clipboard history.

If you copy a password from a tool that does **not** set these flags, SnipDeck will capture it. SnipDeck does not attempt to detect passwords heuristically.

## Clearing your data

To delete all SnipDeck data:

```powershell
Remove-Item -Recurse "$env:APPDATA\SnipDeck"

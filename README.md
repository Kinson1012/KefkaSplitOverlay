# Kefka Split Overlay

A personal, independent adaptation of
[AweiYourdog's FF14-Kefka-P4-for-PC](https://github.com/AweiYourdog/FF14-Kefka-P4-for-PC).
Windows / C# / .NET 8 / WPF / GPL-3.0.

## What changed

- **Trigger window:** the original wording/icons in a compact grouped layout:
  GC1 + Water/Fire 1 above GC2 + Water/Fire 2, with Thunder/Ice below and
  the two bomb buttons in a narrow column on the right.
- **Results window:** only the eight calculated results, in resolution order.
- **Compact mode:** press `▣` in either title bar to hide both windows and show
  one **48 × 48 DIP** K icon. One click restores both windows at their saved
  locations and sizes. Drag the icon to reposition it; dragging does not restore.
- Drag either title bar independently; resize either window from its edges.
  Content scales uniformly while preserving the grouped arrangement.
- Settings save language, opacity, layout lock, positions, sizes and whether
  the app was compact. Current encounter selections deliberately do not persist
  between app launches. They do persist through hide/show and language changes.
- Use **Recover positions** in Settings or the icon's right-click menu after a
  monitor/layout change. Restored windows are also clamped to a connected monitor.
- Three upstream icons are packaged as PNG, removing the WebP codec dependency.

The default trigger size is **384 × 262 DIP**, following the latest layout sketch. Windows display scaling can change its physical
pixel dimensions. The result window starts at 232 × 416 DIP.

This build uses the original **Flow** mechanic rules. The upstream Pikmin and
Beta interfaces are not included. It takes manual input only and does not read
game memory, logs or network traffic.

## Hotbar mapping

The inputs use the original Flow labels and icons, grouped as in the latest sketch. Blue circles mean true;
red question marks mean false. The original icon artwork is unchanged.

| Section, in order | Inputs, left to right |
| --- | --- |
| GC1 | Blue circle / red ?; early / late; bomb icon |
| Water/Fire 1 | Blue circle / red ?; water icon / fire icon |
| GC2 | Blue circle / red ?; bomb icon |
| Water/Fire 2 | Blue circle / red ? |
| Thunder line | Blue circle / red ? |
| Ice cone | Blue circle / red ? |

Layout: first row GC1 / Water-Fire 1 / GC1 bomb; second row GC2 / Water-Fire 2 / GC2 bomb; bottom row Thunder / Ice.

Group headings and early/late wording come directly from the original translation
properties. Hover a button for its meaning. Selection is marked by a yellow
border and blue background. Result headings and number/fire/water labels also
come from the original Flow interface.

The updated appearance defaults to opaque black backgrounds, white labels and
bright yellow result text. Existing preferences are upgraded to an opaque
background once; positions and language are retained. The input window is resized to the new
proportions once on upgrade; later custom sizes are retained. You can still
adjust opacity in Settings afterwards. Result rows remain opaque for readability.

Letters are source identifiers, not keyboard shortcuts. C/D also determine the
complementary GC2 timing, exactly as in upstream. O/Q start selected; the final
result therefore initially says **Avoid Both**. This is an upstream default,
not automatic encounter detection.

## Run / build

Use FFXIV in borderless-windowed mode. Windows gameplay testing is still needed
for this adaptation, particularly focus, drag/resize and multi-monitor DPI.

Install the **.NET 8 SDK** on Windows, open PowerShell in this folder, and run:

```powershell
dotnet run --project UMADOverlay/UMADOverlay.csproj
```

Create the standalone Windows x64 build (the resulting app does not require a
separate .NET runtime installation):

```powershell
dotnet publish UMADOverlay/UMADOverlay.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o dist/app
```

Run `dist/app/KefkaSplitOverlay.exe`. Publish trimming is deliberately disabled
for WPF. Before distributing an executable, include `LICENSE`, `NOTICE.md` and
the complete corresponding source. The included GitHub workflow packages these
together automatically.

Preferences are stored in `%LOCALAPPDATA%/KefkaSplitOverlay/settings.json`.
Corrupt/unreadable settings fall back to defaults; write errors are reported in
Settings. Exit the app before manually editing or deleting the settings file.

## Use your own GitHub repository without forking

Create an **empty repository** in your own account, then use its URL below.
Do not create a README or licence on GitHub first, since both are already here.

```powershell
git init
git add .
git commit -m "Create personal Kefka split overlay from attributed GPL upstream"
git branch -M main
git remote add origin https://github.com/YOUR_ACCOUNT/YOUR_REPOSITORY.git
git push -u origin main
```

This is a standalone source copy, with no upstream `.git` history or remote.
Keep `LICENSE` and attribution when redistributing. A GitHub fork relationship
is not required by GPL-3.0; its source and licence obligations still apply.

After pushing, open **Actions → Build Windows app**. A successful run provides
the **KefkaSplitOverlay-Windows-x64** artifact, containing the app, notices and
matching source. This workflow uploads a build artifact; it does not publish a
public Release automatically.

## Tests and verification status

```powershell
dotnet run --project tests/CoreTests -c Release
dotnet run --project tests/OverlaySmokeTests -c Release
```

- CoreTests checks 8,748 Flow input combinations, selection toggles,
  translations, and preferences round-trip/corruption handling.
- OverlaySmokeTests checks WPF resource loading, both windows opening,
  hide/restore, shared selections, language updates, tile uniqueness and locks.
- The creation environment could not access a .NET SDK or Windows desktop.
  These C# tests are supplied **but have not been executed there**. The Windows
  workflow must pass before treating this build as verified.
- Static checks confirm XML validity, packaged image resources, all input
  mappings, original Flow order and icons, and byte-for-byte preservation of the upstream
  Flow rule source. They do not independently validate FFXIV encounter strategy.

### Manual Windows acceptance checks

1. Click inputs in the trigger window and confirm immediate results in the other.
2. Move and resize the windows independently; confirm the Flow layout scales without reordering.
3. Click `▣`: only one K icon remains. Click K: both windows return with selections.
4. Drag K: it moves without expanding. Test its right-click recovery and exit.
5. Select each language; check results, labels, and tooltips remain usable.
6. Restart: geometry/preferences return, encounter selections start fresh.
7. Test clicks during gameplay: normal overlay buttons should not take keyboard
   focus. The separate Settings window intentionally accepts focus.
8. Test Windows display scaling and disconnecting a secondary monitor.

See `NOTICE.md` for provenance and modification details.

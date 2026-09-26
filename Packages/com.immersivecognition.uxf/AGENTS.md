# Agent guide for the UXF package

Read `README.md` and the installed package version before editing. This file is shipped as package-local context so an agent working from a consumer project can find the same boundaries without relying on the development repository history.

## Architecture

- `Session` is the Unity-facing coordinator. It owns blocks, lifecycle events and configured data handlers.
- `Block` owns trials. `Trial` owns per-trial settings/results and controls tracker start/stop.
- `Settings` cascades through `ISettingsContainer`; preserve this behavior when adding configuration.
- `DataHandler` is the extension point for persistence/transports. `FileSaver` is asynchronous and must be cleaned up; HTTP/WebGL handlers have platform constraints.
- Editor tooling lives under `Scripts/Etc/Editor`; UI and platform integrations are optional. Keep `UnityEditor` out of runtime code.
- `UXF.Runtime` is the player-facing assembly; `UXF.Editor`, `UXF.Form.Editor` and `UXF.HideIf.Editor` contain editor-only drawers. Tests are isolated in `UXF.Tests.Editor` and `UXF.Tests.PlayMode`; do not add NUnit or UnityEditor references to runtime code.
- The bundled Windows file-dialog integration uses Win32 P/Invoke and does not impose the legacy `.NET Framework` / `NET_Unity_4_8` API profile. The package target is `.NET Standard 2.1`; validate any project-specific dependency that requires Framework.
- The package manifest declares UGUI 2.0.0 because the optional UI components use `UnityEngine.UI`; core Session/Block/Trial code can still be used without adding a `UIController`.
- Full domain and scene reloads remain the validated play-session configuration. Runtime reset hooks clear known static Session, SessionLogger and random-generator state even when domain reload is disabled, but fast Enter Play Mode remains unvalidated while scene reload is disabled because component and integration state can persist.
- Android FileSaver builds use `PersistentDataPath` (app-private storage). The package no longer enables or requires Unity's legacy External (SDCard) permission; a different storage provider must document and own any platform permission it needs.

## Safe changes

- Prefer existing public extension points, serialized references and focused interfaces.
- Search all usages before changing a public member or serialized field. Preserve field names and UnityEvent methods; use migration attributes for serialized renames.
- Do not edit a consumer's PackageCache copy. Change the source package or use a local package override.
- Do not add a singleton, service locator or dependency solely to avoid a narrow test seam.
- Never put Unity API calls on the `FileSaver` worker thread.
- Treat output paths, CSV/JSON formats and bytes as public data contracts. Add regression tests before changing them.

## Validation

At minimum, test the affected EditMode behavior and compile a clean consumer using the installed package. For lifecycle, UI, browser, native-plugin or build changes, add the corresponding PlayMode/platform test or document the manual evidence. Run the package validation workflow when preparing a release. Do not claim a platform is supported from editor compilation alone. Import the `UXF Examples` Package Manager sample before using sample scenes; copy its `StreamingAssets` fixtures into the consumer project's root `Assets/StreamingAssets` when a scene needs them. Sample scripts must not depend on hardcoded package-source paths; users import them through Package Manager's **Samples** tab.

## Documentation

Update the package README, relevant wiki page and changelog when public behavior changes. Keep examples complete and version-matched. Record assumptions and unsupported platform combinations in the user-facing docs.

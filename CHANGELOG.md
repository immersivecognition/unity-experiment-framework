# Changelog

## 3.0.0

- Major release for the Unity 6 / UPM package line. Review the package migration and platform validation notes before upgrading an existing project.

- Moved the authoritative package source to `Packages/com.immersivecognition.uxf` so the development project consumes the same embedded package source that CI archives and consumers install.
- Added package development workflow documentation, updated the Git URL path and changed the clean consumer fixture to use the embedded package path.
- CI now validates imported sample metadata, builds a checksummed UPM archive, and publishes only version-matched tags; licensed Unity result artifacts are required to contain discovered tests.
- Fixed Unity 6 import diagnostics caused by a malformed editor-script meta file, duplicate Test Runner assembly references and orphaned empty-folder metas left by the Samples move.
- Replaced deprecated WebGL `Pointer_stringify` calls with `UTF8ToString`.
- Corrected Unity 6 web-request version guards and the setup wizard's desktop API-profile target.
- Removed obsolete pre-Unity-6 fallback branches from runtime web requests, EventSystem/session-debugger lookup, WebGL template checks and Web AWS target gating.
- Replaced the Windows file-dialog adapter's legacy System.Windows.Forms/Ookii.Dialogs assemblies with Win32 common-dialog P/Invoke; the package now targets .NET Standard 2.1 without those bundled DLLs.
- The Windows-only `SFB.WindowWrapper` helper is no longer part of the adapter; callers should use `StandaloneFileBrowser` instead of depending on dialog implementation details.
- Setup wizard now accepts Unity's normal reload behavior and flags only explicitly enabled fast Enter Play Mode settings.
- Added subsystem-registration resets for known runtime static state; fast Enter Play Mode still requires scene-reload acceptance tests before it can be advertised as supported.
- Build validation now checks UI-free Sessions and does not show modal dialogs in batch mode.
- Local-file handler platform checks now run for UI-free and manual-start Sessions instead of only Sessions discovered through a UIController.
- Android FileSaver build validation now requires app-private `PersistentDataPath` without enabling the legacy External (SDCard) permission.
- Tracker table serialization now uses a worker queue and `Session.End()` waits for queued tracker writes; `ResultsDictionary` writes are synchronized.
- Removed the obsolete EasyEvent reflection patch and global reorderable inspector; UXF inspectors now use Unity's native serialized property/list/event controls.
- Added runtime/editor/test assembly definitions, Package Manager `UXF Examples` samples, and package-local agent recipes.
- Package samples now carry fresh-GUID Unity 6 WebGL templates; imported consumers are instructed to copy the selected template into `Assets/WebGLTemplates` because Unity only discovers custom templates there.
- Package metadata now declares UGUI, includes sample StreamingAssets fixtures and provides a clean UPM consumer fixture for CI.
- Package metadata now declares the UnityWebRequest module required by HTTPPost and UI web-file loading, so minimal consumers compile without development-project module assumptions.
- The clean Package Manager consumer now has explicit EditMode/PlayMode test assemblies and smoke tests for runtime session use and sample metadata.
- Consumer sample smoke tests now verify the packaged Unity 6 WebGL template as well as the sample README.
- The `[UXF_Rig]` Session component is enabled by default; Session and EventSystem fallback lifecycle code no longer runs in edit mode. UI/data-handler validation is now side-effect-free so inspecting the rig does not dirty scenes.
- CSV values now use standard quoting for separators, quotes and embedded line breaks; malformed quoted records fail explicitly.
- FileSaver rejects rooted or nested path components in experiment and participant identifiers.
- FileSaver now applies EN-US output formatting explicitly without changing the caller thread's culture.
- FileSaver relative paths now use the platform path API and emit stable forward-slash output on Windows, macOS and Linux; FileSaver tests use isolated temporary roots.
- FileSaver cleanup is now safe before setup and when repeated.
- FileSaver queue item counting is synchronized while the worker is active.
- Unity 6 EditMode coverage now runs cleanly under the native test runner; culture, CSV, JSON and EditMode lifecycle/static-state regressions are covered by the updated tests.
- Session and SessionLogger static state now resets across play sessions; logger callbacks detach on destruction.
- Trial worker queues now drain and restart after `Session.End()` and across domain-reload-disabled play starts; `FormElementEntry.element` is read-only to package consumers.
- Hardened the sample upload server against path traversal and source-controlled credentials.
- Added repository and package-local agent guides, plus Unity 6 package installation documentation.

All notable UXF package changes are documented here. Release entries should describe public API, serialization, data-format, platform and installation changes.

## 2.4.5

- Development baseline migrated to Unity 6000.3.25f1.
- Added the initial Unity Package Manager manifest and package-local agent/contributor guidance.
- Compatibility cleanup is in progress; consult the Unity 6 upgrade backlog before relying on platform support claims.

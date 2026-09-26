# UXF package changelog

## 3.0.0

- Major release for the Unity 6 / UPM package line. Review the package migration and platform validation notes before upgrading an existing project.
- Added Input System dependency and runtime input bridge for UXF pointer/key reads; the rig switches its own and scene-owned legacy UI input modules at runtime so Input System-only consumers can use the UI.
- Fixed the Windows folder browser's initial-path callback passing a string as a shell item ID, which could corrupt native state and cause a stack overflow.

- Moved the authoritative package source to `Packages/com.immersivecognition.uxf` so the development project consumes it as an embedded Package Manager package; updated the Git installation path, clean consumer fixture, exporter and CI archive workflow.
- The package workflow now emits a SHA-256 checksum and verifies package/tag/changelog consistency before a tagged release.
- Added a documented package development workflow covering embedded source, imported samples, clean consumers, archive validation and legacy Assets migration.
- Fixed Unity 6 import diagnostics caused by a malformed editor-script meta file, duplicate Test Runner assembly references and orphaned empty-folder metas left by the Samples move.
- Unity 6 compatibility cleanup is in progress; consult the repository upgrade backlog before relying on platform support claims.
- Windows native file dialogs now use Win32 common-dialog P/Invoke; the legacy System.Windows.Forms/Ookii.Dialogs assemblies were removed and the package targets .NET Standard 2.1.
- The Windows-only `SFB.WindowWrapper` implementation detail was removed; use `StandaloneFileBrowser` as the public entry point.
- Deprecated WebGL string bridges were updated and package-local agent guidance was added.
- Obsolete pre-Unity-6 runtime/editor fallback branches were removed from web requests, object lookup, WebGL template checks and Web AWS target gating.
- The setup wizard now distinguishes normal reload behavior from explicitly enabled fast Enter Play Mode settings.
- Local-file handler platform checks now apply to UI-free and manual-start Sessions as well as UI-driven Sessions.
- Android FileSaver build validation now requires app-private `PersistentDataPath` without enabling the legacy External (SDCard) permission.
- Batchmode build failures now raise `BuildFailedException` without opening an editor modal dialog.
- Tracker table serialization now uses a worker queue and `Session.End()` waits for queued tracker writes; `ResultsDictionary` writes are synchronized.
- Removed the obsolete EasyEvent reflection patch and global reorderable inspector; UXF inspectors now use Unity's native serialized property/list/event controls.
- Added runtime/editor/test assembly definitions, Package Manager `UXF Examples` samples, and package-local agent recipes.
- Package samples now carry fresh-GUID Unity 6 WebGL templates; imported consumers are instructed to copy the selected template into `Assets/WebGLTemplates` because Unity only discovers custom templates there.
- Known runtime static Session, SessionLogger and random-generator state now resets at subsystem registration; fast Enter Play Mode remains unvalidated when scene reload is disabled.
- CSV output now preserves separators/quotes/newlines using standard quoting; FileSaver rejects unsafe session path components, keeps caller culture unchanged and tolerates repeated cleanup.
- FileSaver relative paths now use the platform path API and emit stable forward-slash output on Windows, macOS and Linux; FileSaver tests use isolated temporary roots.
- FileSaver queue accounting is synchronized while the worker is active.
- Unity 6 EditMode coverage now runs cleanly under the native test runner; culture, CSV, JSON and EditMode lifecycle/static-state regressions are covered by the updated tests.
- Session and SessionLogger static state now resets across play sessions; logger callbacks detach on destruction.
- Trial worker queues now drain and restart cleanly after `Session.End()` and when a play session starts with domain reload disabled.
- `FormElementEntry.element` remains readable after UI generation but can only be assigned by UXF; consumers should configure the serialized entry fields instead.
- Package and repository guidance now provide explicit entry points for agents and contributors.
- Package metadata now declares the UnityWebRequest module required by HTTPPost and UI web-file loading, so minimal consumers compile without development-project module assumptions.
- The clean Package Manager consumer now has explicit EditMode/PlayMode test assemblies and smoke tests for runtime session use and sample metadata.
- Consumer sample smoke tests now verify the packaged Unity 6 WebGL template as well as the sample README.
- The `[UXF_Rig]` Session component is enabled by default; Session and EventSystem fallback lifecycle code no longer runs in edit mode. UI/data-handler validation is now side-effect-free so inspecting the rig does not dirty scenes.

## 2.4.5

- Development baseline migrated to Unity 6000.3.25f1.

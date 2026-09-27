# UXF package changelog

## 3.0.0

### Installation and compatibility

- UXF is now a Unity Package Manager package at `Packages/com.immersivecognition.uxf`, targeting Unity 6000.3 and .NET Standard 2.1. Install the package through the version-pinned Git URL in the package README; import **UXF Examples** from Package Manager's Samples tab.
- Added explicit runtime, editor and test assemblies. The package declares UGUI 2.0.0, Input System 1.16.0 and UnityWebRequest 1.0.0 dependencies. UXF runtime scripts remain render-pipeline-agnostic.
- Example scenes use URP 17.3 materials and require URP setup in the consuming project. Their settings fixtures must be copied into the project's root `Assets/StreamingAssets`; WebGL templates must be copied into the root `Assets/WebGLTemplates`.

### Input, UI and platform integrations

- UXF pointer, keyboard, tracker and UI components support projects using the Input System alone or alongside the legacy Input Manager. An active UXF rig converts its legacy UI input module at runtime when the Input System is enabled.
- Windows file/save dialogs now use Win32 common-dialog APIs; folder selection uses Shell `IFileOpenDialog` on an STA thread. Removed the bundled `System.Windows.Forms` and Ookii dialog DLLs and the Windows-only `SFB.WindowWrapper` helper. Continue to call `SFB.StandaloneFileBrowser`.
- Windows directory selection now defers the native picker until the UI click ends and rejects reentrant folder dialogs. The final selection/cancel path and standalone player backends still require validation.
- Android FileSaver validation now uses app-private `PersistentDataPath` without requesting the legacy External (SDCard) permission. Updated WebGL string interop and Unity 6 platform/version checks.

### Data and lifecycle

- CSV output now quotes separators, quotes and line breaks; malformed quoted input fails explicitly. Check downstream CSV readers that relied on the earlier format.
- FileSaver rejects rooted or nested experiment/participant identifiers, preserves the caller's culture while formatting output, uses portable forward-slash relative paths, and handles repeated or early cleanup.
- Tracker serialization uses a worker queue; `Session.End()` waits for accepted tracker writes. FileSaver queue accounting and `ResultsDictionary` writes are synchronized.
- Session, logger and random-generator static state reset between play sessions. The `[UXF_Rig]` Session is enabled by default, and editor validation no longer modifies scenes merely by inspecting them.

### Editor and API migration

- Removed the EasyEvent reflection patch and global reorderable inspector in favor of Unity's native serialized controls.
- `FormElementEntry.element` remains readable but is now assigned by UXF. Configure its serialized entry fields rather than assigning the generated element directly.
- Build validation checks manual and UI-free Sessions and reports batchmode failures with `BuildFailedException` rather than opening a dialog.
- Added package-local agent guidance and recipes for installed-package consumers.

### Validation limits

- This release line is developed in Unity 6000.3.25f1. Earlier local package and clean-consumer EditMode/PlayMode suites passed, but final-source test runs, tagged installation and player/device checks are still required before claiming full platform support.
- Fast Enter Play Mode with scene reload disabled, Windows IL2CPP dialogs, browser upload completion, macOS/Linux native dialogs, Android devices and sample visuals remain unverified. `ExampleGlow` still uses Built-in Unlit/Color and needs a URP visual check.

## 2.4.5

- Development baseline migrated to Unity 6000.3.25f1.
- Added the initial Unity Package Manager manifest and package-local agent/contributor guidance.

# Unity project context

<!-- unity-onboarding:generated:start -->

Analyzed 2026-09-26 at the merged `origin/master` baseline (`b82033f`) plus the current uncommitted migration work. Root: `D:/UnityProjects/unity-experiment-framework`. UXF is an embedded UPM package under `Packages/com.immersivecognition.uxf`; clean-consumer installation and native test runs are represented by `ci/consumer`. See [upgrade backlog](../maintenance/Unity6-upgrade-backlog.md).

## Environment and structure

- **Confirmed:** Unity `6000.3.25f1` (`e1dba0a9aba4`), from `ProjectSettings/ProjectVersion.txt`; UXF `3.0.0` in `Packages/com.immersivecognition.uxf/VERSION.txt`.
- **Confirmed:** Built-in rendering configured (`GraphicsSettings.asset` has no custom pipeline); legacy Input Manager selected (`activeInputHandler: 0`). Runtime mouse/UI code uses `UnityEngine.Input`. No active multiplayer framework found; Multiplayer Center is an editor dependency, not evidence of multiplayer gameplay.
- **Confirmed:** The development manifest includes Test Framework 1.6.0, UGUI 2.0.0, XR Legacy Input Helpers 2.1.13, Navigation 2.0.14 and Timeline 1.8.13; the UXF package declares UGUI 2.0.0 and UnityWebRequest 1.0.0 as runtime dependencies and is listed in the manifest's `testables` array. Registry/built-in dependencies, no Git/local/scoped-registry dependencies in the manifest. Lockfile exists but is untracked in the audit baseline.
- `Packages/com.immersivecognition.uxf/Scripts`: core Session, Block, Trial, cascading Settings, Tracker, DataHandler implementations, optional UGUI, and editor tooling nested under Editor folders.
- `Packages/com.immersivecognition.uxf/Plugins`: browser JavaScript bridges and a macOS native file-browser bundle. Windows dialogs are implemented in C# with Win32 P/Invoke; the old managed dialog DLLs were removed. Linux wrapper exists but a corresponding native library was not found in the plugin inventory.
- `Packages/com.immersivecognition.uxf/Samples~`: shuttle, CSV, movement, multiscene and Corsi examples plus fresh-GUID WebGL templates that are imported through Package Manager's Samples tab. `Assets/StreamingAssets`: development-project example settings and CSV specifications. `Assets/WebGLTemplates`: development-project copies of the 2019 and 2020 templates.
- Package Manager imports samples into the generated, Git-ignored `Assets/Samples/` folder; keep editing the package's `Samples~` source and preserve sample GUIDs when testing an import.
- `Packages/com.immersivecognition.uxf/Tests/Editor`: editor tests using production handlers; `Packages/com.immersivecognition.uxf/Tests/Runtime`: PlayMode smoke coverage. `Assets/Testing`: manual AWS scenes/scripts.
- `Assets/Editor/ExportUFXPackage.cs`: interactive legacy package exporter. The embedded UPM manifest, package README/AGENTS guide, first-party runtime/editor/test assembly definitions, clean-consumer fixture and archive workflow now exist under `Packages/com.immersivecognition.uxf`/`.github`; tagged/archive installation and player builds remain to be executed.
- `docs/wiki`: checked-in wiki source; `docs/xml2md.py`: API Markdown generator; `docs/index.html`: website; `webserver_example`: Flask upload example.

## Architecture and boundaries

Session is a MonoBehaviour coordinator, optionally accessed through a public static singleton. It owns mutable blocks, which own mutable trials. Trials coordinate tracking and saving. Settings resolve through parent `ISettingsContainer` instances. UnityEvents connect experiment construction and scene behavior. DataHandler is an abstract MonoBehaviour; FileSaver uses a dedicated worker and custom BlockingQueue; HTTPPost uses coroutines; DynamoDB uses JavaScript callbacks. UI is optional in the API, and build validation now traverses every Session independently of UI objects. Session, SessionLogger, the shared random generator and Trial's worker queue now reset runtime state at play-session startup; fast Enter Play Mode still needs PlayMode coverage.

Existing abstractions include `IExperimentUnit`, `ISettingsContainer`, `IExperimentBuilder`, `IDataAssociatable` and `IStandaloneFileBrowser`. Interface expansion should target concrete testing and dependency needs. Many properties already have private/protected setters; public mutable fields and exposed collections are the larger encapsulation issue.

Package assembly boundaries now exist: `UXF.Runtime`, editor-only drawer assemblies, and separate Editor/PlayMode test assemblies. Native Unity runs pass 58/58 package EditMode and 1/1 PlayMode tests; the clean consumer passes 59/59 EditMode and 2/2 PlayMode tests. Player compilation and platform backends still need validation; do not infer those from editor tests.

Conventions: primarily `UXF`, `UXF.UI`, `UXF.EditorUtils` namespaces; mixed indentation; XML documentation on much of the public API; extensive public serialized fields and custom property attributes. The repository and package now include explicit `AGENTS.md` guides for context discovery and safe edits.

## Scenes and startup

`ProjectSettings/EditorBuildSettings.asset` now contains only development and manual AWS scenes; Package Manager samples are ignored until a consumer imports them and configure their own scenes (the multiscene sample adds its pair in `OnValidate`). UIController or manual sample code calls Session.Begin; `onSessionBegin` builds experiments and starts trials. The multiscene sample changes scenes between trial types. These are serialized build settings, not a verified player execution trace.

## Validation and tooling

NUnit tests cover session/trial construction and events, settings, tables/JSON/CSV, locales, saving, trackers and UI logic. Editor and PlayMode test assemblies now exist, including a UI-free session smoke test and a worker-restart regression. Runtime, editor, sample and clean-consumer smoke sources pass isolated Unity Roslyn compilation. Unity's native batchmode runner has now executed the package suite on Unity 6000.3.25f1: 58/58 EditMode tests and 1/1 PlayMode test passed. The clean consumer now passes 59/59 EditMode tests (including package sample metadata) and 2/2 PlayMode tests through the file-based Package Manager dependency. Player-platform builds still need a licensed run.

Unity CLI 1.0.0-beta.11 is installed locally and the project declares `com.unity.pipeline` `0.8.0-exp.1`; `ProjectSettings/PackageManagerSettings.asset` enables pre-release packages for this tooling dependency. After a clean Editor restart, the CLI can inspect the Console, report editor status and recompile status, enumerate 58 EditMode tests and control the Editor. The Pipeline test wrapper can leave `Temp/pipeline_test_request.json` pending after an interrupted or domain-reloading run; use Unity's native batchmode runner for deterministic CI evidence and clear that request only after closing the affected Editor. Existing Editor.log and generated compiler response files were inspected as supporting local evidence. The response file confirms `UNITY_6000` exists and `UNITY_2020_OR_NEWER` is absent. The project-side import diagnostics found in Editor.log (malformed `UXFEditorBase.cs.meta`, duplicate Test Runner references and orphaned empty-folder metas from the Samples move) were corrected; remaining Rider, licensing and DNS messages are environment diagnostics. An obsolete local Rider path appears in that log. Android FileSaver validation now targets app-private `PersistentDataPath` and no longer requires the legacy External (SDCard) permission.

## Constraints and unknowns

The workspace already contains modified settings, materials, sprite metadata, version and manifest, plus untracked lock/settings/import outputs. Preserve this work. The current implementation also adds package metadata/docs and a CI archive workflow; these are scaffolding, not proof of a clean consumer install or release artifact. Minimum supported Unity version, supported platform/backend matrix, package layout/assembly boundaries, release version and API-breaking policy need explicit decisions before implementation. Windows/WebGL/Android/VR are prominent in code/docs; macOS/Linux/iOS support needs actual player evidence. Shader visuals, native binary architectures, serialization migration, browser uploads and live services were not executed or certified.

Principal evidence: project version/manifest/lock/settings; README and wiki; Session/Trial/Block/Settings/Tracker/DataHandler/FileSaver/HTTPPost/WebAWSDynamoDB; UXFWizard/UXFBuildPreprocessor/SessionEditor/UIControllerEditor/UXFEditorBase/ExecutionOrderManager; table/CSV/JSON utilities; test sources; JS bridges/templates; exporter; documentation generator and Flask example.

<!-- unity-onboarding:generated:end -->

# Unity 6 upgrade and package release backlog

Audit date: 2026-09-26. Baseline: merged `origin/master` at `b82033f` plus existing local changes; Unity **6000.3.25f1**, UXF **3.0.0**. This is a task inventory, not a claim that the upgrade has been validated. Source/configuration, local logs and official Unity references were inspected. A first implementation tranche is now present in the workspace; the validation evidence and remaining work are recorded per task. No live cloud operations or scene saves were performed.

## How to use this backlog

Every unchecked task is open. **P0** establishes the baseline; **P1** should block the affected supported release feature; **P2** is planned modernization; **P3** is optional or cleanup. Severity describes impact separately. **Confirmed** means the stated code/configuration fact is directly visible, not necessarily that a player failure was reproduced. **Likely** means integration/runtime evidence is still required. **Decision** denotes proposed product/API work rather than a defect. Size: **S** localized, **M** several files, **L** cross-cutting/platform work. Sizes exclude waiting for platform tooling. The backlog contains 73 tasks, including J01–J05 for agent understanding in installed consumer projects.

Paths below are repository-relative. Method/class names are stable evidence anchors. Tests specified in a task are its acceptance criteria, not separate duplicate work. The [validation matrix](Unity6-validation-matrix.md) expands the cross-cutting checks. The [project context](../AI/UnityProjectContext.md) records architecture and audit limits.

## Recommended execution order

1. A01–A04: capture upgrade baseline, support policy and reproducible import/test results.
2. B01–B08 and C01–C07: fix supported-platform compatibility and data integrity, accompanied by regression tests.
3. D01–D08 and F01–F05: establish package/assembly boundaries, isolated tests and clean consumer projects.
4. E01–E06: automate validation, package creation and release artifacts.
5. G01–G07: stage API improvements with migration protection; do not bundle an unbounded rewrite into compatibility fixes.
6. H01–H08 and I01–I06: publish accurate docs, audit examples/dependencies and finish release evidence. Documentation and targeted tests should travel with each earlier fix.
7. J01–J05: ship and validate package-local guidance for agents alongside the UPM documentation work; this need not wait for all optional API modernization.

Implementation started in this workspace: the Unity 6 browser bridge/version-guard/wizard edits, Win32 file-dialog migration with removal of legacy managed dialog assemblies, UI-free build validation, quoted CSV parser/writer, FileSaver path containment and explicit culture formatting, static Session/logger/random reset hooks, hardened upload example, embedded package manifest, package README/AGENTS/license, repository and package agent guides, root installation guidance, native inspector replacement, first-party runtime/editor/test assembly definitions, Package Manager Samples metadata, sample StreamingAssets fixtures, isolated FileSaver test output, changelog and package-archive workflow are present. The package is now under `Packages/com.immersivecognition.uxf/Samples~`; clean consumer installation and local native test suites now pass, while tagged-install, sample-by-sample and player/platform checks remain open.

P1 tasks for a platform may be deferred only if that platform/feature is explicitly excluded from the release support matrix. No claim of exhaustive *runtime defects* is possible without builds; this inventory covers observed issues and the remaining investigations needed for a defensible release.

## A. Baseline and support decisions

### [ ] A01 — Preserve and classify the Unity upgrade diff

**P0 · High · Confirmed · S**

Evidence: modified `ProjectSettings`, `Packages/manifest.json`, UXF version, sprite `.meta` files and ExampleFire material; untracked package lock, settings, Resources/BillingMode and generated example/import files. Record intended source changes versus regenerated/local outputs before committing. Review GUIDs and imported settings instead of reverting everything or accepting everything blindly.

Done: the upgrade is an independently reviewable commit; lockfile policy is recorded; no generated logs or unexplained service/sample artifacts enter the release. Existing user changes remain preserved.

### [ ] A02 — Define the supported Unity/platform/API matrix

**P0 · High · Decision · S**

Evidence: `ProjectVersion.txt` says 6000.3.25f1, README says 2018/2019, wiki Compatibility says 2018+, and platform checks differ among handlers. Decide minimum Unity, tested Unity versions, Windows/macOS/Linux/Web/Android/iOS scope, Mono/IL2CPP coverage, rendering and input support, and whether this is a breaking UXF release.

Done: one support table distinguishes tested, intended and unsupported combinations; package metadata, CI and docs derive from it. Legacy removal tasks depend on this decision. Proposed first target: current 6000.3 editor, with other versions supported only after validation.

### [ ] A03 — Capture a fresh compile, Console and test baseline

**P0 · High · Likely · M**

Evidence: the current Editor.log showed a malformed `UXFEditorBase.cs.meta`, duplicate Test Runner references in the new test asmdefs, and orphaned empty-folder metas left by the Samples move; it also contains environment-only Rider, licensing and DNS diagnostics. The project-side import diagnostics are now fixed. A fresh licensed test-result artifact is still missing. Capture exact source revision, clean package resolution/import, compiler errors/warnings, test discovery, failures and platform modules. Resolve the local Rider path separately from package code. Isolate file-writing fixtures before running them (F02).

Done: repeatable commands and archived logs/results establish real failures; each failure is mapped to a task; missing licensing/modules are reported as environment blockers rather than code defects. Do not treat this audit's static review as a passing build.

### [ ] A04 — Define release and migration acceptance gates

**P0 · High · Decision · S**

Evidence: manual exporter, no changelog/release pipeline, extensive serialized public fields. Decide package identity/versioning, supported old `.unitypackage` migration, API deprecation window and data-format compatibility. Separate upgrade, packaging and API-breaking milestones.

Done: release checklist includes fresh install, upgrade of an existing experiment, preserved UnityEvents/references, output-schema checks and platform evidence; no release is based solely on the development scene compiling.

## B. Unity compatibility and obsolete tooling

### [ ] B01 — Correct wizard target selection and compatibility checks

**P1 · High · Confirmed · M · Depends A02**

Evidence: the wizard now targets `NamedBuildTarget.Standalone` and recommends `.NET Standard 2.1`; the Win32 file-dialog adapter no longer requires the legacy `.NET Framework` profile. Fast Enter Play Mode remains unsupported because scene reload and integration state have not been proven safe without reloads, even though known static Session/logger/random state now resets at subsystem registration. The check must accept Unity's normal behavior when `enterPlayModeOptionsEnabled` is false and must reject only explicitly enabled options that skip domain or scene reload.

Done: checks operate on the intended active target, recommend only required API settings, correctly interpret reload options, and do not silently alter unrelated targets. Exercise Windows/Web/Android choices and standard/fast enter-play configurations.

### [ ] B02 — Remove obsolete Unity branches and fix version guards

**P1 · Medium · Confirmed · M · Depends A02**

Evidence: the runtime HTTP/UI web-request code, EventSystem fallback, session debugger, build preprocessor, setup wizard and Web AWS target checks no longer carry pre-Unity-6 branches. The retired EasyEvent and reorderable files are absent; remaining version guards need review only where they protect supported platform/editor behavior.

Implementation started: removed obsolete fallback branches and warning suppression from the current runtime/editor paths while preserving the Unity 6 APIs. The full runtime and editor source sets compile through Unity's generated Roslyn response files after filtering references to the retired dialog assemblies. The remaining branches are intentionally isolated to the two legacy editor extensions until their behavior is replaced or the extensions are removed.

Done: use supported APIs directly for the chosen minimum version, retain only necessary tested branches, remove blanket suppression and obsolete messages. `UNITY_6000` **does exist** in this editor; replacing exact-major guards with minimum-version guards is future-proofing, not a current undefined-symbol fix. Verify every retained compilation branch.

### [ ] B03 — Retire the global reorderable inspector safely

**P1 · Medium · Confirmed · M · Depends A02**

Evidence: the former `ReorderableArrayInspector` installed a fallback editor for all UnityEngine.Object types; SessionEditor and UIControllerEditor used only its property-range helpers in this package. Unity 6 now supplies native serialized list/event controls for the retained inspectors, and the fallback has been removed.

Implementation started: replaced the two dependent inspectors with a narrow `UXFEditorBase` that uses native `SerializedProperty` drawing, removed the global fallback and its reflection helpers, and removed the package's reorderable/edit-scriptable attributes after auditing all references. Serialized fields remain in place; inspector parity and multi-object/prefab behavior still need editor acceptance tests.

Done: replace dependent custom-inspector behavior using supported serialized-property UI; remove the global fallback, obsolete attributes/helpers and third-party code only after auditing all references. Preserve tabs, handler drawers, nested data, drag order, prefab overrides, undo/redo and multi-object editing. Test feature parity before deleting the folder; native list support alone is not proof of complete parity.

### [ ] B04 — Audit other bundled inspector extensions

**P2 · Medium · Likely · M**

Evidence: `EasyEventEditor.cs` patched Unity's internal event drawer map through reflection, but its Unity 6 path was a no-op; the reorderable reflection utilities were only consumed by the two UXF inspectors. ReadOnly, InspectorButton, form and handler drawers remain first-party editor features.

Implementation started: removed the dead EasyEvent reflection extension, the unused reorderable support files and the unreferenced `HideIfExampleScript` from the runtime assembly; native Unity 6 event/list controls now own those interactions. Retained first-party drawers remain to be tested for inspector, event-binding, undo and prefab behavior.

Done: retained drawers pass inspector, event-binding, undo and prefab tests in the supported editor; no package-wide override changes unrelated components. Dead example scripts such as HideIfExampleScript are removed from the shipped runtime or moved to samples.

### [ ] B05 — Replace obsolete JavaScript browser interop

**P1 · High · Confirmed · M**

Evidence: `AWSJS.jslib`, `WebFunctions.jslib` and `StandaloneFileBrowser.jslib` now use `UTF8ToString` in place of the removed `Pointer_stringify` bridge. Unity documents this migration in [browser interop migration](https://docs.unity3d.com/6000.3/Documentation/Manual/web-interacting-browser-deprecated.html). Callback scope, SendMessage, heap access, object URL disposal and repeated listener installation still need browser acceptance tests.

Done: Web player exercises upload, download, clipboard and mocked AWS callbacks with Unicode; no obsolete-function or missing-global errors. A successful C# compile is insufficient.

### [ ] B06 — Replace legacy Web templates and make cloud scripts optional

**P1 · High · Confirmed · L · Depends B05, D05**

Evidence: `Assets/WebGLTemplates/UXF WebGL 2019` uses UnityLoader; 2020 template uses createUnityInstance; both load AWS SDK 2.776.0 regardless of handler. Wizard/build processor require these template names. Rebuild against the chosen Unity template and audit the AWS dependency/version/API separately; remove the 2019 template after A02.

Done: loading/error/progress UI, sizing/fullscreen, compression/hosting headers and callbacks work in the browser matrix; non-AWS experiments do not require AWS script loading. Offline/CDN failure is visible. Document custom-template integration and imported-template ownership.

### [ ] B07 — Make Android settings/profile access match actual storage behavior

**P1 · High · Likely · M**

Evidence: UXFBuildPreprocessor previously demanded the legacy External (SDCard) permission while requiring PersistentDataPath. The wizard and build check now use app-private PersistentDataPath without changing that permission. ExperimentProfileSelector uses Directory.GetFiles/CreateDirectory; CSVExperimentBuilder uses File.Exists/ReadAllLines on StreamingAssets, while the build processor allows Android UI profile acquisition. Device validation is still required for packaged settings/CSV access and lifecycle behavior.

Done: remove unjustified permission requirements after target validation; load packaged settings/CSV through a platform-capable provider or explicitly reject unsupported modes. Editor-side validation no longer enables or requires External (SDCard) permission for FileSaver, but device tests must still prove startup, CSV loading and data saving without desktop filesystem assumptions; user overrides are preserved.

### [ ] B08 — Validate native file dialogs and platform gating

**P1 · High · Likely · L**

Evidence: the Windows Forms/Ookii managed binaries have been removed and replaced with Win32 common-dialog P/Invoke. A macOS bundle remains in Plugins; Linux wrapper imports `StandaloneFileBrowser`, but no Linux native binary appears in the inspected tree. Do not promise Linux dialog support from C# wrappers alone. Audit importer platform/CPU settings, native architectures, IL2CPP support and licensing.

Implementation started: the Windows adapter preserves open/multi-select/folder/save behavior through `comdlg32.dll`, `shell32.dll`, `user32.dll` and `ole32.dll`. A reproduced folder-button stack overflow led to deferred click handling and a guard against a second modal folder dialog. A direct Windows harness found and fixed invalid `BROWSEINFO` marshalling; it opened and canceled the native folder dialog successfully. The Unity PlayMode reentry regression passed. Run the UXF folder-button flow in a Windows Editor with an existing directory, then cancel, select a folder and click again; repeat in Windows Mono and IL2CPP players. Also check file/save dialogs, Unicode paths and owner-window behavior. The original SFB upstream still documents a .NET 2.0 Windows dependency, so replacing this adapted backend requires a separate API, license and platform review.

Done: supported native dialogs work on actual OS/architectures; missing/unsupported backends fail gracefully. Unneeded binaries are excluded from other players. Replace or remove dependencies only with a tested replacement and notices.

### [ ] B09 — Support or clearly delimit modern input configurations

**P2 · High for Input-System-only consumers · Confirmed · M**

Evidence: activeInputHandler is legacy; KeypressEvent, InputFieldKeyboardReturnEvent, ClickOutsidePanelEvent and mouse trackers use Input; EventSystemFallback instantiates a preset event-system prefab. Add an optional input adapter/backend or explicitly state the restriction.

Done: legacy, Both and Input-System-only projects either work as advertised or get an actionable configuration diagnostic. No duplicate EventSystems in additive scenes; keyboard submit, pointer UI, notes and tracking remain functional. Avoid forcing an input-package dependency on core-only consumers.

### [ ] B10 — Replace import-time execution-order mutation

**P2 · Medium · Confirmed · S**

Evidence: `ExecutionOrderManager` scans all runtime scripts by class-name string on InitializeOnLoad and writes FileSaver importer execution order. Determine whether explicit teardown ordering still requires it; prefer explicit lifecycle ownership or a class-level default where suitable.

Done: package import/reload does not mutate cached package metadata, and save completion on stop/destroy/quit is regression-tested before deleting the workaround.

### [ ] B11 — Validate all sessions at build time, including UI-free experiments

**P1 · High · Confirmed · M**

Evidence: `UXFBuildPreprocessor.OnProcessScene` now discovers Sessions independently of UIController objects and applies local-file/handler checks to UI-free and manual-start Sessions. UI-specific settings checks remain attached to UIController objects. CancelBuild still displays a modal dialog outside batchmode; inspect inactive-object intent and provide useful batchmode failures.

Implementation started: moved local-file access validation out of the UI loop so a Session without a UI cannot bypass that platform check, and made build cancellation skip the modal dialog in batchmode. Remaining acceptance work covers inactive objects, all handler combinations and actual batchmode/manual builds.

Done: unsupported handlers are rejected in manual-start/UI-free and UI scenes, inactive objects are handled intentionally, missing parent references give clear errors, and CI never waits on a dialog.

## C. Data integrity, lifecycle and remote handling

### [ ] C01 — Give save operations observable completion and failure

**P1 · High · Confirmed · L**

Evidence: DataHandler returns a location synchronously; FileSaver queues work and logs worker exceptions; HTTPPost.CleanUp does nothing while coroutine requests remain outstanding; Session.End assumes cleanup forces completion and then reports completion. Define queued/succeeded/failed semantics and an explicit flush/completion contract compatible with Unity and Web.

Done: the UI/session cannot report successful durable saving while a handler has failed; multiple handlers expose partial failure; bounded retries, cancellation and shutdown behavior are specified and tested. Preserve existing extension points through adapters where needed.

### [ ] C02 — Make FileSaver lifecycle and queue synchronization robust

**P1 · High · Likely · M · Depends C01**

Evidence: CleanUp now tolerates no setup and repeated calls, and clears the worker reference after joining. A timeout, enqueue-versus-close race coverage and the unsynchronized `BlockingQueue.NumItems` read remain; reproduce those before choosing a replacement queue.

Done: cleanup before setup, double cleanup, restart, concurrent enqueue/close and worker exceptions terminate deterministically without hanging or losing accepted writes. Queue completion, bounded waiting and error reporting are explicit; no worker is left running after failed tests or destroyed sessions.

### [ ] C03 — Make session/trial transitions exception-safe and idempotent

**P1 · High · Likely · M**

Evidence: Session.Begin assigns state and starts handlers without a rollback guard; Session.End sets isEnding before SaveResults and later operations that can throw outside catch blocks. Public trial state permits invalid transitions. Define reentrant Begin/End, listener failures, partial setup and cleanup semantics.

Done: injected failure at each phase leaves a recoverable state, each started handler gets cleanup, and end events fire according to a documented order exactly once. Test automatic last-trial end, explicit end, destroy and quit together.

### [ ] C04 — Make CSV parsing/output lossless and explicit

**P1 · High · Confirmed · M**

Evidence: UXFDataTable now parses quoted separators/quotes/newlines, detects comma/semicolon input and rejects malformed quoted records; its tests cover round-tripping those values. SessionLogger still removes commas and flattens stacktraces, and culture-specific delimiter/legacy output compatibility and log preservation remain.

Done: quoted separators, quotes, CR/LF, Unicode, blank/null values, duplicate/empty headers and malformed rows have tested behavior. Specify output schema/delimiter/precision and migration impact on analysis scripts. Logs preserve original content via proper encoding rather than deletion.

### [ ] C05 — Preserve arbitrary binary data through every handler

**P1 · High · Confirmed · M**

Evidence: HTTPPost.HandleBytes decodes arbitrary bytes as UTF-8 before sending form text. Audit FileSaver, DownloadOrCopyHandler and WebAWSDynamoDB against the same bytes contract; unsupported representations should reject explicitly rather than corrupt data.

Done: bytes including invalid UTF-8 and zero bytes round-trip exactly through supported transports using a specified binary/base64 format; content metadata and server decoding match; unsupported handlers give explicit errors.

### [ ] C06 — Contain all data paths and prevent accidental overwrites

**P1 · High · Confirmed · M**

Evidence: FileSaver now rejects rooted or separator-containing experiment/participant components and checks the resolved session path stays under its root; regression coverage was added. `webserver_example.py` likewise rejects absolute/traversal paths. Overwrite policy, symlink behavior, data-name containment and full cross-platform coverage remain.

Done: traversal, absolute paths, mixed separators, Unicode and same-session collisions are tested; writes stay inside the selected root; an existing result is never silently replaced contrary to policy. Flask example tests use temporary directories and no network deployment.

### [ ] C07 — Restore culture isolation and portable relative paths

**P1 · Medium · Confirmed · M**

Evidence: FileSaver now applies EN-US formatting explicitly without changing the caller's CurrentCulture. The previous GetRelativePath implementation appended a Windows backslash before constructing Uris on every OS.

Implementation started: GetRelativePath now uses `Path.GetRelativePath` and normalizes separators to forward slashes; regression cases cover nested paths, trailing separators and same-root output. Full Windows/macOS/Linux runtime evidence remains open.

Done: starting/ending a session leaves host culture unchanged; relative outputs are consistent and remain inside root on Windows/macOS/Linux with spaces, accents and trailing separators. Existing output consumers remain compatible or receive migration guidance.

### [ ] C08 — Bound and dispose HTTP requests

**P1 · High · Confirmed · M · Depends C01**

Evidence: HTTPPost creates UnityWebRequests without disposal, timeout, completion tracking or structured error delivery. Add disposal, timeout and retry policy, cancellation on teardown, and meaningful status/body reporting without logging authentication values. Clarify idempotency before retrying POSTs.

Done: deterministic fake/local server tests cover success, HTTP error, timeout, disconnect, retry and destroy; no dangling requests or duplicate stored data. Completion integrates with C01 rather than a second independent status mechanism.

### [ ] C09 — Verify cloud callback, batching and browser-close reliability

**P1 · High · Likely · L**

Evidence: WebAWSDynamoDB requestCallbackMap and CleanUp; AWSJS batchWriteItem callbacks and beforeunload handling. Audit callback removal on failure/timeout, unprocessed batch items, service limits, duplicate callbacks, reconnects and repeated session initialization. Do not promise browser-close delivery.

Done: mocked integration tests exercise failures and partial batches; live tests are opt-in against disposable resources; outstanding uploads and unrecoverable failures are visible before completion. Credentials/configuration are not bundled into published samples.

### [ ] C10 — Reset static state and event subscriptions across sessions

**P2 · High · Likely · M**

Evidence: public Session.instance and SessionLogger.instance are now reset at SubsystemRegistration; the shared random generator and Trial worker queue also reset, and Session/SessionLogger clear references and detach callbacks during destruction. Fast Enter Play Mode still needs an actual PlayMode run because failure-path ordering, persistent objects and every event subscription have not been exercised.

Done: no stale session, duplicate event, leaked logger or orphaned file worker after repeated play/stop, scene unload or failed setup. Support fast Enter Play Mode or document and correctly detect the remaining limitation.

### [ ] C11 — Define measurement time and long-session memory behavior

**P2 · Medium · Decision · M**

Evidence: Trial/Tracker use scaled float Time.time; trackers and logger buffer tables, and CSV conversion materializes arrays. Decide scaled versus unscaled/monotonic time, precision, pause behavior and expected experiment duration/sample rate. Measure before redesigning storage.

Done: documented timestamps pass pause/timeScale/long-duration tests; stress capture reports allocations, retained memory, frame impact and flush time. Introduce chunking/streaming only if measured workloads require it, preserving data order.

## D. Unity Package Manager distribution

### [ ] D01 — Create a single authoritative UPM package root

**P1 · High · Confirmed gap · M · Depends A02, A04**

Evidence: the package manifest now exists at `Packages/com.immersivecognition.uxf/package.json`, is versioned `3.0.0`, declares Unity 6/UGUI/UnityWebRequest requirements and advertises a `Samples~` entry. A clean consumer resolves the local package and compiles/tests it; actual tagged Git/archive installation and sample behavior are still open. Confirm the package ID/root and development-project arrangement with one copy of runtime code. Refine semver metadata, minimum Unity, dependencies and documentation/license links. Follow [Unity package layout](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-layout.html).

Done: a clean consumer can install a local package and compile it; the development project consumes the same source; no duplicate Assets and Packages copies. Package identity and version have one source of truth.

### [ ] D02 — Introduce explicit runtime/editor/test/sample assemblies

**P1 · High · Confirmed gap · L · Depends D01**

Evidence: runtime/editor/test assembly definitions now exist for the package, nested editor drawers and Editor/PlayMode tests. The old test-only `TestableDataHandler` fixture was removed; tests now exercise production FileSaver behavior. Root native runs pass 58/58 EditMode and 1/1 PlayMode tests, and the clean consumer passes 59/59 EditMode and 2/2 PlayMode tests. Optional UI/platform code remains in the runtime assembly behind existing Unity guards; clean player compilation and backend coverage must still verify that boundary.

Done: core/player assemblies have no unconditional editor/NUnit dependencies; test assemblies are discoverable and excluded from players; all supported backends compile. Preserve serialized type identity and provide migration for changed assembly-qualified references.

### [ ] D03 — Declare only actual consumer dependencies

**P1 · Medium · Confirmed gap · M · Depends D02**

Evidence: the package now declares only the runtime dependencies it directly uses (`com.unity.ugui` and `com.unity.modules.unitywebrequest`); development-only services remain in the host project. The clean consumer initially exposed the missing UnityWebRequest declaration, which is now fixed and covered by the 59/59 EditMode and 2/2 PlayMode native runs. Optional native/cloud/input functionality still needs platform-specific dependency and player-build review.

Done: a minimal consumer resolves without inheriting unrelated project services; core-only usage works; optional features document their dependencies. Removal is justified by usage analysis and clean import tests, not by package age alone.

### [ ] D04 — Convert examples into importable, self-contained samples

**P1 · High · Confirmed gap · L · Depends D01, D02**

Evidence: the example tree is now under `Packages/com.immersivecognition.uxf/Samples~` and is advertised through the package manifest. Unity ignores the package's `Samples~` source until a consumer imports it, so development build settings no longer point at unavailable sample scenes. A clean-consumer Editor smoke test verifies that the resolved package exposes `Samples~/README.md` and the Unity 6 WebGL template; sample StreamingAssets fixtures and fresh-GUID WebGL templates travel with the package, but each sample still needs standalone import/build execution and any root-level `Assets/StreamingAssets` or `Assets/WebGLTemplates` copy must be made explicit.

Done: import each sample alone and in combination into a fresh project; scenes/prefabs/events resolve, CSV/settings exist and multiscene configuration is explicit. Tests and AWS experiments are not accidentally shipped as production runtime content.

### [ ] D05 — Make paths and setup work from immutable package installs

**P1 · High · Confirmed gap · M · Depends D01**

Evidence: wizard/exporter now read `Packages/com.immersivecognition.uxf/VERSION.txt`; the wizard directs users to Package Manager Samples; imported sample instructions now cover root `StreamingAssets` and `WebGLTemplates` copies; templates and profile data still expect special Assets folders; execution-order script mutates importers. Resolve package metadata/assets without hardcoded old paths and make setup rerunnable without overwriting user files.

Done: registry/Git cached installs are read-only in practice; setup writes only user-owned Assets/configuration, handles conflicts and can be rerun without overwriting user edits. No package-cache writes or manual path surgery are needed.

### [ ] D06 — Provide version-pinned Git installation and update instructions

**P1 · High · Confirmed gap · M · Depends D01, E04**

Evidence: root/package README now documents a tagged Git URL with `?path=Packages/com.immersivecognition.uxf`, as described by [Unity Git dependencies](https://docs.unity3d.com/6000.3/Documentation/Manual/upm-git.html); actual installation, update and rollback are still unverified. Decide separately whether a scoped registry is worthwhile; it is not required for Git-based Package Manager installation.

Done: actual released tag installs through Package Manager in a new project; update, rollback and uninstall are demonstrated; docs identify prerequisites and exact URL shape. Do not advertise an uncreated tag or registry.

### [ ] D07 — Provide an Assets-to-UPM migration path

**P1 · High · Likely · L · Depends D02, D04**

Evidence: existing users have Assets/UXF serialized scripts/prefabs/UnityEvents and may modify bundled examples/UI. Preserve .meta GUIDs during relocation; detect duplicate installs; document backup, removal and user-customization handling. Avoid indiscriminate folder deletion.

Done: an archived representative legacy consumer upgrades without missing scripts, lost settings or broken events; duplicates are detected; serialized renames have migration support and data outputs remain readable.

### [ ] D08 — Package license, notices, docs and a reviewed file allowlist

**P1 · Medium · Confirmed gap · M**

Evidence: root and package LICENSE files now exist; MiniJSON, SFB and web template libraries still need provenance/notice review. The former ReorderableInspector and EasyEventEditor sources have been removed. Current exporter recursively exports all UXF, StreamingAssets and templates.

Done: redistributed components have appropriate attribution/license files; package contains intended sources, .meta files and documentation only, excluding tests where inappropriate, generated data, credentials, logs and dev-only settings. Inventory is checked against the built artifact.

## E. GitHub CI, artifacts and releases

### [ ] E01 — Add PR import/compile/test CI

**P1 · High · Confirmed gap · M · Depends A03, D02, F02**

Evidence: `.github/workflows/package.yml` validates the path-scoped manifest, archives the package on PR/push and contains gated licensed Unity test jobs. Local native Unity runs provide current evidence (58/58 EditMode + 1/1 PlayMode in the package); a GitHub-hosted licensed run still depends on `UNITY_CI_ENABLED` and license secrets, so PR enforcement and uploaded-result coverage are not yet proven. Pin the supported editor with reproducible dependency resolution, bounded job time, test discovery checks and uploaded logs/XML results. Do not assume fork PRs can access credentials.

Done: a clean PR passes; intentionally broken source/test and zero discovered tests fail the job; failure artifacts remain available. Cache keys include relevant Unity/package inputs; untrusted PR code does not receive publishing/service secrets.

### [ ] E02 — Add clean consumer and platform smoke-build CI

**P1 · High · Confirmed gap · L · Depends E01, D04**

Build a temporary consumer using the actual package artifact, not only this development project. Add Windows and Web smoke builds first if selected by A02, then supported OS/Android/IL2CPP combinations; distinguish automated and manual hardware/browser checks.

Evidence: the clean consumer fixture now uses explicit runtime and Editor test assemblies and passes 59/59 EditMode plus 2/2 PlayMode tests against the local package path. The licensed CI job now downloads and unpacks the exact archive produced by the package job before resolving the consumer dependency; Git-tag installation and player/platform smoke builds remain open.

Done: package install plus representative sample build succeeds without hidden project assets; editor/NUnit/native dependencies cannot leak unnoticed. Unsupported CI platform coverage is explicitly recorded, not shown as passed.

### [ ] E03 — Build versioned UPM artifacts without interactive Unity UI

**P1 · High · Confirmed gap · M · Depends D01, D08**

Evidence: ExportUFXPackage.ExportPackage remains an interactive menu flow with a dialog and broad export roots. `.github/workflows/package.yml` now creates a versioned ZIP, SHA-256 checksum and checks required package entries, but package allowlists, dependency validation and release provenance remain. Asset-package export is a separate compatibility decision.

Done: CI produces an installable UPM artifact from one commit, verifies metadata/version consistency and publishes a checksum/file inventory. Re-running produces equivalent content, with any nondeterministic archive metadata documented.

### [ ] E04 — Add gated tagged-release workflow

**P1 · High · Confirmed gap · M · Depends E01–E03**

Implementation started: the workflow now gates a release job to pushed `v*`/numeric tags, verifies the checksum and tag/package version, attaches the ZIP and checksum, and limits `contents: write` to that job. Changelog consistency, immutable-release behavior and broader artifact provenance remain to validate.

Done: dry-run produces reviewable artifacts without publishing; mismatched versions/tests block release; repeated invocation cannot silently replace a release with different bytes. Registry publication, if selected, uses its own explicit credentials and approval policy.

### [ ] E05 — Retain or retire `.unitypackage` export deliberately

**P2 · Medium · Decision · M · Depends A04, D07**

Evidence: existing users and README rely on legacy export. If retained, extract a public batchmode export method from the dialog wrapper, trim/validate version text and use an explicit content list; if retired, give users a final supported legacy release and migration instructions.

Done: legacy output imports into a clean project without duplicate code/test helpers/private assets, or retirement is clearly documented. UPM and legacy artifacts cannot drift to different implementations.

### [ ] E06 — Automate documentation and package hygiene checks

**P2 · Medium · Confirmed gap · M · Depends H03**

Evidence: manual XML/Markdown generation and broad recursive exports remain; the new workflow checks basic package manifest/semver and required files but not docs, GUIDs, file allowlists or compile diagnostics. Add those checks.

Done: a deliberately stale generated reference or broken manifest fails; generated docs do not churn nondeterministically; checks are runnable locally. Wiki publication remains separate from PR validation.

## F. Tests and verification

### [ ] F01 — Establish discoverable EditMode and PlayMode suites

**P1 · High · Confirmed gap · M · Depends D02**

Evidence: package tests have explicit runtime/editor/test assemblies. Native Unity runs pass 58/58 EditMode and 1/1 PlayMode tests in the development project; a clean consumer passes 59/59 EditMode and 2/2 PlayMode tests, including package metadata and UI-free runtime smoke coverage. Broader lifecycle/UI/reload and player-backend coverage remains open. Keep pure-model tests separate from lifecycle tests and verify actual NUnit/Test Framework compatibility instead of blanket-renaming assertions.

Done: expected fixture/case inventory is recorded, both suites are discoverable locally and in CI, and no test helper or NUnit reference ships in a player. No unexplained ignored/skipped tests.

### [ ] F02 — Make fixtures isolated, repeatable and safe

**P1 · High · Confirmed · M**

Evidence: TestFileIOManager now allocates a unique temporary root per test, shuts down its worker in teardown and removes output recursively. FileSaver culture is restored, SaveTrialData uses isolated temporary storage, and Session/SessionLogger fixtures clear stale static state. Remaining shared fixtures and concurrency cases still need the same isolation treatment; teardown must remain finally-safe.

Implementation started: TestFileIOManager now allocates a unique temporary root per test, shuts down the worker in teardown and removes its output recursively. The remaining shared session fixtures still need the same isolation treatment.

Done: repeated/reordered suites pass without external writes, residue, live threads or reliance on old output files. Tests do not destroy unrelated scene objects through Session.instance.

### [ ] F03 — Strengthen concurrency and persistence assertions

**P1 · High · Confirmed · M · Depends C01–C02, F02**

Evidence: TestMultithreading.AddSettingsDuringWriting still has no saved-content assertions; TestFileSaver.WriteManyFiles queues writes and deletes paths without checking every payload. A FileSaver traversal regression now exists, but concurrency still relies on lifecycle timing. Replace timing luck with synchronization and bounded completion.

Done: tests assert accepted operation counts, order where promised, bytes on disk, immutable snapshots, error propagation, reuse and shutdown. Inject permission/I/O failures and prove no false successful completion; avoid sleeps as the only synchronization.

### [ ] F04 — Extend model/serialization regression coverage

**P1 · Medium · Confirmed gap · M**

Extend existing TestTrials, TestSessionBuilding, TestEvents, TestSettings, TestDataTable, TestJSON, TestBuildingFromCSV and TestLocale with invalid/empty boundaries, event ordering, settings inheritance and aliasing, lossless CSV cases, JSON numeric/null/Unicode behavior, malformed input, blocked CSV ordering and repeated lifecycle transitions.

Done: each corrected C/G issue has a behavior-level regression case; output golden fixtures protect column names/order/precision and legacy readers; assertions check public contracts rather than internal implementation details.

### [ ] F05 — Add lifecycle and editor integration regressions

**P1 · High · Likely · L · Depends B03, C03, C10**

Evidence: a PlayMode assembly now covers UI-free session lifecycle and native runs pass its smoke case; Editor tests cover package metadata, FileSaver output and SessionLogger lifecycle. Add PlayMode coverage for tracker update modes, session destruction, scene transitions, UI startup, repeated sessions and reload options, plus focused editor checks for drawers and wizard target settings.

Done: tests restore project/global settings they alter; no scene or asset changes leak from tests; a representative old prefab and UnityEvent configuration survives migration.

### [ ] F06 — Separate remote contract tests from live service tests

**P1 · High · Confirmed gap · M · Depends C08–C09**

Evidence: Assets/Testing contains AWS scenes/manual experiments rather than isolated transport tests. Fake HTTP/AWS outcomes for PR CI; create opt-in live smoke tests with explicit resource ownership, limits and cleanup. Test the Python server independently.

Done: contributors need no cloud account to run core tests; live tests cannot run accidentally on forks or write real participant records; browser callbacks, retry and malformed-response cases are covered.

### [ ] F07 — Add package install/upgrade and native/browser acceptance tests

**P1 · High · Confirmed gap · L · Depends D06–D07, E02**

Execute the accompanying validation matrix against local/Git/archive installs, all samples, legacy migration, supported native plugins and browser features. Automate repeatable cases and record manual/device evidence for the rest.

Done: every advertised combination has dated evidence for the released artifact; failures and excluded combinations are visible. An untested matrix cell cannot be called supported merely because editor tests pass.

## G. Core API and maintainability

### [ ] G01 — Encapsulate mutable session/block/trial state

**P2 · Medium · Confirmed design risk · L · Depends A04, F04**

Evidence: Session.blocks/currentTrialNum/currentBlockNum/instance, Block.trials, Trial.block/status/result are public mutable fields. Most existing reference properties already have private/protected setters; `FormElementEntry.element` now has an internal setter so UXF owns generated UI references. Inventory external mutation before removing access.

Done: read-only views plus validated create/add/reorder/remove/transition operations protect ownership and numbering; intentional result editing remains possible. Preserve serialized field data/UnityEvents, provide deprecations or a major-version migration, and update examples/tests/docs together.

### [ ] G02 — Introduce narrow service interfaces where they enable isolation

**P2 · Medium · Decision · L · Depends C01, D02**

Evidence: IExperimentUnit/ISettingsContainer/IExperimentBuilder/IDataAssociatable already exist, but data handling, time and platform file access remain tied to concrete MonoBehaviours. Evaluate interfaces for storage/transport, clock and read-only session access; retain Unity-facing components as adapters.

Done: core tests can use non-Unity fakes for a concrete use case; interface scope follows needed behavior; serialized component references and UnityEvents remain practical. Avoid creating an interface for every class with no consumer.

### [ ] G03 — Define Settings and results ownership/snapshot semantics

**P2 · Medium · Confirmed design risk · M**

Evidence: Settings.baseDict has a private setter but exposes a mutable dictionary; its constructor retains caller dictionaries; SetParent accepts arbitrary parent chains. Trial.result is replaceable. Define shared versus copied nested values, valid parent chains and mutation after save/end.

Done: read-only accessors/snapshot APIs are explicit, invalid cycles fail clearly, defaults/typed conversions behave consistently, and nested mutable data cannot change an already accepted save unexpectedly. Preserve supported configuration workflows.

### [ ] G04 — Decouple path/format/transport concerns and unify contracts

**P2 · Medium · Confirmed design risk · M · Depends C04–C08**

Evidence: handlers duplicate filename/session-path logic and choose different folder mappings; Session and Trial duplicate save dispatch. Extract narrowly scoped shared policy where contracts are truly common, including overwrite, formatting and location return values.

Done: parameterized contract tests cover all handlers and data types; expected legacy path differences are documented rather than accidentally normalized. Platform-specific operations remain behind adapters.

### [ ] G05 — Define optional UI and capability boundaries

**P2 · Medium · Decision · M · Depends B11, D03**

Evidence: Session API supports manual startup, but build checks and wizard assume bundled UI/template/handlers. Replace scattered platform allow/deny knowledge with a tested capability description and make UI/remote/native features independently usable where feasible.

Done: a minimal UI-free experiment can build/save; diagnostics distinguish unsupported from unverified; core does not require optional service configuration or globally rewrite host settings.

### [ ] G06 — Profile allocation hotspots before optimization

**P3 · Low pending measurement · Likely · M**

Evidence: Trial.number materializes Session.Trials.ToList; table Headers creates arrays, AddCompleteRow performs repeated key matching, tracker rows and SaveResults allocate. Benchmark large trial sets/high-rate capture and long logs before caching or redesigning.

Done: representative baseline and after measurements show benefit without stale numbering after trial edits, changed event order or data loss. No speculative collection rewrite is required to complete the Unity upgrade.

### [ ] G07 — Remove stale APIs and confusing names with migration discipline

**P3 · Low · Confirmed · M · Depends A04**

Evidence: LocalFileDataHander and ExportUFXPackage typos, obsolete Settings indexer, unused-looking DynamoDBCreationQuery, old FileIOManager documentation and mixed public naming. Search actual references and downstream API use before removal/renaming.

Done: obsolete internals are removed; public/serialized types use aliases, migration support or a documented breaking release. Format-only changes are separate from behavioral fixes to keep review tractable.

## H. Documentation and wiki

### [ ] H01 — Rewrite installation and support entry points

**P1 · High · Confirmed · M · Depends A02, D06**

Evidence: README/Get-started/Compatibility describe old editor versions and asset-package import. Add verified Package Manager instructions, supported versions/platforms, sample import, native/input dependencies, upgrade/rollback/uninstall and known limitations.

Done: a new user follows instructions in a clean supported editor without undocumented fixes; version claims match CI/manual evidence; legacy instructions are clearly versioned.

### [ ] H02 — Refresh every operational wiki area against the new behavior

**P1 · Medium · Confirmed · L**

Review these checked-in pages as a checklist: WebGL-build-setup, Remote-data-collection, AWS-DynamoDB-setup, HTTP-POST-setup, Custom-Data-Handler, Data-collection, Collect-Custom-Data, Logging-system, Tracker-system, Built-in-UI, Datapoints-in-the-UI, Settings-system, Session-generation, Add-trials-during-session, Specify-trials-with-CSV-file, Events, Multi-scene-experiments, Examples, Common-issues and Unit-tests.

Done: paths, menu names, screenshots, code, save completion, input/storage constraints and output examples match the release. Tutorials are exercised; external tutorial content is flagged for owner review rather than assumed updated by local edits.

### [ ] H03 — Repair and automate API-reference generation

**P2 · Medium · Confirmed · M · Depends D02**

Evidence: docs/xml2md.py ignores its documented output-directory argument, writes hardcoded wiki/Generated paths, assumes summary elements exist and groups types by only the second namespace segment. docs/WIKI-README.md relies on manual IDE project changes; generated pages include stale FileIOManager/SessionDebugger names.

Done: generator accepts output paths, handles missing/multiline XML/nested namespaces/overloads, uses a pinned Python environment and current assembly XML, and removes obsolete generated pages safely. Run from a clean checkout; generated reference matches actual API. Add focused generator fixtures.

### [ ] H04 — Establish wiki/website source and publishing ownership

**P2 · Medium · Confirmed gap · M**

Evidence: checked-in docs/wiki plus links to a separate GitHub wiki and docs/index.html; instructions describe manual publication. Decide canonical source, review flow, versioned docs and synchronization mechanics; audit _Sidebar/_Footer/Home/Programming-reference and website links.

Done: releases link to matching docs; link checking understands GitHub wiki links; publishing cannot overwrite unreviewed external edits. No remote wiki content was changed or assumed identical during this local audit.

### [ ] H05 — Write contributor, test and release guides

**P1 · Medium · Confirmed gap · M**

Evidence: repository/package contributor guides and the package development workflow now document editor/modules, local package setup, assembly boundaries, isolated test commands, artifact creation, CI/fork behavior, release checklist and troubleshooting. `docs/wiki/Unit-tests.md` now points at Unity 6 and the licensed workflow; keep this task open until a fresh contributor can reproduce the full release checklist from a clean checkout.

Done: a contributor reproduces PR checks and builds a reviewable package locally; secrets and live service setup are optional and separate from normal tests.

### [ ] H06 — Publish API/data migration notes and changelog

**P1 · High · Confirmed gap · M · Depends A04, G01**

Document changed fields/properties/interfaces, inspector removal, package paths, settings handling, timing/CSV/binary output, renamed types and dependency changes. Include old-to-new examples and analysis-script adjustments.

Done: every breaking task links to a migration entry and regression evidence; changelog/version agree; unchanged compatibility is not asserted without tests.

### [ ] H07 — Document remote collection deployment and data handling accurately

**P1 · High · Likely · M · Depends C06, C08–C09**

Evidence: Flask example uses demonstration authentication and broad CORS; browser template loads cloud SDK; README makes broad remote-collection claims. Explain client-visible configuration, server-side access control, HTTPS deployment, restricted origins, retention/logging and upload confirmation. Audit example IAM/setup guidance against the selected integration before recommending it.

Done: sample is clearly scoped, does not invite production use of demo credentials, and documents failures/offline/browser-close limitations. No secrets or participant identifiers are added to docs, fixtures or CI artifacts.

### [ ] H08 — Update secondary educational/analysis pages and visual assets

**P2 · Low · Confirmed review need · M**

Review Concepts, Background, Programming-ideas, Useful-code-snippets, Factorial-design, Processing-DynamoDB-CSVs, Example-R-script-for-processing-UXF-data, Useful-asset-links, Unity-tutorial-links, Videos and UXF-2.0. Keep historical information labeled, fix API references, verify external links and regenerate representative output/screenshots after final behavior stabilizes.

Done: examples use supported APIs/data schema; stale links and duplicate pages have deliberate redirects/archive labels; no historical compatibility statement is mistaken for the current release guarantee.

## I. Samples, repository hygiene and release finish

### [ ] I01 — Run and repair every example as a standalone consumer sample

**P1 · High · Likely · L · Depends D04, F07**

Evidence: shuttle automatic/manual/CSV variants, movement, multiscene and Corsi samples; current enabled scenes mix shuttle startup with two other sample scenes. Verify each intended startup/build scene set and input/physics behavior, including existing velocity/find-object version branches.

Done: sample checklist includes trial counts/settings, expected events, UI/keyboard/pointer behavior, scene transitions and actual saved data; no reliance on the development project's shared build settings. Visual review covers upgraded materials/sprites and selected render-pipeline support.

### [ ] I02 — Audit serialized references, GUIDs and upgrade-generated assets

**P1 · High · Likely · M**

Evidence: substantial sprite/material metadata changes and new generated CSV sample directory in the local upgrade. Scan missing/duplicate GUIDs, missing scripts, persistent UnityEvent methods and prefab references, then verify suspect assets in Unity. Account for built-in/package references before calling an unresolved GUID broken.

Done: imported and migrated samples/prefabs have no unexplained missing reference; baseline and package relocation preserve intentional GUIDs. No broad reserialization without a reviewed reason.

### [ ] I03 — Stop tracking generated logs and development artifacts

**P2 · Medium · Confirmed · S**

Evidence: Logs/Packages-Update.log tracked, many untracked import logs, .gitignore lacks Logs, bin/YourApi.XML and docs/generated-docs.xml exist. Define generated-versus-source policy, improve ignores and remove tracked generated files only in a reviewed cleanup commit.

Done: normal editor import/test/build/docs generation leaves no unexpected tracked changes; useful committed documentation fixtures are retained intentionally; no unrelated user file is deleted.

### [ ] I04 — Trim development dependencies and document vendor provenance

**P2 · Medium · Likely · M**

Evidence: manifest includes Multiplayer Center/navigation/tilemaps/timeline/editor services with no established requirement from core; vendored code/binaries lack a consolidated version/update inventory. Trace actual usage, remove demonstrably unnecessary dev dependencies, and record retained library origin/version/license/patches.

Done: clean resolution and examples still work; provider dependencies are not confused with multiplayer gameplay; third-party modifications are maintainable and the package lock is reproducible.

### [ ] I05 — Turn the Python upload example into a reproducible tested example

**P2 · Medium · Confirmed gap · M · Depends C06, H07**

Evidence: webserver_example now has pinned `requirements.txt`, environment-based credentials/output configuration, containment handling and client tests; the tests are skipped until those dependencies are installed and deployment boundaries still need documentation/validation. Keep the explicit supported environment/dependency specification and test client coverage.

Done: a clean environment runs the example/tests; malformed fields, authentication failure, containment, overwrite and I/O errors have tested responses; logs do not expose participant data or credentials.

### [ ] I06 — Perform release-candidate verification and close evidence gaps

**P1 · High · Decision gate · M · Depends all selected P1 tasks**

Execute [validation matrix](Unity6-validation-matrix.md) against the final artifact and consumer upgrade. Record test/build counts/results, hashes, platform exclusions, known issues and resolved task IDs. Use targeted Unity bug-investigation work for reproduced failures and build-validation work for the release gate.

Done: no untriaged compile failures, unacknowledged saving failures or unsupported advertised installation path; release notes accurately distinguish tested support from deferred work. No profiler/device/browser result is inferred from static code inspection.

## J. Agent understanding in users' installed projects

### [ ] J01 — Ship a concise, versioned agent entry point with the package

**P2 · Medium · Decision · M · Depends D01, D08, H01**

User-requested addition: agents helping package users need consumer-facing context, not just this development repository's audit. Provide a clearly linked, plain-Markdown entry point in the shipped package, reachable from its README. Summarize package/version identity, Session → Block → Trial relationships, settings inheritance, lifecycle/event order, data handlers, optional UI, extension points and supported configurations; link to detailed local docs and source rather than duplicating everything. Keep repository-maintainer instructions separate.

Done: guidance is present in local/Git/archive installs and readable offline, uses package-relative links, and accurately describes the installed version. An agent can identify the public API and a minimal valid experiment without needing this repository's history, remote wiki or the user's entire project. The entry point is short enough for routine context loading.

### [ ] J02 — Make guidance discoverable without assuming agent-specific auto-loading

**P2 · Medium · Decision · M · Depends J01, D05**

Design explicit discovery: README link, documentation index and a short user-facing instruction for directing an agent to the installed package's guide. Evaluate an optional package-scoped AGENTS.md or tool-specific adapter only after checking the relevant agent's discovery/scope behavior. A file in a package cache must not be assumed automatically loaded when an agent edits the user's Assets scripts.

Done: a fresh consumer can locate the correct guide/version through the documented route; any claimed automatic discovery is tested for the named tool. Installation does not overwrite or inject instructions into the host project's root guidance. Package guidance respects host-project conventions and directs edits to user-owned scripts or supported extension points, with explicit embedding/forking instructions when package modification is intentional.

### [ ] J03 — Publish executable recipes and explicit API contracts for common agent tasks

**P2 · Medium · Decision · M · Depends J01, H03, F04**

Build a small indexed recipe set using tested samples: create trials/blocks, read typed inherited settings, record results, implement a tracker/data handler, start without UI, configure saving and coordinate scene changes. Explain preconditions, valid call order, ownership/mutation rules, thread/platform restrictions, output schema and save-completion semantics. Mark stable APIs versus internals and deprecated/version-specific alternatives.

Done: complete recipes compile against the released package and link to their tests/source. Typical errors have actionable explanations; agents can extend an experiment without guessing method names, editing internals or copying outdated wiki snippets. Migration recipes are tied to specific versions and updated with API changes.

### [ ] J04 — Provide a compact documentation map and safe diagnostic context

**P2 · Medium · Decision · M · Depends J01, H03, E06**

Add a maintained map from common questions to relevant namespaces/types, source files, recipes and troubleshooting pages. Prefer existing package metadata and a simple index over an unproven custom agent protocol. Specify a copyable diagnostic checklist or optional exporter for package/Unity version, selected platform/input/backend, enabled handlers and relevant sanitized errors, so an agent can distinguish package limitations from host-project configuration.

Done: links and symbols are checked during release; runtime/editor/sample boundaries are obvious. Diagnostics exclude credentials, participant identifiers/results and unnecessary local paths, and users can review any generated report. The guide explains what evidence is needed before declaring a save, build or experiment successful.

### [ ] J05 — Evaluate agent usability from a clean installed-package consumer

**P2 · Medium · Decision · M · Depends J01–J04, D06, F07**

Create repeatable evaluation scenarios using only a fresh consumer and the installed artifact: locate documentation, build a minimal experiment, add a custom tracker, diagnose a settings/startup error, select a compatible handler and migrate one changed API. Include the documented explicit-discovery route; separately test any tool-specific automatic discovery. Keep development-repository context out of the evaluation.

Done: record tool/model/version, supplied context, generated changes, compilation/tests and task outcomes. Score API accuracy, lifecycle/data correctness, discovery effort and unnecessary package edits; establish an initial baseline before choosing thresholds. Use failures to improve docs/recipes, keep results reviewable, and run a bounded reevaluation when public APIs, package layout or guidance change. Do not require a paid agent service for ordinary contributor tests.

## Evidence and audit limitations

Positive foundations: focused model/data tests already exist; core concepts have existing interfaces; settings inheritance and handler extension points are established; source-controlled examples and wiki content provide migration fixtures. These can be extended instead of replacing the framework wholesale.

This audit inspected configuration, principal runtime/editor/data paths, test sources, browser plugins/templates, sample entry points, docs tooling and server example. It did not execute all samples, resolve every scene GUID, decompile native DLLs, inspect remote wiki revisions, exercise AWS, profile capture or run a fresh build/test suite. Those gaps are explicit tasks above. The machine log's old Rider path is an environment issue, not evidence that UXF itself fails compilation.

Official references consulted: [Unity package layout](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-layout.html), [Git package dependencies](https://docs.unity3d.com/6000.3/Documentation/Manual/upm-git.html), [browser interop migration](https://docs.unity3d.com/6000.3/Documentation/Manual/web-interacting-browser-deprecated.html), and [conditional compilation](https://docs.unity3d.com/6000.3/Documentation/Manual/platform-dependent-compilation.html). Actual local compiler defines were checked to avoid mistaking `UNITY_6000` for an invalid symbol.

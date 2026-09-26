# Unity 6 release validation matrix

Companion to [task backlog](Unity6-upgrade-backlog.md). **Player/device and tagged-install results are still pending** as of 2026-09-26. The embedded package and clean consumer have now completed native Unity 6 EditMode and PlayMode runs; the package archive passes manifest/contents checks. Static audit and editor tests are not a release pass for browser, device or native-plugin behavior. Select support scope in A02 before expanding CI; do not claim every listed platform is supported.

## Required evidence format

For each run record source commit, package version/hash/install method, editor version, host/target/CPU/backend, render/input configuration, command or manual steps, expected/actual result, logs/test XML/artifacts, and related task ID. Use PASS, FAIL, BLOCKED or NOT IN SUPPORT SCOPE. Include failure reasons and any manual coverage; zero discovered tests is a failure, not a passing suite.

## Installation and distribution

| Scenario | Acceptance | Tasks |
| --- | --- | --- |
| Fresh local package consumer | Resolves declared dependencies; runtime/editor compile; no development-project assets required | D01–D03 |
| Git URL pinned to release tag | Installs using documented URL/path; updates and rolls back predictably | D06, E04 |
| Downloaded UPM archive | Installs from the actual CI artifact; metadata and checksum match release | E03–E04 |
| Minimal UI-free consumer | Core session builds and records results without UI/cloud/native configuration | B11, G05 |
| Legacy Assets import to UPM | GUIDs, serialized fields, prefabs and persistent UnityEvents survive; duplicates diagnosed | D07, I02 |
| Reinstall/uninstall | User-owned settings/output/sample modifications preserved; removed package leaves explained sample dependencies | D05–D07 |
| Each sample alone/all samples together | Shared assets resolve without duplicate GUIDs; necessary data/build scenes explicitly installed | D04, I01 |
| Package contents | Reviewed allowlist, licenses, no credentials/participant data/generated logs/unwanted test runtime | D08, E03 |
| Agent documentation in installed package | Version-matched offline entry point, working local links and tested recipes; explicit discovery works without development-repository context | J01–J04 |
| Agent-assisted consumer tasks | Minimal experiment, tracker, diagnosis and migration compile/test correctly; host guidance preserved; no unintended package-cache edits | J02, J05 |

Current local evidence: the rebuilt CI-style package archive contains 526 entries, including `Samples~/README.md`, sample StreamingAssets fixtures and fresh-GUID Unity 6 WebGL templates; the package manifest declares UGUI 2.0.0 and UnityWebRequest 1.0.0. The `ci/consumer` fixture exercises `file:../../../Packages/com.immersivecognition.uxf`. Native Unity 6 batchmode runs passed 58/58 package EditMode tests and 1/1 package PlayMode test; the clean consumer passed 59/59 EditMode and 2/2 PlayMode tests, including package metadata and UI-free runtime smoke coverage. The embedded package and root Assets/package GUID scan pass; generated `Assets/Samples` copies are validated for meta shape but excluded from cross-root duplicate checks because Package Manager preserves sample GUIDs on import. Git-tag/archive installation, player builds, device/browser execution and GitHub-hosted licensed jobs remain open.

The current development-editor Console import diagnostics were triaged through `Editor.log`: the malformed `UXFEditorBase.cs.meta`, duplicate Test Runner references and orphaned empty-folder metas from the Samples move are fixed. Remaining Rider-path, Unity Licensing Client and DNS messages are host-environment blockers and are not package compilation failures.

## Automated suites

| Area | Minimum cases | Tasks |
| --- | --- | --- |
| Test discovery | Expected fixtures/cases; EditMode and PlayMode; package testables; no production NUnit dependency | F01 |
| Fixture isolation | Unique temp output; repeat/reorder; culture/static/event restoration; forced failure teardown | F02 |
| Session/trial | Empty/invalid indices, repeated Begin/End, reentrancy, event exceptions, failed handler setup, teardown exactly once | C03, F04–F05 |
| Ownership/settings | Parent inheritance, missing/type mismatch/defaults, cycle rejection, nested aliasing, read-only views and intentional mutations | G01–G03 |
| CSV/JSON | Quotes, separators, CR/LF, Unicode, null, numerical precision, multiple cultures, malformed/empty/duplicate headers | C04, F04 |
| Saving | Every data type/handler; content and location correctness; arbitrary bytes; overwrite/path containment; failed disk write | C01, C05–C07 |
| Worker lifecycle | No setup/double cleanup/restart; enqueue-close races; bounded completion; worker error; accepted writes preserved | C02, F03 |
| HTTP | Mock success/error/timeout/disconnect; disposal; retry/idempotency; pending-save UI and cancellation | C01, C08 |
| Cloud bridge | Mock callback success/failure/duplicate/timeout; partial batches; repeated initialization; map/listener cleanup | C09, F06 |
| Editor UX | List editing/order/undo/redo/prefab overrides/multi-select; event bindings; correct wizard build target; UI-free validation | B01–B04, B11 |
| Reload/lifecycle | Normal and supported fast-play options; repeat play/stop; additive scenes; destroyed session/logger/tracker; worker queue restart | C10, F03, F05 |
| Documentation generator | Missing XML summaries, namespace nesting, overloads, output path, deterministic regeneration, stale-page cleanup | H03, E06 |
| Python server | Authentication, missing fields, traversal/rooted paths, overwrite policy, I/O failure, lossless payload | C06, I05 |

## Player and device matrix

| Candidate configuration | Required behavior before advertising support | Current evidence |
| --- | --- | --- |
| Windows x64 Mono | Startup, UI, tracking, local output, dialog open/cancel/save, session stop/quit | Win32 P/Invoke adapter passes source-level compilation; runtime dialog flow not exercised |
| Windows x64 IL2CPP | Same plus native/managed plugin and stripping/AOT compatibility | Not built |
| Web player, selected desktop browsers | Template loading, input, settings, Unicode interop, download/clipboard, fake HTTP/cloud, upload completion/errors | Interop bridge migrated to `UTF8ToString`; not built/run |
| Web hosting variants | Actual selected compression/MIME/decompression behavior; HTTPS/CORS; CDN failure; page close with pending writes | Not exercised |
| Android device / intended VR device | Packaged settings/CSV access, persistent output, lifecycle suspend/resume, selected input and no unnecessary permission dependency | Desktop filesystem assumptions found; not built/run |
| macOS Intel/Apple Silicon | Native bundle architecture, dialog behavior, paths and local output | Bundle present; architecture/runtime unverified |
| Linux x64 | Native dependency availability, file dialogs, path conversion and output | Native wrapper found; matching binary not found |
| iOS if retained | AOT/stripping, supported handler/output/input paths and device lifecycle | Wiki claim only; no device validation |
| Built-in rendering | All samples/UI/materials correct after upgrade | Configured, no visual run |
| URP/HDRP if advertised | Core behavior plus explicit sample-material policy; no pink/invisible required content | No active pipeline dependency; not validated |
| Legacy input / Both / Input System only | Pointer/keyboard UI, trackers, fallback and scene-owned EventSystems and additive scenes work as documented | Package input bridge and runtime EventSystem conversion implemented; local Input System-only run pending because Unity Licensing Client timed out; CI clean-consumer matrix now covers legacy and Input System-only EditMode/PlayMode |

Live cloud tests must be opt-in, use disposable resources and synthetic data, and record cleanup. Browser-beforeunload cannot be treated as guaranteed durable upload; test normal completion and document unavoidable close/offline limitations.

## Sample checklist

| Sample | Check |
| --- | --- |
| 1a SpaceShuttle | UI start, generated trials, events and saved results |
| 1b ManualStart | No UI dependency; manual setup and save/cleanup |
| 1c BuildFromCSV | Settings/CSV installation, valid/invalid table feedback and blocked specification behavior |
| 2 MoveToTarget | Pointer tracking, sprites, target/task sequence, tracker output |
| 3 MultiScene Football/RT | Complete scene list, scene load ordering, preserved Session, task lookup, response timing |
| 4 CorsiBlock | Click/input, stimulus sequence, feedback and result rows |
| AWS manual test scenes | Explicit opt-in configuration; never bundled as silently active cloud tests |

## Performance and final release

Measure representative long sessions, high-rate tracking and many trials: frame cost, allocations, retained buffers, output size, flush latency and failure responsiveness. Record workloads and acceptable thresholds before optimizing. Compare output golden fixtures and existing analysis scripts after timing/CSV/API changes.

Final release requires passing supported cells, documented exclusions, resolved or explicitly accepted remaining risks, updated wiki/README/changelog, installable verified artifact and an exercised legacy migration. Do not check these boxes based only on package archive creation.

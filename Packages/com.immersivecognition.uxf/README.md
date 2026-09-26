# UXF - Unity Experiment Framework

This is the UXF package. It provides `Session`, `Block`, `Trial`, cascading `Settings`, trackers, optional UI and pluggable data handlers for Unity experiments.

## Install with Unity Package Manager

For the current development repository, add this Git URL in **Window > Package Manager > + > Add package from git URL**:

```text
https://github.com/immersivecognition/unity-experiment-framework.git?path=Packages/com.immersivecognition.uxf#<release-tag>
```

Use a release tag instead of `master` for a reproducible project. The package requires Unity 6000.3 or newer for this release line and declares UGUI plus UnityWebRequest as package dependencies. The package is hosted inside the development repository; the `?path=Packages/com.immersivecognition.uxf` suffix is required.

The repository's own development project embeds this package under `Packages/com.immersivecognition.uxf`, so it appears under Package Manager's **In Project** list and exposes the **UXF Examples** sample. A separate consumer can install the same package through the Git URL above or a local `file:` dependency, as exercised by `ci/consumer`.

After installation, core runtime scripts are under `Scripts`; editor tooling is under `Scripts/Etc/Editor`; browser/native integrations are under `Plugins`. Import the `UXF Examples` entry from Package Manager's **Samples** tab when you want the executable recipes; the package keeps those files under `Samples~` so they are not compiled into the core package automatically. Read the sample README after importing: several scenes need the sample `StreamingAssets` files copied into the consumer project's root `Assets/StreamingAssets` folder.

### Importing UXF Examples

1. Open **Window > Package Manager** and select **In Project**.
2. Select **UXF - Unity Experiment Framework** and open its **Samples** tab.
3. Click **Import** beside **UXF Examples**. Unity copies the sample into `Assets/Samples/`.
4. Open the scene you want from that imported folder. For the automatic, CSV and multi-scene examples, copy the sample's `StreamingAssets` fixtures into the project's root `Assets/StreamingAssets` folder and merge with existing files.
5. For WebGL, copy `Assets/Samples/<package>/3.0.0/UXF Examples/WebGLTemplates/UXF WebGL 2020` into the project's root `Assets/WebGLTemplates/` folder, then select **UXF WebGL 2020** in WebGL Player Settings. Unity does not discover custom templates from `Assets/Samples/` directly.

The `[UXF_Rig]` Session component is enabled by default and only runs its lifecycle in play mode. If a rig imported from an older UXF version still shows Session disabled, enable it and apply the prefab override (or re-import the sample). UXF's editor validation is side-effect-free, so opening or inspecting the rig should not dirty the scene.

If UXF is not listed after changing the package source or adding a Git URL, let Package Manager finish resolving packages or restart the Unity editor once. Do not open scenes directly from `Samples~`; that folder is package source and remains hidden by design.

## Agent and contributor context

Coding agents should read [`AGENTS.md`](AGENTS.md) before changing package code, then use the tested [`AGENT-RECIPES.md`](AGENT-RECIPES.md) for common tasks. The guide describes the public architecture, safe extension points and validation expectations. The development-project context is available in the [repository documentation](https://github.com/immersivecognition/unity-experiment-framework/blob/master/docs/AI/UnityProjectContext.md); it is not a substitute for the installed package version's API documentation.

The main concepts are:

1. A `Session` owns ordered `Block` objects.
2. A `Block` owns ordered `Trial` objects.
3. Settings cascade from session to block to trial.
4. A trial records results and tracker data through configured `DataHandler` components.
5. UI, WebGL/AWS, HTTP and native file dialogs are optional integrations; do not add them to a core-only consumer without checking their platform requirements. Use `SFB.StandaloneFileBrowser` as the dialog API. Windows dialogs use the OS common-dialog APIs through P/Invoke; macOS/Linux support still depends on the bundled native backends and requires platform validation.

Build validation is session-based: UI-free and manually started `Session` components are checked for handler and platform compatibility even when no `UIController` is present. In batchmode, an unsupported configuration fails the build with a `BuildFailedException` so CI does not wait for an editor dialog.

On Android, `FileSaver` must use `PersistentDataPath`. This is app-private storage and does not require Unity's legacy External (SDCard) permission. Validate any custom storage provider separately on the target device.

Tracker tables are serialized through a worker queue when a trial ends; `Session.End()` waits for queued tracker writes before producing the session results table. Keep worker actions free of Unity API calls, and treat save completion/failure behavior as part of the data contract.

Start with the checked-in [wiki](https://github.com/immersivecognition/unity-experiment-framework/wiki) and generated API reference in the repository. Use the examples as executable recipes and keep package edits separate from user-owned experiment scripts.

## Development project

The repository also contains the Unity development project, tests, documentation source and release tooling. See the repository [README](https://github.com/immersivecognition/unity-experiment-framework/blob/master/README.md) and [Unity 6 upgrade backlog](https://github.com/immersivecognition/unity-experiment-framework/blob/master/docs/maintenance/Unity6-upgrade-backlog.md) for upgrade and release work. Do not edit files inside a consumer project's PackageCache; make changes here and install a tag or local package during development.

For repository contributors, the authoritative source is this embedded package at `Packages/com.immersivecognition.uxf`; the root development project consumes it directly. Use Package Manager's **In Project** view and import **UXF Examples** from **Samples** when testing examples. The repository's [package development workflow](https://github.com/immersivecognition/unity-experiment-framework/blob/master/docs/maintenance/Package-development-workflow.md) describes the clean consumer, CI archive and legacy migration checks.

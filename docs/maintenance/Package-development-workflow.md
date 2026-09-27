# UXF package development workflow

The repository is both the UXF package source and a Unity development project. The authoritative package root is `Packages/com.immersivecognition.uxf`. Keeping the source in `Packages` makes Unity treat it as an embedded package, so the development project consumes the same files that are archived and published. Do not create a second copy under `Assets/UXF`.

## Daily package work

1. Open the repository root in Unity 6000.3.25f1 (or the supported editor for the release line).
2. Use **Window > Package Manager > In Project** to select UXF. The package's **Samples** tab contains the `UXF Examples` import.
3. Edit runtime, editor, tests and package documentation under `Packages/com.immersivecognition.uxf`. Keep user-owned development fixtures in the root `Assets` folder.
4. Import samples into `Assets/Samples/` when working on them. That folder is a generated local copy and is ignored by Git. Copy their documented `StreamingAssets` fixtures into the root `Assets/StreamingAssets` only when a scene needs them; never edit the package's `Samples~` source as if it were an imported sample.
5. Run the affected EditMode and PlayMode tests from the Test Runner. The development manifest enables the package in its `testables` array, and `ci/consumer` does the same for clean-consumer tests. Compile a clean consumer when changing package metadata, assembly definitions or installation behavior.

Unity's package manager should not be pointed at `Assets/UXF`: Unity imports files under `Assets` as project assets and rejects that location as a local package source. A separate consumer can use the repository Git URL with `?path=Packages/com.immersivecognition.uxf#<release-tag>` or the `file:../../../Packages/com.immersivecognition.uxf` fixture path used by CI.

## What is released

`.github/workflows/package.yml` validates `package.json`, package metadata, assembly definitions, samples, Unity `.meta` files and removal of retired legacy sources. It then creates a versioned UPM ZIP from the package root. The archive must contain `package.json`, package-local guidance, the changelog, license and `Samples~`; it must not include the development project's `Assets`, `Library`, `ProjectSettings` or generated output.

The Git URL is the preferred installation path for released tags. The old `.unitypackage` exporter remains only for legacy consumers. Do not install both the UPM package and a legacy `Assets/UXF` copy in one project; migrate serialized references and preserve `.meta` GUIDs before removing the old copy. The licensed clean-consumer CI job downloads the exact archive emitted by the package job, unpacks it into a temporary package root and resolves that artifact through Package Manager before running its smoke and package tests.

Pushing a tag whose name is `v<package.version>` (or `<package.version>`) runs package validation, verifies that the tag matches `package.json`, and creates the GitHub release with the versioned ZIP and a `.sha256` checksum only after the licensed Unity and clean-consumer test jobs pass. A push to `master` can also create the versioned release when those jobs pass and the version has no release tag. Pull requests and ordinary branch pushes upload the archive as a CI artifact without publishing a release. The licensed jobs require the repository `UNITY_CI_ENABLED` variable and Unity license secrets; when they are skipped or fail, publication is skipped.

Those licensed jobs record test XML from the test runner output path and fail when no tests are discovered. Code Coverage is disabled by default for this Unity 6 workflow; enable it only after a runner validates the coverage package and PlayMode stability for the supported editor.

### Live Unity Editor control

Unity 6 projects can expose the open Editor to the Unity CLI through the Unity Pipeline package. On a development machine, install the CLI, sign in, and install the package from the project directory:

```powershell
unity auth login
unity pipeline install --project-path .
unity pipeline list
unity command --project-path .
```

The package is experimental (`com.unity.pipeline` `0.8.0-exp.1`), so this project enables pre-release packages in `ProjectSettings/PackageManagerSettings.asset`. Restart the Editor after installation if `pipeline list` reports the package but the server is unreachable. Use `unity command recompile --project-path .` and poll `unity command recompile_status --project-path .` to check live compilation. Test discovery and live control use `unity command list_tests --project-path . --mode editor` and `unity command run_tests --project-path . --mode editor --async_tests true`, followed by `unity command test_status --project-path .`. For deterministic CI evidence, use Unity's native runner from a clean project process: `Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform editmode -logFile Temp/editmode.log` and repeat with `-testPlatform playmode`; the runner writes its XML to the platform's standard test-results location. If an interrupted Pipeline run leaves `Temp/pipeline_test_request.json`, close the affected Editor before removing that request and retrying. The CLI and Pipeline package are development tooling; they are not included in the distributable UXF package archive.

## Package-local guidance

Installed-package agents should start with [`Packages/com.immersivecognition.uxf/README.md`](../../Packages/com.immersivecognition.uxf/README.md), [`AGENTS.md`](../../Packages/com.immersivecognition.uxf/AGENTS.md) and [`AGENT-RECIPES.md`](../../Packages/com.immersivecognition.uxf/AGENT-RECIPES.md). These files are included in the archive so an agent can discover the package boundaries, extension points and validation commands without the development repository.

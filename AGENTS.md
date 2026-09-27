# Agent guide for the UXF development repository

Read [`docs/AI/UnityProjectContext.md`](docs/AI/UnityProjectContext.md) for the current architecture and [`docs/maintenance/Unity6-upgrade-backlog.md`](docs/maintenance/Unity6-upgrade-backlog.md) for the migration work inventory. The development editor baseline is Unity 6000.3.25f1.

## Repository boundaries

- `Packages/com.immersivecognition.uxf` is the embedded source package and contains its own installed-package guide in `Packages/com.immersivecognition.uxf/AGENTS.md`. To use examples, open Package Manager, select UXF under **In Project**, open **Samples**, and import **UXF Examples** into `Assets/Samples/` before opening a scene. WebGL consumers must copy the imported `WebGLTemplates/UXF WebGL 2020` folder into the project root `Assets/WebGLTemplates/` because Unity discovers custom templates there.
- Package runtime/editor/test boundaries are declared by the `UXF.Runtime`, `UXF.Editor`, `UXF.Form.Editor`, `UXF.HideIf.Editor`, `UXF.Tests.Editor` and `UXF.Tests.PlayMode` assembly definitions. Samples live under `Packages/com.immersivecognition.uxf/Samples~` and are imported through Package Manager; the sample README explains the root `StreamingAssets` fixtures.
- `docs/wiki` is a nested Git repository. Changes there need to be committed in that repository and then recorded by updating the parent submodule pointer.
- `Library`, `Temp`, `Logs`, `dist` and PackageCache output are generated or local state. Do not edit PackageCache files to fix package code.
- Preserve existing user changes in ProjectSettings, imported assets and examples; inspect the diff before reverting anything.

## Safe implementation rules

- Preserve Unity serialized field names, UnityEvent methods and assembly-qualified types. Search usages before changing public API.
- Keep UnityEditor APIs in editor folders and never call Unity APIs from the FileSaver worker thread.
- Treat CSV/JSON output paths, file names and handler return values as public data contracts. Add a focused regression test before changing them.
- Update the package README, relevant wiki page and changelog when public behavior or installation changes.

## Validation expectations

At minimum, compile the project in the target Unity editor and run the affected EditMode and PlayMode tests. For package work, validate a clean consumer using the Git URL documented in `Packages/com.immersivecognition.uxf/README.md`, then import the `UXF Examples` sample from Package Manager. The CI Unity matrix covers the development project and `ci/consumer`, and is enabled by the repository variable `UNITY_CI_ENABLED=true` plus Unity license secrets; package/archive checks run without credentials. Record platform, backend and test limitations in the validation matrix; editor compilation alone does not establish player support.

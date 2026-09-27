# UXF agent recipes

These recipes target the package version that contains this file. Read `README.md` and `AGENTS.md` first, then adapt user-owned scripts rather than editing the package cache.

## Start a UI-free session

```csharp
using UnityEngine;
using UXF;

public sealed class ExperimentBootstrap : MonoBehaviour
{
    [SerializeField] private Session session;

    private void Start()
    {
        Block block = session.CreateBlock(1);
        session.Begin("example", "participant-001", settings: Settings.empty);
        session.BeginNextTrial();
        session.CurrentTrial.result["score"] = 1;
        session.CurrentTrial.End();
        session.End();
    }
}
```

Create blocks before calling `BeginNextTrial`. `Session.Begin` does not create blocks or trials. Call `Session.End` once the final trial has ended; it waits for queued tracker-table writes before saving the session results table.

## Use cascading settings

```csharp
Settings settings = Settings.empty;
settings.SetValue("trial_count", 10);
session.Begin("example", "participant-001", settings: settings);

int count = session.settings.GetInt("trial_count", 0);
```

Block and trial settings override their parent values. Use typed `GetInt`, `GetFloat`, `GetBool`, `GetString`, `GetDict` or `GetObject` calls; avoid the obsolete settings indexer in new code.

## Record tracker and trial data

```csharp
session.CurrentTrial.result["response"] = responseValue;
tracker.StartRecording();
tracker.RecordRow();
tracker.StopRecording();
```

Tracker data is captured while the trial is active and serialized after the trial ends. Do not call Unity APIs from custom worker actions or from a `FileSaver` worker thread.

`Session.End()` waits for tracker-table work before writing `trial_results`. If code queues work through `Trial.ManageInWorker`, pair it with `Trial.WaitForTasks()` at a defined shutdown point; the worker can be restarted for a later session, but a task must not enqueue more work while shutdown is in progress.

## Add a custom data handler

Inherit `DataHandler`, implement every `Handle*` method, add the component to the Session's `dataHandlers` field, and implement `SetUp`/`CleanUp` when the transport has a lifecycle. Keep platform checks in `IsCompatibleWith`/`IsIncompatibleWith` under `UNITY_EDITOR`; never make the runtime assembly depend unconditionally on `UnityEditor`.

## Diagnose a build or save issue

Capture the Unity version, UXF package version, active build target, input backend, enabled data handlers, whether a `UIController` is present, the first error message and whether `Session.End` returned. Do not include credentials, participant identifiers, result rows or full local paths in a diagnostic report. Build validation checks UI-free Sessions as well as UI-driven Sessions; batchmode failures are `BuildFailedException` errors.

## Package boundaries

`UXF.Runtime` is player-facing. `UXF.Editor`, `UXF.Form.Editor` and `UXF.HideIf.Editor` are editor-only. `UXF.Tests.Editor` and `UXF.Tests.PlayMode` are test assemblies. To inspect an example, open **Window > Package Manager > In Project**, select UXF, choose **Samples**, and click **Import** beside `UXF Examples`; open scenes from the resulting `Assets/Samples/` folder. Copy the sample `StreamingAssets` fixtures into the root `Assets/StreamingAssets` when the selected scene requires them.

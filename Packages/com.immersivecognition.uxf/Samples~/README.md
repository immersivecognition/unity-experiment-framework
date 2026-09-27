# UXF Examples sample

Import this sample from **Window > Package Manager > In Project > UXF - Unity Experiment Framework > Samples > UXF Examples > Import**. Unity copies it into `Assets/Samples/`; open scenes from that imported folder rather than from the package's hidden `Samples~` source. The scenes and scripts are self-contained, but the automatic, CSV and multi-scene examples also read example settings from `Assets/StreamingAssets`.

UXF runtime scripts remain render-pipeline-agnostic and do not install URP. To use the URP examples in an existing Built-in project:

1. Install **Universal RP 17.3.0** from **Window > Package Manager > Unity Registry** (or use a URP project template).
2. In the Project window, create a `Settings` folder under `Assets`. Select it, then choose **Assets > Create > Rendering > URP Asset (with Universal Renderer)**. Unity creates the pipeline asset and its renderer data in that folder.
3. Activate the asset. In **Edit > Project Settings > Quality**, assign it to the quality levels you use. A quality-level assignment overrides **Project Settings > Graphics > Default Render Pipeline**. You can instead set the Graphics default and leave each Quality override empty; then that default is used for those levels. If an inactive-profile warning remains, check the active quality level for a different assigned asset.
4. The sample's Standard and fire-particle materials have been converted to URP shaders. Its procedural skyboxes still use Unity's built-in skybox shader, which URP supports. `ExampleGlow` still uses the built-in Unlit/Color shader and needs a visual check; if it appears pink, change it to **Universal Render Pipeline/Unlit**. See [Unity's URP asset guide](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/urp-asset-create.html) and [skybox compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/shader-skybox-shaders.html).
5. The example scenes use real-time lighting; you do not need to bake lighting.

For a development checkout, those files are already present. In a clean consumer, copy the contents of this sample's `StreamingAssets` folder into the consumer project's root `Assets/StreamingAssets` folder before opening the scenes. Merge with an existing folder rather than replacing user files.

If you build WebGL, copy the selected template folder from `WebGLTemplates/` into the consumer project's root `Assets/WebGLTemplates/` folder. Unity discovers custom WebGL templates only from that project path. Select `UXF WebGL 2020` in WebGL Player Settings; the 2019 folder is retained only as a historical migration reference.

The sample data is deliberately kept outside the runtime package assembly. If an agent is diagnosing a sample, report the imported sample version, the scene name and whether these StreamingAssets files were copied.

The examples use `UXFInput` for mouse and key reads, so they can run with **Active Input Handling** set to **Input System Package**. Each scene's active UXF rig switches its EventSystem to `InputSystemUIInputModule` at runtime; no scene-asset change is needed. Package Manager imports are copies: after updating UXF, re-import **UXF Examples** to refresh old scripts, keeping a backup of any local changes first.

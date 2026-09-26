# UXF Examples sample

Import this sample from **Window > Package Manager > In Project > UXF - Unity Experiment Framework > Samples > UXF Examples > Import**. Unity copies it into `Assets/Samples/`; open scenes from that imported folder rather than from the package's hidden `Samples~` source. The scenes and scripts are self-contained, but the automatic, CSV and multi-scene examples also read example settings from `Assets/StreamingAssets`.

For a development checkout, those files are already present. In a clean consumer, copy the contents of this sample's `StreamingAssets` folder into the consumer project's root `Assets/StreamingAssets` folder before opening the scenes. Merge with an existing folder rather than replacing user files.

If you build WebGL, copy the selected template folder from `WebGLTemplates/` into the consumer project's root `Assets/WebGLTemplates/` folder. Unity discovers custom WebGL templates only from that project path. Select `UXF WebGL 2020` in WebGL Player Settings; the 2019 folder is retained only as a historical migration reference.

The sample data is deliberately kept outside the runtime package assembly. If an agent is diagnosing a sample, report the imported sample version, the scene name and whether these StreamingAssets files were copied.

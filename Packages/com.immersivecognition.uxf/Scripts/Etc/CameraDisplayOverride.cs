using UnityEngine;
using UnityEngine.Rendering;

namespace UXF.EditorUtils
{
    /// <summary>
    /// Disables stereo-eye rendering for this camera when using the built-in render pipeline.
    /// Scriptable render pipelines do not support Camera.stereoTargetEye.
    /// </summary>
    public class CameraDisplayOverride : MonoBehaviour
    {
        /// <summary>
        /// Sets the camera to render to neither stereo eye in the built-in render pipeline.
        /// </summary>
        void OnValidate()
        {
#if UNITY_EDITOR
            if (GraphicsSettings.currentRenderPipeline != null)
                return;

            Camera camera = GetComponent<Camera>();
            if (camera != null && camera.stereoTargetEye != StereoTargetEyeMask.None)
                camera.stereoTargetEye = StereoTargetEyeMask.None;
#endif
        }
    }
}

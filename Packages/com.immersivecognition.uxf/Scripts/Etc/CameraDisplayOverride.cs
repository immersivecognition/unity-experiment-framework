using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF.EditorUtils
{
    /// <summary>
    /// Class which handles the cascading settings system. Wraps a Dictionary.
    /// </summary>
    public class CameraDisplayOverride : MonoBehaviour
    {
        
        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
        }

    }
}
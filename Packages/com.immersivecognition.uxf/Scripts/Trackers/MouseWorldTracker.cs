using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to any gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record the position of the mouse in world coordinates.
    /// </summary>
    public class MouseWorldTracker : Tracker
    {
        [Tooltip("Distance from the camera plane to convert pixels to world point.\n\nFor perspective cameras this should be the distance from the camera to the plane you're interested in measuring.\n\nFor orthographic cameras this value shouldn't matter.")]
        public float distanceFromCamera = 1f;

        [Tooltip("Assign the main camera in the scene here.")]
        public Camera mainCamera;

        public override string MeasurementDescriptor => "mouse_world";

        public override IEnumerable<string> CustomHeader => new string[] { "pos_x", "pos_y", "pos_z", };

        /// <summary>
        /// Returns current mouse position in world coordinates
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector3 p = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera));

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                ("pos_x", p.x),
                ("pos_y", p.y),
                ("pos_z", p.z)
            };

            return values;
        }
    }
}
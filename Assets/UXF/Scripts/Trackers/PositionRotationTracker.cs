using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the object at each frame.
    /// </summary>
    public class PositionRotationTracker : Tracker
    {
        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] { "pos_x", "pos_y", "pos_z", "rot_x", "rot_y", "rot_z" };

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector3 p = gameObject.transform.position;
            Vector3 r = gameObject.transform.eulerAngles;

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                ("pos_x", p.x),
                ("pos_y", p.y),
                ("pos_z", p.z),
                ("rot_x", r.x),
                ("rot_y", r.y),
                ("rot_z", r.z)
            };

            return values;
        }
    }
}
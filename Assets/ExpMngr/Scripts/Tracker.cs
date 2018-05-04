using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace ExpMngr
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the object at each frame.
    /// </summary>
    [System.Serializable]
    public class Tracker : MonoBehaviour
    {
        /// <summary>
        /// Name of the object used in saving
        /// </summary>
        public string objectName;
        /// <summary>
        /// The header used when saving the relative filename string within our behavioural data.
        /// </summary>
        public string objectNameHeader { get { return string.Format("{0}_movement_relpath", objectName); } }
        bool recording;

        List<float[]> dataList = new List<float[]>();
        float[] row = new float[6];
        
        /// <summary>
        /// Headers for tracked objects.
        /// </summary>
        public static string[] header = new string[] { "time", "pos_x", "pos_y", "pos_z", "rot_x", "rot_y", "rot_z" };

        // called when component is added
        void Reset()
        {
            objectName = gameObject.name.Replace(" ", "_").ToLower();
        }

        void LateUpdate()
        {
            if (recording)
            {
                row = GraphCurrentValues();
                dataList.Add(row);
            }
        }

        /// <summary>
        /// Begins recording object position.
        /// </summary>
        public void StartRecording()
        {
            dataList.Clear();
            recording = true;
        }

        /// <summary>
        /// Pauses recording object position.
        /// </summary>
        public void PauseRecording()
        {
            recording = false;
        }

        /// <summary>
        /// Stops recording object position and rotation.
        /// </summary>
        public List<float[]> StopRecording()
        {
            recording = false;

            // return copy of data
            return dataList.Clone() as List<float[]>;
        }

        /// <summary>
        /// Acquire values for this frame and store them in an array.
        /// </summary>
        /// <returns></returns>
        public float[] GraphCurrentValues()
        {
            Vector3 p = gameObject.transform.position;
            Vector3 r = gameObject.transform.eulerAngles;

            return new float[]{Time.time,
                 p.x,
                 p.y,
                 p.z,
                 r.x,
                 r.y,
                 r.z
            };

        }
    }
}
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace UXF
{
    /// <summary>
    /// Create a new class that inherits from this component to create custom tracking behaviour on a frame-by-frame basis.
    /// </summary>
    public abstract class Tracker : MonoBehaviour
    {
        /// <summary>
        /// Name of the object used in saving
        /// </summary>
        public string objectName;

        [ReadOnly]
        /// <summary>
        /// Description of the type of measurement this tracker will perform.
        /// </summary>
        public string measurementDescriptor;

        [ReadOnly]
        [Tooltip("Custom column headers for each measurement.")]
        /// <summary>
        /// Custom column headers for tracked objects
        /// </summary>
        public string[] customHeader = new string[] { };

        

        /// <summary>
        /// The header used when saving the relative filename string within our behavioural data.
        /// </summary>
        public string pathHeader
        {
            get
            {
                Debug.AssertFormat(measurementDescriptor.Length > 0, "No measurement descriptor has been specified for the Tracker on {0}!", name);
                return string.Join("_", new string[]{ objectName, measurementDescriptor, "relpath" });
            }
        }

        bool recording;

        List<string[]> data = new List<string[]>();
        string[] row = new string[6];

        public string[] header
        { 
            get
            {
                var newHeader = new string[customHeader.Length + 1];
                newHeader[0] = "time";
                customHeader.CopyTo(newHeader, 1);
                return newHeader;
            }
        } 

        // called when component is added
        void Reset()
        {
            objectName = gameObject.name.Replace(" ", "_").ToLower();
            SetupDescriptorAndHeader();
        }

        // called by unity just before rendering the frame
        void LateUpdate()
        {
            if (recording)
            {
                row = GetCurrentValues();
                if (row.Length != customHeader.Length)
                    throw new InvalidDataException(string.Format("GetCurrentValues provided {0} values but expected the same as the number of headers! {1}", row.Length, customHeader.Length));

                data.Add(row);
            }
        }

        /// <summary>
        /// Begins recording object position.
        /// </summary>
        public void StartRecording()
        {
            data.Clear();
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
        /// Stops recording.
        /// </summary>
        public void StopRecording()
        {
            recording = false;
        }


        public IList<string[]> GetDataCopy()
        {
            return data.Clone();
        }

        /// <summary>
        /// Acquire values for this frame and store them in an array.
        /// </summary>
        /// <returns></returns>
        protected abstract string[] GetCurrentValues();

        /// <summary>
        /// Override this method and define your own descriptor and header.
        /// </summary>
        protected abstract void SetupDescriptorAndHeader();

    }
}
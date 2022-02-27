using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    /// <summary>
    /// Create a new class that inherits from this component to create custom tracking behaviour on a frame-by-frame basis.
    /// </summary>
    public abstract class Tracker : MonoBehaviour
    {
        private bool recording = false;
        private static string[] baseHeaders = new string[] { "time" };

        /// <summary>
        /// Name of the object used in saving
        /// </summary>
        public string objectName;

        /// <summary>
        /// Description of the type of measurement this tracker will perform.
        /// </summary>
        public abstract string MeasurementDescriptor { get; }  

        /// <summary>
        /// Custom column headers for tracked objects.
        /// </summary>
        public abstract IEnumerable<string> CustomHeader { get; }  
   
        /// <summary>
        /// A name used when saving the data from this tracker.
        /// </summary>
        public string DataName
        {
            get
            {
                Debug.AssertFormat(MeasurementDescriptor.Length > 0, "No measurement descriptor has been specified for this Tracker!");
                return string.Join("_", new string[]{ objectName, MeasurementDescriptor });
            }
        }

        public bool Recording { get { return recording; } }

        public UXFDataTable Data { get; private set; } = new UXFDataTable();
        

        /// <summary>
        /// When the tracker should take measurements.
        /// </summary>
        [Tooltip("When the measurements should be taken.\n\nManual should only be selected if the user is calling the RecordRow method either from another script or a custom Tracker class.")]
        public TrackerUpdateType updateType = TrackerUpdateType.LateUpdate;

        // called when component is added
        void Reset()
        {
            objectName = gameObject.name.Replace(" ", "_").ToLower();
        }

        // called by unity just before rendering the frame
        void LateUpdate()
        {
            if (recording && updateType == TrackerUpdateType.LateUpdate) RecordRow();
        }

        // called by unity when physics simulations are run
        void FixedUpdate()
        {
            if (recording && updateType == TrackerUpdateType.FixedUpdate) RecordRow();
        }

        /// <summary>
        /// Records a new row of data at current time.
        /// </summary>
        public void RecordRow()
        {
            if (!recording) throw new System.InvalidOperationException("Tracker measurements cannot be taken when not recording!");
            
            UXFDataRow newRow = GetCurrentValues();
            newRow.Add(("time", Time.time));
            Data.AddCompleteRow(newRow);
        }

        /// <summary>
        /// Begins recording.
        /// </summary>
        public void StartRecording()
        {
            var header = baseHeaders.Concat(CustomHeader);
            Data = new UXFDataTable(header.ToArray());
            recording = true;
        }

        /// <summary>
        /// Stops recording.
        /// </summary>
        public void StopRecording()
        {
            recording = false;
        }

        /// <summary>
        /// Acquire values for this frame and store them in an UXFDataRow. Must return values for ALL columns.
        /// </summary>
        /// <returns></returns>
        protected abstract UXFDataRow GetCurrentValues();

    }

    /// <summary>
    /// When the tracker should collect new measurements. Manual should only be selected if the user is calling the RecordRow method either from another script or a custom Tracker class.
    /// </summary>
    public enum TrackerUpdateType
    {
        LateUpdate, FixedUpdate, Manual
    }
}
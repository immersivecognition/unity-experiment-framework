using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UXF
{

    public abstract class DataHandler : MonoBehaviour
    {
        public bool active = true;
        public Session session { get; private set; }

        public void Initialise(Session session)
        {
            this.session = session;
        }

        public abstract bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = ""); 
        public virtual void SetUp() { }
        public abstract string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public virtual void CleanUp() { }

# if UNITY_EDITOR
        /// <summary>
        /// Returns true if this data handler is definitley compatible with this build target group.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public virtual bool IsCompatibleWith(UnityEditor.BuildTargetGroup buildTarget) { return false; }

         /// <summary>
        /// Returns true if this data handler is definitley incompatible with this build target group.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public virtual bool IsIncompatibleWith(UnityEditor.BuildTargetGroup buildTarget) { return false; }
# endif

    }

    public enum DataType
    {
        SessionInfo, TrialResults, Trackers, Other
    }

    public interface IDataAssociatable
    {
        void SaveDataTable(UXFDataTable table, string dataName, DataType dataType = DataType.SessionInfo);
        void SaveJSONSerializableObject(List<object> serializableObject, string dataName, DataType dataType = DataType.SessionInfo);
        void SaveJSONSerializableObject(Dictionary<string, object> serializableObject, string dataName, DataType dataType = DataType.SessionInfo);
        void SaveText(string text, string dataName, DataType dataType = DataType.SessionInfo);
        void SaveBytes(byte[] bytes, string dataName, DataType dataType = DataType.SessionInfo);
    }


}

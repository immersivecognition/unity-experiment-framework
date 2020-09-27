using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UXF
{

    public abstract class DataHandler : MonoBehaviour
    {
        /// <summary>
        /// Text representing the storage location for the data.
        /// </summary>
        public string storageLocation;
        public Session session { get; private set; }

        /// <summary>
        /// True if this DataHandler requires a local directory.
        /// </summary>
        public static bool requiresLocalDirectory;

        public void Initialise(Session session)
        {
            this.session = session;
        }

        public abstract bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = ""); 
        public abstract void SetUp();
        public abstract string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other);
        public abstract void CleanUp();
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

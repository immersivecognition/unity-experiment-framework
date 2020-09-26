using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UXF
{

    public abstract class DataHandler : MonoBehaviour
    {

        public Session session { get; private set; }
        public static bool requiresLocalDirectory;

        public void Initialise(Session session)
        {
            this.session = session;
        }

        public abstract void HandleDataTable(UXFDataTable table, string experiment, string ppid, string sessionName, string dataName, DataType dataType = DataType.General);
        public abstract void HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, string sessionName, string dataName, DataType dataType = DataType.General);
        public abstract void HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, string sessionName, string dataName, DataType dataType = DataType.General);
        public abstract void HandleText(string text, string experiment, string ppid, string sessionName, string dataName, DataType dataType = DataType.General);
        public abstract void HandleBytes(byte[] bytes, string experiment, string ppid, string sessionName, string dataName, DataType dataType = DataType.General);
    }

    public enum DataType
    {
        General, TrialResults, Tracker, Log, ParticipantDetails, Settings
    }

}

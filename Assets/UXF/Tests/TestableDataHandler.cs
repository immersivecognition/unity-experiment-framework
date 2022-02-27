using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;


namespace UXF.Tests
{
    public class TestableDataHandler : DataHandler
    {
        public UXFDataTable trialResults;

        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNumber = 0)
        {
            return String.Empty;
        }

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNumber = 0)
        {
            if (dataType == UXFDataType.TrialResults)
            {
                trialResults = table;
            }
            return String.Empty;
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNumber = 0)
        {
            return String.Empty;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNumber = 0)
        {
            return String.Empty;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNumber = 0)
        {
            return String.Empty;
        }
    }
}
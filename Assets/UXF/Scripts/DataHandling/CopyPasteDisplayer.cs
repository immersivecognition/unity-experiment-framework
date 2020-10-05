using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;


namespace UXF
{

    public class CopyPasteDisplayer : DataHandler
    {
        public GameObject gameObjectToDisplay;
        public InputField textDisplay;
        public bool trialResultsOnly = true;

        private string accummulatedText = "";

        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false;
        }

        public override void SetUp()
        {
            accummulatedText = "";
        }

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";

            string[] lines = table.GetCSVLines();
            string fname = string.Format("# {0}.csv\n\r", dataName);
            accummulatedText += fname;
            accummulatedText += string.Join("\n\r", lines);
            accummulatedText += "\n\r";

            return fname;
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";

            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("# {0}.json\n\r", dataName);
            accummulatedText += fname;
            accummulatedText += text;
            accummulatedText += "\n\r";

            return fname;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("# {0}.json\n\r", dataName);
            accummulatedText += fname;
            accummulatedText += text;
            accummulatedText += "\n\r";

            return fname;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string fname = string.Format("# {0}.txt\n\r", dataName);
            accummulatedText += fname;
            accummulatedText += text;
            accummulatedText += "\n\r";

            return fname;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string fname = string.Format("# {0}.txt\n\r", dataName);
            accummulatedText += fname;
            accummulatedText += System.Text.Encoding.UTF8.GetString(bytes);
            accummulatedText += "\n\r";

            return fname;
        }

        public override void CleanUp()
        {
            gameObjectToDisplay.SetActive(true);
            textDisplay.text = accummulatedText;
            textDisplay.Select();
            GUIUtility.systemCopyBuffer = accummulatedText;
        }
        
    }


}

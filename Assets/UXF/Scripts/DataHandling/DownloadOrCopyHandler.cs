using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;

namespace UXF.UI
{

    public class DownloadOrCopyHandler : DataHandler
    {
        public GameObject displayUI;
        public Transform contentParent;
        public WebFileElement webFileElementPrefab;
        public bool trialResultsOnly = true;


        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false;
        }

        public override void SetUp()
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";

            string[] lines = table.GetCSVLines();
            string fname = string.Format("{0}.csv", dataName);
            string content = string.Join("\n", lines);
            // if trial results, push to top of list for convenience
            CreateNewItem(content, fname, pushToTop: dataType == DataType.TrialResults); 

            return fname;
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";

            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("{0}.json", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("{0}.json", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string fname = string.Format("{0}.txt", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType = DataType.Other)
        {
            if (dataType != DataType.TrialResults && trialResultsOnly) return "NA";
            string fname = string.Format("{0}.txt", dataName);
            string content = System.Text.Encoding.UTF8.GetString(bytes);
            CreateNewItem(content, fname);

            return fname;
        }

        public override void CleanUp()
        {
            displayUI.SetActive(true);
        }

        void CreateNewItem(string content, string filename, bool pushToTop = false)
        {
            WebFileElement newElement = Instantiate(webFileElementPrefab, contentParent);
            if (pushToTop) newElement.transform.SetAsFirstSibling();

            newElement.Setup(content, filename);
        }
        
    }


}

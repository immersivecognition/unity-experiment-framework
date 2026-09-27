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

        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            if (dataType != UXFDataType.TrialResults && trialResultsOnly) return "NA";
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string[] lines = table.GetCSVLines();
            string fname = string.Format("{0}.csv", dataName);
            string content = string.Join("\n", lines);
            // if trial results, push to top of list for convenience
            CreateNewItem(content, fname, pushToTop: dataType == UXFDataType.TrialResults); 

            return fname;
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            if (dataType != UXFDataType.TrialResults && trialResultsOnly) return "NA";
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("{0}.json", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            if (dataType != UXFDataType.TrialResults && trialResultsOnly) return "NA";
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string text = MiniJSON.Json.Serialize(serializableObject);
            string fname = string.Format("{0}.json", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            if (dataType != UXFDataType.TrialResults && trialResultsOnly) return "NA";
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string fname = string.Format("{0}.txt", dataName);
            CreateNewItem(text, fname);

            return fname;
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            if (dataType != UXFDataType.TrialResults && trialResultsOnly) return "NA";
            if (dataType.GetDataLevel() == UXFDataLevel.PerTrial) dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

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

# if UNITY_EDITOR
        /// <summary>
        /// Returns true if this data handler is definitley compatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTargetGroup.Standalone:
                case UnityEditor.BuildTargetGroup.WebGL:
                    return true;
                default:
                    return false;
            }
        }

         /// <summary>
        /// Returns true if this data handler is definitley incompatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsIncompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                default:
                    return false;
            }
        }
# endif
        
    }


}

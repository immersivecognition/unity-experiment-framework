using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Data;
using System.Linq;
using UnityEngine.Events;
using System;

namespace UXF
{
    public class ParticipantListSelection : MonoBehaviour
    {

        public string currentFolder;
        string ppListPath;
        public DataTable ppList = null;
        public Text ppListNameDisplay;
        public ParticipantSelector participantSelector;
        public FileIOManager fileIOManager;
        public ExperimentStartupController startup;
        public FillableFormController form;
        public PopupController popupController;
        public Button startButton;

        string ppListLocKey = "ParticipantListLocation";

        public void Init()
        {
            ExperimentStartupController.SetSelectableAndChildrenInteractable(participantSelector.gameObject, false);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(form.gameObject, false);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, false);
            SetFromCache();
        }

        public void SetFromCache()
        {            
            if (PlayerPrefs.HasKey(ppListLocKey))
            {
                string loc = PlayerPrefs.GetString(ppListLocKey);
                if (File.Exists(loc))
                {
                    CheckSetList(loc);
                }
            }
        }
        

        public void SelectList()
        {
            SFB.StandaloneFileBrowser.OpenFilePanelAsync("Select participant list", currentFolder, "csv", false, (string[] paths) => { CheckSetList(paths); });
        }

        public void CreateList()
        {
            SFB.StandaloneFileBrowser.SaveFilePanelAsync("Create participant list", currentFolder, "participant_list", "csv", (string path) => { PrepNewPPList(path); });
        }


        public void CheckSetList(string[] paths)
        {
            if (paths.Length == 0) { return; }
            CheckSetList(paths[0]);
        }


        public void CheckSetList(string path)
        {
            if (path == "") return;
            ppListPath = path;

            ppListNameDisplay.text = ppListPath;
            ppListNameDisplay.color = Color.black;
            currentFolder = Directory.GetParent(ppListPath).ToString();

            GetCheckParticipantList();
        }

        public void GetCheckParticipantList()
        {
            fileIOManager.ReadCSV(ppListPath, new System.Action<DataTable>((data) => SetPPList(data)));
        }

        void PrepNewPPList(string path)
        {
            if (path == "") return;
    
            Popup pplistAttention = new Popup();
            pplistAttention.messageType = MessageType.Attention;
            pplistAttention.message = string.Format("An empty participant list has been created at {0}. Data you collect will be stored in the same folder as this list.", ppListPath);
            pplistAttention.onOK = new System.Action(() => { CreateNewPPList(path); });
            popupController.DisplayPopup(pplistAttention);
            return;
        }

        void CreateNewPPList(string filePath)
        {
            // create example table
            DataTable exampleData = new DataTable();

            // create headers
            foreach (var header in startup.participantDataPoints.Select(x => x.internalName))
            {
                exampleData.Columns.Add(new DataColumn(header, typeof(string)));
            }            

            // create example row
            DataRow row1 = exampleData.NewRow();
            foreach (var dataPoint in startup.participantDataPoints)
            {
                row1[dataPoint.internalName] = dataPoint.controller.GetDefault();
            }
            row1["ppid"] = "test";

            exampleData.Rows.Add(row1);

            FileInfo fileInfo = new FileInfo(filePath);

            WriteFileInfo writeFileInfo = new WriteFileInfo(
                WriteFileType.ParticipantList,
                fileInfo.DirectoryName,
                fileInfo.Name
                );

            // save, this is single threaded in this case
            fileIOManager.WriteCSV(exampleData, writeFileInfo);

            // re-read it back in
            CheckSetList(filePath);
        }


        public void SetPPList(DataTable data)
        {
            ppList = data;
            if (ppList == null)
            {
                throw new Exception("Error with participant list");
            }

            Debug.Log(string.Format("Loaded: {0}", ppListPath));

            List<string> participants = ppList.AsEnumerable().Select(x => x[0].ToString()).ToList();
            participantSelector.SetParticipants(participants);
            participantSelector.SelectNewList();

            PlayerPrefs.SetString(ppListLocKey, ppListPath);

            // enable selector
            ExperimentStartupController.SetSelectableAndChildrenInteractable(participantSelector.gameObject, true);
            // enable form
            ExperimentStartupController.SetSelectableAndChildrenInteractable(form.gameObject, true);
            // enable start button
            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, true);

            foreach (var dataPoint in startup.participantDataPoints)
            {
                if (!ppList.Columns.Contains(dataPoint.internalName))
                {
                    ppList.Columns.Add(new DataColumn(dataPoint.internalName, typeof(string)));
                }
            }
        }


        public void UpdateFormByPPID(string ppid)
        {
            DataRow row = ppList.AsEnumerable().Single(r => r.Field<string>("ppid") == ppid);

            foreach (var dataPoint in startup.participantDataPoints)
            {
                try
                {
                    dataPoint.controller.SetContents(row[dataPoint.internalName]);
                }
                catch (ArgumentException)
                {
                    string s = string.Format("Column '{0}' not found in participant list - It will be added with empty values", dataPoint.internalName);
                    Debug.LogWarning(s);
                    
                    ppList.Columns.Add(new DataColumn(dataPoint.internalName, typeof(string)));
                    dataPoint.controller.Clear();
                }
            }
        }


        public string Finish()
        {
            // get completed information form
            var completedForm = form.GetCompletedForm();

            if (completedForm == null)
                throw new Exception("Form not completed correctly!");

            // get PPID and set to safe name
            string ppid = completedForm["ppid"].ToString();
            ppid = Extensions.GetSafeFilename(ppid);

            // check if not empty
            if (ppid.Replace(" ", string.Empty) == string.Empty)
            {
                form.ppidElement.controller.DisplayFault();
                throw new Exception("Invalid participant name!");
            }

            DataRow row;
            // if we have new participant selected, we need to create it in the pplist
            if (participantSelector.IsNewSelected())
            {
                // add new participant to list
                row = ppList.NewRow();
                ppList.Rows.Add(row);
            }
            // else we update the row with any changes we made in the form
            else 
            {
                string oldPpid = participantSelector.participantDropdown.GetContents().ToString();
                // update row
                row = ppList.AsEnumerable().Single(r => r.Field<string>("ppid") == oldPpid);
            }

            // update row in table
            foreach (var keyValuePair in completedForm)
                row[keyValuePair.Key] = keyValuePair.Value;     

            // write pplist
            CommitCSV();
            return ppid;

        }

        public void UpdateDatapoint(string ppid, string datapointName, object value)
        {
            DataRow row = ppList.AsEnumerable().Single(r => r.Field<string>("ppid") == ppid);
            
            try
            {
                row[datapointName] = value;
            }
            catch (ArgumentException e)
            {
                string s = string.Format("Column '{0}' not found in data table - It will be added with empty values", e.ParamName);
                Debug.LogWarning(s);
                ppList.Columns.Add(new DataColumn(datapointName, typeof(string)));
                row[datapointName] = value;
            }
            
        }

        public void CommitCSV()
        {
            FileInfo fileInfo = new FileInfo(ppListPath);

            WriteFileInfo writeFileInfo = new WriteFileInfo(
                WriteFileType.ParticipantList,
                fileInfo.DirectoryName,
                fileInfo.Name
                );

            fileIOManager.WriteCSV(ppList, writeFileInfo);
            Debug.Log(string.Format("Updating: {0}", ppListPath));
        }

        public Dictionary<string, object> GenerateDict()
        {
            return form.GetCompletedForm();
        }

    }
}
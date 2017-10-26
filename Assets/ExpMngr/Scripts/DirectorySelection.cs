using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Data;
using System.Linq;
using UnityEngine.Events;
using System;

namespace ExpMngr
{
    public class DirectorySelection : MonoBehaviour
    {

        public string currentFolder;
        string ppListPath { get { return Path.Combine(currentFolder, "participant_list.csv"); } }
        public DataTable ppList = null;

        public Text folderNameDisplay;
        public ParticipantSelector participantSelector;
        public ExperimentSession experiment;
        public ExperimentStartupController startup;
        public FillableFormController form;
        public Button startButton;

        void Start()
        {
            ExperimentStartupController.SetSelectableAndChildrenInteractable(participantSelector.gameObject, false);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(form.gameObject, false);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, false);
        }


        public void SelectFolder()
        {
            SFB.StandaloneFileBrowser.OpenFolderPanelAsync("Select experiment folder", currentFolder, false, (string[] paths) => { CheckSetFolder(paths); });
        }


        public void CheckSetFolder(string[] paths)
        {
            if (paths.Length == 0) { return; }
            string path = paths[0];

            folderNameDisplay.text = path;
            folderNameDisplay.color = Color.black;
            currentFolder = path;

            GetCheckParticipantList();

        }


        public void GetCheckParticipantList()
        {
            experiment.ReadCSVFile(ppListPath, new System.Action<DataTable>((data) => SetPPList(data)));
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
            row1["ppid"] = "1";

            exampleData.Rows.Add(row1);

            // save
            experiment.WriteCSVFile(exampleData, filePath);

            // re-read it back in
            GetCheckParticipantList();
        }


        public void SetPPList(DataTable data)
        {
            ppList = data;
            if (ppList == null)
            {
                Debug.LogWarning("No participant list found in directory! Creating an example one in the experiment directory.");
                CreateNewPPList(ppListPath);

                //// disable selector
                //participantSelector.SelectNew();
                //ExperimentStartupController.SetSelectableAndChildrenInteractable(participantSelector.gameObject, false);
                //// disable form
                //form.Clear();
                //ExperimentStartupController.SetSelectableAndChildrenInteractable(form.gameObject, false);
                //// disable start button
                //ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, false);

                return;
            }

            Debug.Log(string.Format("Loaded: {0}", ppListPath));

            List<string> participants = ppList.AsEnumerable().Select(x => x[0].ToString()).ToList();
            participantSelector.SetParticipants(participants);
            participantSelector.SelectNew();

            // enable selector
            ExperimentStartupController.SetSelectableAndChildrenInteractable(participantSelector.gameObject, true);
            // enable form
            ExperimentStartupController.SetSelectableAndChildrenInteractable(form.gameObject, true);
            // enable start button
            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, true);

        }


        public void UpdateFormByPPID(string ppid)
        {
            DataRow row = ppList.AsEnumerable().Single(r => r.Field<string>("ppid") == ppid);

            foreach (var dataPoint in startup.participantDataPoints)
            {
                dataPoint.controller.SetContents(row[dataPoint.internalName]);
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
            experiment.WriteCSVFile(ppList, ppListPath);
            Debug.Log(string.Format("Updating: {0}", ppListPath));
            return ppid;

        }


    }
}
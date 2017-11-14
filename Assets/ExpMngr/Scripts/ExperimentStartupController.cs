using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace ExpMngr
{
    public class ExperimentStartupController : MonoBehaviour
    {
        [SerializeField]
        private List<FormElementEntry> _participantDataPoints = new List<FormElementEntry>();
        /// <summary>
        /// List of datapoints you want to collect per participant. These will be generated for the GUI and added as new columns in the participant list. Participant ID is added automatically.
        /// </summary>
        public List<FormElementEntry> participantDataPoints { get { return _participantDataPoints; } }

        /// <summary>
        /// Maximum number of "sessions" available to select via the UI
        /// </summary>
        public int maxNumSessions = 1;

        public string newParticipantName = "<i><color=grey>+ New participant</color></i>";

        public DirectorySelection dirSelect;
        public FillableFormController ppInfoForm;
        public DropDownController sessionNumDropdown;

        public PopupController popupController;

        [Space]
        public ExperimentSession experimentSession;




        void Start()
        {
            ppInfoForm.Generate(participantDataPoints);

            List<string> sessionList = new List<string>();
            for (int i = 1; i <= maxNumSessions; i++)
            {
                sessionList.Add(i.ToString());
            }
            sessionNumDropdown.SetItems(sessionList);

            dirSelect.Init();
        }



        public static void SetSelectableAndChildrenInteractable(GameObject stepGameObject, bool state)
        {
            try { stepGameObject.GetComponent<Selectable>().interactable = state; }
            catch (NullReferenceException) { }

            var selectables = stepGameObject.GetComponentsInChildren<Selectable>();
            foreach (var selectable in selectables)
            {
                selectable.interactable = state;
            }
        }



        /// <summary>
        /// Called upon press of the start button in the UI. Creates the experiment session
        /// </summary>
        public void StartExperiment()
        {
            string ppid = dirSelect.Finish();
            int sessionNum = int.Parse(sessionNumDropdown.GetContents().ToString());
            var infoDict = dirSelect.GenerateDict();
            
            Action finish = new Action( () =>
                {
                    experimentSession.InitSession(ppid, sessionNum, dirSelect.currentFolder, infoDict);
                    gameObject.SetActive(false);
                } 
            );

            bool exists = experimentSession.CheckSessionExists(ppid, sessionNum, dirSelect.currentFolder);
            if (exists)
            {
                Popup existsWarning = new Popup();
                existsWarning.messageType = MessageType.Warning;
                existsWarning.message = string.Format("Warning - session \\{0}\\{1:0000}\\ already exists. Pressing OK will overwrite all data collected for this session", new object[]{ppid, sessionNum});
                existsWarning.onOK = finish;
                popupController.DisplayPopup(existsWarning);
            }
            else
            {
                finish.Invoke();
            }

        }
    }
}
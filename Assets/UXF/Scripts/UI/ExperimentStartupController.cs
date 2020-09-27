using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SubjectNerd.Utilities;

namespace UXF.UI
{
    public class ExperimentStartupController : MonoBehaviour
    {
        [Header("User interface")]

        [Tooltip("List of datapoints you want to collect per participant. These will be generated for the GUI and added as new columns in the participant list. Participant ID is added automatically.")]
        [SerializeField]
        [Reorderable]
        private List<FormElementEntry> _participantDataPoints = new List<FormElementEntry>();
        /// <summary>
        /// List of datapoints you want to collect per participant. These will be generated for the GUI and added as new columns in the participant list. Participant ID is added automatically.
        /// </summary>
        public List<FormElementEntry> participantDataPoints { get { return _participantDataPoints; } }

        [Tooltip("Maximum number available to select as the session number via the UI")]
        public int maxNumSessions = 1;

        [Tooltip("Search pattern to use when scanning the StreamingAssets folder for settings files.")]
        public string settingsSearchPattern = "*.json";

        [HideInInspector]
        public string newParticipantName = "<i><color=grey>+ New participant</color></i>";

        [Header("Instance references")]
        public SettingsSelector settingsSelector;
        public DirectorySelector dirSelect;
        public FillableFormController ppInfoForm;
        public DropDownController sessionNumDropdown;
        public PopupController popupController;

        public GameObject startupPanel;

        [Space]
        public Session session;

        void Start()
        {
            ppInfoForm.Generate(participantDataPoints, true);

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
            int sessionNum = int.Parse(sessionNumDropdown.GetContents().ToString());
            var infoDict = ppInfoForm.GetCompletedForm();
            var settings = settingsSelector.GetSettings();
            string ppid = (string) infoDict["ppid"];

            Action finish = new Action(() =>
                {
                    session.Begin(settingsSelector.experimentName,
                                                    ppid,
                                                    dirSelect.currentDirectory,
                                                    sessionNum,
                                                    infoDict,
                                                    settings);
                    startupPanel.SetActive(false);
                }
            );

            bool exists = session.CheckSessionExists(dirSelect.currentDirectory, settingsSelector.experimentName, ppid, sessionNum);

            if (exists)
            {
                Popup existsWarning = new Popup();
                existsWarning.messageType = MessageType.Warning;
                existsWarning.message = string.Format("Warning - session \\{0}\\{1}\\{2:0000}\\ already exists. Pressing OK will overwrite all data collected for this session", session.experimentName, ppid, sessionNum);
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
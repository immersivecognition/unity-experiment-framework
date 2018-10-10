using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace UXF
{
    public class ExperimentStartupController : MonoBehaviour
    {

        [Header("Quick start")]
        [Tooltip("When enabled, the experiment will instantly start using the 'quick_start' as the participant id, 1 as the session, and the save folder and settings path provided")]
        public bool quickStartMode;

        [Tooltip("Save data location in quick start (i.e. directory where the participant list is located). Relative to project path.")]
        [ConditionalHide("quickStartMode", true)]
        public string saveDataLocation = "example_output";

        [Tooltip("Name of the settings file to be used in quick start (as located in StreamingAssets folder)")]

        [ConditionalHide("quickStartMode", true)]
        public string experimentSettingsName = "example_experiment_1.json";

        [Header("User interface")]

        [Tooltip("List of datapoints you want to collect per participant. These will be generated for the GUI and added as new columns in the participant list. Participant ID is added automatically.")]
        [SerializeField]
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
        public ParticipantListSelection ppListSelect;
        public FillableFormController ppInfoForm;
        public DropDownController sessionNumDropdown;
        
        public PopupController popupController;

        public GameObject startupPanel;

        [Space]
        public Session session;

        void Start()
        {

            if (quickStartMode)
            {
                QuickStart();
            }
            else
            {
                ppInfoForm.Generate(participantDataPoints, true);

                List<string> sessionList = new List<string>();
                for (int i = 1; i <= maxNumSessions; i++)
                {
                    sessionList.Add(i.ToString());
                }
                sessionNumDropdown.SetItems(sessionList);

                ppListSelect.Init();
            }

        }

        public void QuickStart()
        {

            string experimentName = Path.GetFileNameWithoutExtension(experimentSettingsName);

            string path = Path.IsPathRooted(saveDataLocation) ? saveDataLocation : Path.Combine(Directory.GetCurrentDirectory(), saveDataLocation);

            if (!Directory.Exists(path))
            {
                Debug.LogErrorFormat("Quick start failed: Cannot find path {0}", path);
                return;
            }

            Action<Dictionary<string, object>> finish = new Action<Dictionary<string, object>>((dict) =>
            {
                session.Begin(
                    experimentName,
                    "quick_start",
                    path,
                    1,
                    null,
                    new Settings(dict)
                );
                startupPanel.SetActive(false);
            });

            session.ReadSettingsFile(
                Path.Combine(settingsSelector.settingsFolder, experimentSettingsName),
                finish
            );
            
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
            string ppid = ppListSelect.Finish();
            int sessionNum = int.Parse(sessionNumDropdown.GetContents().ToString());
            var infoDict = ppListSelect.GenerateDict();
            var settings = settingsSelector.GetSettings();

            Action finish = new Action(() =>
                {
                    session.Begin(settingsSelector.experimentName,
                                                    ppid,
                                                    ppListSelect.currentFolder,
                                                    sessionNum,
                                                    infoDict,
                                                    settings);
                    startupPanel.SetActive(false);
                }
            );

            bool exists = Session.CheckSessionExists(settingsSelector.experimentName, ppid, ppListSelect.currentFolder, sessionNum);

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
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using SubjectNerd.Utilities;

namespace UXF.UI
{
    [DefaultExecutionOrder(100)]
    public class UIController : MonoBehaviour
    {
        [Tooltip("How should the session be started? With the built-in UI, automatic start as soon as the scene is loaded, or manual start (useful if you want to build your own UI).")]
        public StartupMode startupMode = StartupMode.BuiltInUI;

        [Tooltip("Name of the experiment used in data output.")]
        public string experimentName = "my_experiment";

        [Tooltip("Where should the session settings be acquired from?")]
        public SettingsMode settingsMode = SettingsMode.AcquireFromUI;

        [Tooltip("The pattern used to search for settings profile files in the StreamingAssets folder.")]
        public string settingsSearchPattern = "*.json";

        [Tooltip("The location of the settings profile file to download.")]
        public string jsonURL = "https://gist.githubusercontent.com/jackbrookes/0f9770fcfe3d448e0f7a1973c2ac7419/raw/f2d234c92c77a817f9fc6390fcfcb39814c33d3c/example_settings.json";

        [Tooltip("How should the Participant ID be collected?")]
        public PPIDMode ppidMode = PPIDMode.AcquireFromUI;

        [Tooltip("Assign to a .txt file that contains one word per line. A random item in the list will be used for generating the PPID.")]
        public TextAsset uuidWordList;

        [Tooltip("Should the session number be acquired from the UI, or always be set to 1?")]
        public SessionNumMode sessionNumMode = SessionNumMode.AcquireFromUI;

        [SubjectNerd.Utilities.Reorderable]
        public List<FormElementEntry> participantDataPoints = new List<FormElementEntry>();

        [Tooltip("The text shown next to the terms and conditions checkbox.")]
        [TextArea]
        public string termsAndConditions = "Please tick if you understand the instructions and agree for your data to be collected and used for research purposes.<color=red>*</color>";

        [Tooltip("Should the terms and conditions checkbox be pre-ticked?")]
        public bool tsAndCsInitialState = false;

        public bool RequiresFilePathElement
        {
            get
            {
                if (session == null) return false;
                return session.ActiveDataHandlers
                    .Where((dh) => dh is LocalFileDataHander)
                    .Any(dh => ((LocalFileDataHander)dh).dataSaveLocation == DataSaveLocation.AcquireFromUI);
            }
        }

        public IEnumerable<LocalFileDataHander> ActiveLocalFileDataHandlers
        {
            get
            {
                if (session == null)
                {
                    return new List<LocalFileDataHander>();
                }
                else
                {
                    return session.ActiveDataHandlers
                        .Where((dh) => dh is LocalFileDataHander)
                        .Cast<LocalFileDataHander>();
                }
            }
        }

        #region HIDDEN_VARIABLES
        public Transform instructionsContentTransform;
        public Transform sidebarContentTransform;
        public FormElement settingsElement;
        public FormElement localFilePathElement;
        public FormElement ppidElement;
        public FormElement sessionNumElement;
        public FormElement tsAndCsToggle;
        public FormElement textPrefab;
        public FormElement dropDownPrefab;
        public FormElement checkBoxPrefab;
        private Session session;
        private Canvas canvas;
        private PopupController popupController;
        private string[] words;
        private string jsonText;
        private Coroutine uiStartRoutine;
        private Coroutine autoStartRoutine;

        #endregion

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            if (session == null) session = GetComponentInParent<Session>();
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (popupController == null) popupController = GetComponentInChildren<PopupController>(true);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += LateValidate;
            foreach (var dh in ActiveLocalFileDataHandlers)
            {
                dh.onValidateEvent.AddListener(() => UnityEditor.EditorApplication.delayCall += UpdateLocalFileElementState);
            }
#endif
        }

        // https://forum.unity.com/threads/sendmessage-cannot-be-called-during-awake-checkconsistency-or-onvalidate-can-we-suppress.537265/
        public void LateValidate()
        {
            if (tsAndCsToggle != null)
            {
                tsAndCsToggle.title.text = termsAndConditions;
                tsAndCsToggle.SetContents(tsAndCsInitialState);
            }
            UpdateExperimentProfileElementState();
            UpdatePPIDSessionNumElementState();
            UpdateUIState();
            UpdateLocalFileElementState();
        }

        void UpdateUIState()
        {
            if (settingsElement != null) canvas.enabled = (startupMode == StartupMode.BuiltInUI);
        }

        void UpdateExperimentProfileElementState()
        {
            if (settingsElement != null) settingsElement.gameObject.SetActive(settingsMode == SettingsMode.AcquireFromUI);
        }

        void UpdateLocalFileElementState()
        {
            if (localFilePathElement != null) localFilePathElement.gameObject.SetActive(RequiresFilePathElement);
        }

        void UpdatePPIDSessionNumElementState()
        {
            if (ppidElement != null) ppidElement.gameObject.SetActive(ppidMode == PPIDMode.AcquireFromUI);
            if (sessionNumElement != null) sessionNumElement.gameObject.SetActive(ppidMode == PPIDMode.AcquireFromUI && sessionNumMode == SessionNumMode.AcquireFromUI);
        }


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if (session == null) session = GetComponentInParent<Session>();
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (popupController == null) popupController = GetComponentInChildren<PopupController>(true);
            // read word list
            if (uuidWordList) words = uuidWordList.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            GenerateSidebar();
        }


        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            UpdateExperimentProfileElementState();
            UpdateLocalFileElementState();
            UpdatePPIDSessionNumElementState();
            UpdateUIState();
            if (startupMode == StartupMode.Automatic) AutoBeginSession();
        }


        public void AutoBeginSession()
        {
            if (autoStartRoutine == null) autoStartRoutine = StartCoroutine(AutoBeginSessionSequence());
        }

        IEnumerator AutoBeginSessionSequence()
        {
            Settings newSettings = Settings.empty;
            yield return GetJsonUrl();
            if (jsonText == string.Empty)
            {
                Utilities.UXFDebugLogErrorFormat("Error downloading data from URL: {0}. Using blank settings instead.", jsonURL);
            }
            try
            {
                newSettings = new Settings((Dictionary<string, object>)MiniJSON.Json.Deserialize(jsonText));
            }
            catch (InvalidCastException)
            {
                Utilities.UXFDebugLogErrorFormat("Text downloaded from {0} is cannot be parsed, empty settings used instead. Check the data is valid json ({1})", jsonURL, jsonText);
            }

            autoStartRoutine = null;

            session.Begin(
                experimentName,
                GenerateUniquePPID(),
                settings: newSettings
            );
        }

        public void TryBeginSessionFromUI()
        {
            if (uiStartRoutine == null) uiStartRoutine = StartCoroutine(TryBeginSessionFromUISequence());
        }

        IEnumerator TryBeginSessionFromUISequence()
        {
            bool error = false;

            // EXPERIMENT NAME
            string newExperimentName;
            switch (settingsMode)
            {
                case SettingsMode.AcquireFromUI:
                    newExperimentName = settingsElement
                        .GetContents()
                        .ToString()
                        .Replace(".json", "");
                    break;
                case SettingsMode.DownloadFromURL:
                case SettingsMode.Empty:
                    newExperimentName = experimentName;
                    break;
                default:
                    throw new Exception();
            }

            // DATA PATH
            string localFilePath = "";
            if (RequiresFilePathElement)
            {
                if (!localFilePathElement.gameObject.activeSelf)
                {
                    Utilities.UXFDebugLogError("Cannot start session - need Local Data Directory element, but it is not active.");
                    yield break;
                }

                localFilePath = (string)localFilePathElement.GetContents();
                if (localFilePath.Trim() == string.Empty)
                {
                    localFilePathElement.DisplayFault();
                    Utilities.UXFDebugLogError("Local data directory is empty");
                    error = true;
                }
                else if (!Directory.Exists(localFilePath))
                {
                    localFilePathElement.DisplayFault();
                    Utilities.UXFDebugLogErrorFormat("Cannot start session - local data directory {0} does not exist.", localFilePath);
                    error = true;
                }

                foreach (var dh in ActiveLocalFileDataHandlers)
                {
                    dh.StoragePath = localFilePath;
                }
            }

            // PPID & SESSION NUM
            string newPpid = "";
            int sessionNum = 1;
            switch (ppidMode)
            {
                case PPIDMode.AcquireFromUI:
                    newPpid = ppidElement
                        .GetContents()
                        .ToString()
                        .Trim();
                    if (newPpid == string.Empty)
                    {
                        ppidElement.DisplayFault();
                        error = true;
                    }
                    if (sessionNumMode == SessionNumMode.AcquireFromUI)
                    {
                        int newSessionNum = Convert.ToInt32(sessionNumElement.GetContents());
                        if (newSessionNum <= 0)
                        {
                            sessionNumElement.DisplayFault();
                            error = true;
                        }
                        else
                        {
                            sessionNum = newSessionNum;
                        }
                    }
                    break;
                case PPIDMode.GenerateUnique:
                    newPpid = GenerateUniquePPID();
                    break;
                default:
                    throw new Exception();
            }

            // PARTICIPANT DETAILS
            Dictionary<string, object> newParticipantDetails;
            var validityList = SidebarStateIsValid(out newParticipantDetails);
            bool sidebarValid = true;
            foreach (var v in validityList)
            {
                sidebarValid = sidebarValid && v.valid;
                if (!v.valid && v.entry.element != null) v.entry.element.DisplayFault();
            }
            if (!sidebarValid) error = true;

            // TERMS AND CONDITIONS
            bool acceptedTsAndCs = (bool)tsAndCsToggle.GetContents();
            if (!acceptedTsAndCs)
            {
                tsAndCsToggle.DisplayFault();
                error = true;
            }


            // SETTINGS
            Settings newSettings = null;
            switch (settingsMode)
            {
                case SettingsMode.AcquireFromUI:
                    string settingsPath = Path.Combine(Application.streamingAssetsPath, settingsElement.GetContents().ToString());
                    string settingsText;
                    try
                    {
                        settingsText = File.ReadAllText(settingsPath);
                    }
                    catch (FileNotFoundException e)
                    {
                        Debug.LogException(e);
                        settingsElement.DisplayFault();
                        error = true;
                        break;
                    }
                    Dictionary<string, object> deserializedJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(settingsText);
                    if (deserializedJson == null)
                    {
                        Utilities.UXFDebugLogErrorFormat("Cannot deserialize json file: {0}.", settingsPath);
                        settingsElement.DisplayFault();
                        error = true;
                    }
                    else
                    {
                        newSettings = new Settings(deserializedJson);
                    }
                    break;
                case SettingsMode.DownloadFromURL:
                    yield return GetJsonUrl();
                    if (jsonText == string.Empty)
                    {
                        error = true;
                        Utilities.UXFDebugLogErrorFormat("Error downloading data from URL: {0}. Using blank settings instead.", jsonURL);
                        newSettings = Settings.empty;
                    }
                    try
                    {
                        newSettings = new Settings((Dictionary<string, object>)MiniJSON.Json.Deserialize(jsonText));
                    }
                    catch (InvalidCastException)
                    {
                        error = true;
                        Utilities.UXFDebugLogErrorFormat("Text downloaded from {0} is cannot be parsed, empty settings used instead. Check the data is valid json ({1})", jsonURL, jsonText);
                        newSettings = Settings.empty;
                    }
                    break;
                case SettingsMode.Empty:
                    newSettings = Settings.empty;
                    break;
                default:
                    throw new Exception();
            }

            uiStartRoutine = null;
            if (error) yield break;

            bool exists = session.CheckSessionExists(
                localFilePath,
                newExperimentName,
                newPpid,
                sessionNum
            );

            if (exists)
            {
                Popup newPopup = new Popup()
                {
                    message = string.Format(
                        "{0} - {1} - Session #{2} already exists! Press OK to start the session anyway, data may be overwritten.",
                        newExperimentName,
                        newPpid,
                        sessionNum
                    ),
                    messageType = MessageType.Warning,
                    onOK = () => {
                        gameObject.SetActive(false);
                        // BEGIN!
                        session.Begin(
                            newExperimentName,
                            newPpid,
                            sessionNum,
                            newParticipantDetails,
                            newSettings
                        );
                    }
                };
                popupController.DisplayPopup(newPopup);
            }
            else
            {
                gameObject.SetActive(false);

                // BEGIN!
                session.Begin(
                    newExperimentName,
                    newPpid,
                    sessionNum,
                    newParticipantDetails,
                    newSettings
                );
            }

            
        }

        public string GenerateUniquePPID()
        {
            string prefix = string.Empty;
            if (words != null) prefix = words[UnityEngine.Random.Range(0, words.Length - 1)] + "-";
            string ppid = Guid.NewGuid().ToString();
            return Extensions.GetSafeFilename(prefix + ppid);
        }

        public void GenerateSidebar()
        {
            Transform[] children = sidebarContentTransform
                .Cast<Transform>()
                .Select(c => c.transform)
                .ToArray();

            foreach (Transform child in children)
            {
                if (ReferenceEquals(settingsElement.transform, child)) continue;
                else if (ReferenceEquals(ppidElement.transform, child)) continue;
                else if (ReferenceEquals(localFilePathElement.transform, child)) continue;
                else if (ReferenceEquals(sessionNumElement.transform, child)) continue;
                else DestroyImmediate(child.gameObject);
            }

            foreach (FormElementEntry entry in participantDataPoints)
            {
                FormElement newElement = null;
                switch (entry.dataType)
                {
                    case FormDataType.String:
                    case FormDataType.Int:
                    case FormDataType.Float:
                        newElement = Instantiate(textPrefab, sidebarContentTransform);
                        newElement.title.text = entry.displayName;
                        newElement.SetDataType(entry.dataType);
                        break;
                    case FormDataType.Bool:
                        newElement = Instantiate(checkBoxPrefab, sidebarContentTransform);
                        newElement.title.text = entry.displayName;
                        break;
                    case FormDataType.DropDown:
                        newElement = Instantiate(dropDownPrefab, sidebarContentTransform);
                        newElement.title.text = entry.displayName;
                        newElement.SetContents(entry.dropDownOptions);
                        break;
                }
                entry.element = newElement;
            }
        }


        public (FormElementEntry entry, bool valid, string invalidReason)[] SidebarStateIsValid(out Dictionary<string, object> data)
        {
            var validityList = new (FormElementEntry entry, bool valid, string invalidReason)[participantDataPoints.Count];
            data = new Dictionary<string, object>();

            int i = 0;
            foreach (FormElementEntry entry in participantDataPoints)
            {
                if (entry.element == null)
                {
                    validityList[i] = (entry, false, string.Format("The UI element for '{0}' ({1}) has not been generated.", entry.displayName, entry.internalName));
                }
                else
                {
                    object contents;
                    bool valid = SidebarItemIsValid(entry, out contents);
                    if (valid)
                    {
                        data.Add(entry.internalName, contents);
                    }
                    string reason = valid ?
                        string.Empty
                        : string.Format("The data in the UI element '{0}' ({1}) cannot be converted to selected data type ({2}).", entry.displayName, entry.internalName, entry.dataType);
                    validityList[i] = (entry, valid, reason);
                }
                i++;
            }

            return validityList;
        }


        public static bool SidebarItemIsValid(FormElementEntry entry, out object contents)
        {
            contents = null;
            try
            {
                switch (entry.dataType)
                {
                    case FormDataType.String:
                        contents = entry.element.GetContents();
                        break;
                    case FormDataType.Float:
                        contents = Convert.ToSingle(entry.element.GetContents());
                        break;
                    case FormDataType.Int:
                        contents = Convert.ToInt32(entry.element.GetContents());
                        break;
                    case FormDataType.Bool:
                        contents = entry.element.GetContents();
                        break;
                    case FormDataType.DropDown:
                        contents = entry.element.GetContents();
                        break;
                }
                return true;
            }
            catch (FormatException)
            {
                contents = null;
                return false;
            }
        }


        public bool PPIDModeIsValid(out string reasonText)
        {
            reasonText = string.Empty;
            if (startupMode == StartupMode.Automatic && ppidMode == PPIDMode.AcquireFromUI)
            {
                reasonText = "If startup mode is Automatic, you cannot use PPID (Participant ID) mode: Acquire With UI.";
                return false;
            }

            return true;
        }


        public bool SettingsModeIsValid(out string reasonText)
        {
            reasonText = string.Empty;
            if (startupMode == StartupMode.Automatic && settingsMode == SettingsMode.AcquireFromUI)
            {
                reasonText = "If startup mode is set to Automatic, you cannot use session settings mode: Acquire With UI.";
                return false;
            }

            return true;
        }

        public bool LocalPathStateIsValid(out string reasonText)
        {
            reasonText = string.Empty;
            if (startupMode == StartupMode.Automatic && RequiresFilePathElement)
            {
                reasonText = "A DataHander has been set to acquire a local data save path from the UI. This is not possible if the startup mode is not set to use the Built-in UI. You can change the Data Save Location to Fixed in the relevant data handlers if you wish to not use the Built-in UI.";
                return false;
            }

            return true;
        }

        public bool DatapointsAreValid(out string reasonText)
        {
            reasonText = string.Empty;

            var numEmpties = participantDataPoints
                .Select(pdp => pdp.internalName.Replace(" ", ""))
                .Where(internalNameNoSpaces => internalNameNoSpaces == string.Empty)
                .Count();

            if (numEmpties == 1)
            {
                reasonText = "In the items in the Participant Datapoints list, there is 1 item with an empty internal name. Please enter an internal name for this item.";
                return false;
            }
            else if (numEmpties > 1)
            {
                reasonText = string.Format("In the items in the Participant Datapoints list, there are {0} items with empty internal names. Please enter an internal name for these items.", numEmpties);
                return false;
            }

            var duplicates = participantDataPoints
                .GroupBy(pdp => pdp.internalName)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                reasonText = "There are duplicated internal names in the Participant Datapoints List ('"
                    + string.Join(", ", duplicates)
                    + "').";
                return false;
            }

            return true;
        }

        IEnumerator GetJsonUrl()
        {
            UnityWebRequest www = UnityWebRequest.Get(jsonURL);
            www.timeout = 5;
            yield return www.SendWebRequest();

            bool error;
#if UNITY_2020_OR_NEWER
            error = www.result != UnityWebRequest.Result.Success;
#else
#pragma warning disable
            error = www.isHttpError || www.isNetworkError;
#pragma warning restore
#endif

            if (error)
            {
                Utilities.UXFDebugLogError(www.error);
                yield break;
            }
            jsonText = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
        }
    }


    public enum StartupMode
    {
        BuiltInUI, Automatic, Manual
    }

    public enum SettingsMode
    {
        AcquireFromUI, DownloadFromURL, Empty
    }

    public enum PPIDMode
    {
        AcquireFromUI, GenerateUnique
    }

    public enum SessionNumMode
    {
        AcquireFromUI, AlwaysSession1
    }
}
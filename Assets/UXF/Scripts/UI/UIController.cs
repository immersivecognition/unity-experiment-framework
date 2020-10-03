using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SubjectNerd.Utilities;

namespace UXF.UI
{
    [DefaultExecutionOrder(100)]
    public class UIController : MonoBehaviour
    {
        [Tooltip("TODO")]
        public StartupMode startupMode = StartupMode.BuiltInUI;

        [Tooltip("TODO")]
        public string experimentName = "my_experiment";

        [Tooltip("TODO")]
        public SessionSettingsMode settingsMode = SessionSettingsMode.AcquireFromUI;

        [Tooltip("TODO")]
        public string settingsSearchPattern = "*.json";

        [Tooltip("TODO")]
        public string jsonURL = "https://gist.githubusercontent.com/jackbrookes/0f9770fcfe3d448e0f7a1973c2ac7419/raw/f2d234c92c77a817f9fc6390fcfcb39814c33d3c/example_settings.json";

        [Tooltip("How should the Participant ID be collected?")]
        public PPIDMode ppidMode = PPIDMode.AcquireFromUI;

        [Tooltip("Assign to a .txt file that contains one word per line. A random item in the list will be used for generating the PPID.")]
        public TextAsset uuidWordList;

        [SubjectNerd.Utilities.Reorderable]
        public List<FormElementEntry> participantDataPoints = new List<FormElementEntry>();

        [Tooltip("The text shown next to the terms and conditions checkbox.")]
        [TextArea]
        public string termsAndConditions = "Please tick if you understand the instructions and agree for your data to be collected and used for research purposes.<color=red>*</color>";

        [Tooltip("Should the terms and conditions checkbox be pre-ticked?")]
        public bool tsAndCsInitialState = false;

        public bool RequiresFilePathElement
        {
            get {
                return session.ActiveDataHandlers
                    .Where((dh) => dh is LocalFileDataHander)
                    .Any(dh => ((LocalFileDataHander)dh).dataSaveLocation == DataSaveLocation.AcquireFromUI);
            }
        }
        
        public IEnumerable<LocalFileDataHander> ActiveLocalFileDataHanders
        {
            get {
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

# region HIDDEN_VARIABLES
        public Transform instructionsContentTransform;
        public Transform sidebarContentTransform;
        public FormElement settingsElement;
        public FormElement localFilePathElement;
        public FormElement ppidElement;
        public FormElement tsAndCsToggle;
        public FormElement textPrefab;
        public FormElement dropDownPrefab;
        public FormElement checkBoxPrefab;
        private Session session;
        private string[] words;
# endregion

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            if (session == null) session = GetComponentInParent<Session>();
            if (tsAndCsToggle != null)
            {
                tsAndCsToggle.title.text = termsAndConditions;
                tsAndCsToggle.SetContents(tsAndCsInitialState);
            }
            foreach (var dh in ActiveLocalFileDataHanders) dh.onValidateEvent.AddListener(UpdateLocalFileElementState);
        }

        void UpdateLocalFileElementState()
        {
            if (localFilePathElement != null) localFilePathElement.gameObject.SetActive(RequiresFilePathElement);
        }


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
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
            UpdateLocalFileElementState();
            if (startupMode == StartupMode.Automatic) AutoBeginSession();
        }


        public void AutoBeginSession()
        {
            session.Begin(
                experimentName, 
                GenerateUniquePPID()
            );
        }


        public void TryBeginSessionFromUI()
        {
            // TERMS AND CONDITIONS
            bool acceptedTsAndCs = (bool) tsAndCsToggle.GetContents();
            if (!acceptedTsAndCs)
            {
                tsAndCsToggle.DisplayFault();
                return;
            }

            // EXPERIMENT NAME
            string newExperimentName;
            switch (settingsMode)
            {
                case SessionSettingsMode.AcquireFromUI:
                    newExperimentName = settingsElement
                        .GetContents()
                        .ToString()
                        .Replace(".json", "");
                    break;
                case SessionSettingsMode.DownloadFromURL:
                case SessionSettingsMode.Empty:
                    newExperimentName = experimentName;
                    break;
                default:
                    throw new Exception();
            }

            // PPID
            string newPpid = "";
            switch (ppidMode)
            {
                case PPIDMode.AcquireFromUI:
                    newPpid = ppidElement
                        .GetContents()
                        .ToString()
                        .Trim();
                    if (newPpid == string.Empty) ppidElement.DisplayFault();
                    break;
                case PPIDMode.GenerateUnique:
                    newPpid = GenerateUniquePPID();
                    break;
                default:
                    throw new Exception();
            }
            

            // SESSION NUM
            int sessionNum = 1; // TODO get session num here

            // PARTICIPANT DETAILS
            Dictionary<string, object> newParticipantDetails;
            var validityList = SidebarStateIsValid(out newParticipantDetails);
            bool sidebarValid = true;
            foreach (var v in validityList)
            {
                sidebarValid = sidebarValid && v.valid;
                if (!v.valid && v.entry.element != null) v.entry.element.DisplayFault();
            }
            if (!sidebarValid) return;

            // SETTINGS
            Settings newSettings = null;
            switch (settingsMode)
            {
                case SessionSettingsMode.AcquireFromUI:
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
                        return;
                    }
                    Dictionary<string, object> deserializedJson = (Dictionary<string, object>) MiniJSON.Json.Deserialize(settingsText);
                    if (deserializedJson == null)
                    {
                        Debug.LogErrorFormat("Cannot deserialize json file: {0}.", settingsPath);
                        settingsElement.DisplayFault();
                        return;
                    }
                    else
                    {
                        newSettings = new Settings(deserializedJson);
                    }
                    break;
                case SessionSettingsMode.DownloadFromURL:
                    return;
                case SessionSettingsMode.Empty:
                    newSettings = Settings.empty;
                    break;
                default:
                    throw new Exception();
            }

            // DATA PATH
            if (RequiresFilePathElement)
            {
                if (!localFilePathElement.gameObject.activeSelf)
                {
                    Debug.LogError("Cannot start session - need Local File Path element, but it is not active.");
                    return;
                }

                string localFilePath = (string) localFilePathElement.GetContents();
                foreach (var dh in ActiveLocalFileDataHanders)
                {
                    dh.storagePath = localFilePath;
                }
            }            

            // BEGIN!
            session.Begin(
                newExperimentName,
                newPpid,
                sessionNum,
                newParticipantDetails,
                newSettings
            );
        }

        public string GenerateUniquePPID()
        {
            string prefix = string.Empty;
            if (words != null) prefix = words[UnityEngine.Random.Range(0, words.Length - 1)] + "-";
            string ppid = Guid.NewGuid().ToString();
            return Extensions.GetSafeFilename(ppid);            
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
                else DestroyImmediate(child.gameObject);
            }

            foreach (FormElementEntry entrey in participantDataPoints)
            {
                FormElement newElement = null;
                switch (entrey.dataType)
                {
                    case FormDataType.String:
                    case FormDataType.Int:
                    case FormDataType.Float:
                        newElement = Instantiate(textPrefab, sidebarContentTransform);
                        break;
                    case FormDataType.Bool:
                        newElement = Instantiate(checkBoxPrefab, sidebarContentTransform);
                        break;
                    case FormDataType.DropDown:
                        newElement = Instantiate(dropDownPrefab, sidebarContentTransform);
                        break;
                }
                entrey.element = newElement;
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
            if (startupMode == StartupMode.Automatic && settingsMode == SessionSettingsMode.AcquireFromUI)
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
                reasonText = "A DataHander has been set to acquire a local data save path from the UI. This is not possible if the startup mode is not set to use the Built-in UI.";
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

    }


    public enum StartupMode
    {
        BuiltInUI, Automatic, Manual 
    }

    public enum SessionSettingsMode
    {
        AcquireFromUI, DownloadFromURL, Empty 
    }

    public enum PPIDMode
    {
        AcquireFromUI, GenerateUnique
    }
}
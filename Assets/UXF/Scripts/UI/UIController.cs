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
    public class UIController : MonoBehaviour
    {
        [Tooltip("TODO")]
        public StartupMode startupMode = StartupMode.BuiltInUI;

        [Tooltip("TODO")]
        public SessionSettingsMode settingsMode = SessionSettingsMode.SelectWithUI;

        [Tooltip("TODO")]
        public string settingsSearchPattern = "*.json";

        [Tooltip("TODO")]
        public string jsonURL = "https://gist.githubusercontent.com/jackbrookes/0f9770fcfe3d448e0f7a1973c2ac7419/raw/f2d234c92c77a817f9fc6390fcfcb39814c33d3c/example_settings.json";

        [Tooltip("TODO")]
        public bool showPPIDElement = true;

        [SubjectNerd.Utilities.Reorderable]
        public List<FormElementEntry> participantDataPoints = new List<FormElementEntry>();

        [Tooltip("The text shown next to the terms and conditions checkbox.")]
        public string termsAndConditions = "Please tick if you understand the instructions and agree for your data to be collected and used for research purposes.<color=red>*</color>";

        [Tooltip("Should the terms and conditions checkbox be pre-ticked?")]
        public bool tsAndCsInitialState = false;
        
# region HIDDEN_VARIABLES
        public GameObject instructionsContentGameObject;
        public Text tsAndCsText;
        public Toggle tsAndCsToggle;
        private Session session;
# endregion

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            if (session == null) session = GetComponentInParent<Session>();

            tsAndCsText.text = termsAndConditions;
            tsAndCsToggle.isOn = tsAndCsInitialState;
        }


        public bool SettingsAreCompatible(out string reasonText)
        {
            reasonText = string.Empty;
            if (startupMode != StartupMode.BuiltInUI && settingsMode == SessionSettingsMode.SelectWithUI)
            {
                reasonText = "If startup mode is not set to the Built-in UI, you cannot use session settings mode: Select With UI.";
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
        SelectWithUI, DownloadFromURL, None 
    }
}
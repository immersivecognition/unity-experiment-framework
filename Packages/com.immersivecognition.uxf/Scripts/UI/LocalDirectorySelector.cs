using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    public class LocalDirectorySelector : MonoBehaviour
    {
        private bool selectingFolder;
        private int folderSelectionRequest;
        
        public FormElement inputField;
        UIController uiController;
        string dataLocKey = "uxf_datalocation";

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            uiController = GetComponentInParent<UIController>();
        }


        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            if (PlayerPrefs.HasKey(dataLocKey))
            {
                string loc = PlayerPrefs.GetString(dataLocKey);
                if (Directory.Exists(loc))
                {
                    inputField.SetContents(loc);
                }
            }
        }

        public void SelectFolder()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (selectingFolder) return;
            selectingFolder = true;
            int request = ++folderSelectionRequest;
            StartCoroutine(SelectFolderAfterClick(request));
#else
            Utilities.UXFDebugLogError("Cannot select directory unless on PC platform!");
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private IEnumerator SelectFolderAfterClick(int request)
        {
            // Let the button's OnPointerClick finish before opening the dialog.
            yield return null;
            if (!isActiveAndEnabled || request != folderSelectionRequest)
            {
                selectingFolder = false;
                yield break;
            }

            string current = (string)inputField.GetContents();
            ShowFolderPanelAsync("Select data directory", current, false, selected =>
            {
                if (request != folderSelectionRequest) return;
                try
                {
                    if (isActiveAndEnabled && selected != null && selected.Length > 0)
                        inputField.SetContents(selected[0]);
                }
                finally
                {
                    selectingFolder = false;
                }
            });
        }

        protected virtual void ShowFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> callback)
        {
            SFB.StandaloneFileBrowser.OpenFolderPanelAsync(title, directory, multiselect, callback);
        }

        private void OnDisable()
        {
            // Ignore a pending native-dialog callback after this selector is disabled.
            folderSelectionRequest++;
            selectingFolder = false;
        }
#endif


        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        void OnDestroy()
        {
            string current = (string) inputField.GetContents();
            if (Directory.Exists(current))
            {
                PlayerPrefs.SetString(dataLocKey, current);
            }
        }

    }

}

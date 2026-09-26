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
    public class ExperimentProfileSelector : MonoBehaviour
    {
        
        public FormElement dropdown;
        UIController uiController;
        string profileKey = "uxf_profile";

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
            Populate();
        }


        public void Populate(bool retry = true)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Utilities.UXFDebugLogWarning("StreamingAssets folder was moved or deleted! Creating a new one.");
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            var profileNames = Directory.GetFiles(Application.streamingAssetsPath, uiController.settingsSearchPattern)
                .Select(f => Path.GetFileName(f))
                .ToList();

            if (profileNames.Count > 0)
            {
                if (PlayerPrefs.HasKey(profileKey))
                {
                    string profile = PlayerPrefs.GetString(profileKey);
                    if (profileNames.Contains(profile))
                    {
                        int profileIdx = profileNames.IndexOf(profile);
                        Tuple<int, IEnumerable<string>> data = new Tuple<int, IEnumerable<string>>(
                            profileIdx, profileNames
                        );

                        dropdown.SetContents(data); // pass tuple
                    }
                    else
                    {
                        dropdown.SetContents(profileNames);
                    }
                }
                else
                {
                    dropdown.SetContents(profileNames);
                }
            }
            else
            {
                string newName = uiController.settingsSearchPattern.Replace("*", "my_experiment");
                string newPath = Path.Combine(Application.streamingAssetsPath, newName);
                if (!File.Exists(newPath))
                {
                    File.WriteAllText(newPath, "{\n}");
                    Utilities.UXFDebugLogErrorFormat("No profiles found in {0} that match search pattern {1}. A blank one called {2} has been made for you.", Application.streamingAssetsPath, uiController.settingsSearchPattern, newName);
                    if (retry) Populate(retry: false);
                }
            }
        }


        public void ShowFolder()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            string winPath = Application.streamingAssetsPath.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer.exe", "/root," + winPath);
#else
            Utilities.UXFDebugLogError("Cannot open directory unless on PC platform!");
#endif
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        void OnDestroy()
        {
            string current = (string) dropdown.GetContents();
            PlayerPrefs.SetString(profileKey, current);
        }

    }

}
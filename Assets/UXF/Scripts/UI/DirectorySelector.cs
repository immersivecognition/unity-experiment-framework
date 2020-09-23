using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System;

namespace UXF.UI
{
    public class DirectorySelector : MonoBehaviour
    {
        [ReadOnly]
        public string currentDirectory;
        public Text dirNameDisplay;
        public Button startButton;
        public Button openFolder;


        string directoryLocKey = "DirectoryLocation";

        public void Init()
        {
            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, false);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(openFolder.gameObject, false);
            SetFromCache();
        }

        public void SetFromCache()
        {            
            if (PlayerPrefs.HasKey(directoryLocKey))
            {
                string loc = PlayerPrefs.GetString(directoryLocKey);
                if (Directory.Exists(loc))
                {
                    UpdateDirectory(loc);
                }
                else
                {
                    PlayerPrefs.DeleteKey(directoryLocKey);
                }
            }
        }
        

        public void SelectDirectory()
        {
            SFB.StandaloneFileBrowser.OpenFolderPanelAsync("Select data save directory", currentDirectory, false, (paths) => {
                if (paths == null) return;
                if (paths.Length == 0) return;
                UpdateDirectory(paths[0]);
            });
        }


        void UpdateDirectory(string newDirectory)
        {
            currentDirectory = newDirectory;

            PlayerPrefs.SetString(directoryLocKey, newDirectory);

            dirNameDisplay.text = string.Format("Selected directory:\n{0}", currentDirectory);
            dirNameDisplay.color = Color.black;

            List<string> participants = Directory.GetDirectories(newDirectory)
                .SelectMany((experimentDir) => Directory.GetDirectories(experimentDir))
                .Select((participantDir) => {
                    string[] splitPath = participantDir.Split(Path.DirectorySeparatorChar);
                    return string.Format("{0}/{1}", splitPath[splitPath.Length - 2], splitPath[splitPath.Length - 1]);              
                    })
                .ToList();
            
            Debug.Log(participants);

            ExperimentStartupController.SetSelectableAndChildrenInteractable(startButton.gameObject, true);
            ExperimentStartupController.SetSelectableAndChildrenInteractable(openFolder.gameObject, true);
        }

        public void OpenDataFolder()
		{
			string winPath = currentDirectory.Replace("/", "\\");
			System.Diagnostics.Process.Start("explorer.exe", "/root," + winPath);
		}


        public void UpdateDatapoint(string ppid, string datapointName, object value)
        {
            throw new NotSupportedException("Participant list system no longer supported.");
        }

        public void CommitCSV()
        {
            throw new NotSupportedException("Participant list system no longer supported.");
        }

        public void UpdateFormByPPID(string ppid)
        {
            throw new NotSupportedException("Participant list system no longer supported.");
        }

    }
}
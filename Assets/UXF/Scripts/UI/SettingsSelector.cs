using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UXF{
	public class SettingsSelector : MonoBehaviour {

		public DropDownController ddController;
		public Session session;
		public PopupController popupController;
		public ExperimentStartupController startupController;
		List<string> settingsNames;
		public string settingsFolder;
		Dictionary<string, object> settingsDict;
		[HideInInspector]
		public string experimentName;

		string settingsFileKey = "SettingsFile";

		void Start ()
		{
			settingsFolder = Application.streamingAssetsPath;
			TryGetSettingsList();
			SetFromCache();
		}
		

		void TryGetSettingsList()
		{
			settingsNames = Directory.GetFiles(settingsFolder, startupController.settingsSearchPattern)
                                .Select(f => Path.GetFileName(f))
                                .ToList();

            if (settingsNames.Count > 0)
            {
                ddController.SetItems(settingsNames);
            }
            else
            {
                Popup settingsError = new Popup();
                settingsError.messageType = MessageType.Error;
                settingsError.message = string.Format(
					"No settings files found at {0} that match pattern {1}. Please create at least one json-encoded file containing your settings.",
					settingsFolder,
					startupController.settingsSearchPattern);
                settingsError.onOK = new System.Action(() => {TryGetSettingsList();});
                popupController.DisplayPopup(settingsError);
            }

		}

	    void SetFromCache()
        {            
            if (PlayerPrefs.HasKey(settingsFileKey))
            {
                string fname = PlayerPrefs.GetString(settingsFileKey);
				string settingsPath = Path.Combine(settingsFolder, fname);
				if (File.Exists(settingsPath) && ddController.optionNames.Contains(fname))
				{
	                ReadSettingsDict(fname);
	            	experimentName = Path.GetFileNameWithoutExtension(fname);
					ddController.SetContents(fname);
				}
				else
				{
					LoadCurrentSettingsDict();
				}
            }
			else
			{
				LoadCurrentSettingsDict();
			}
        }

		public void LoadCurrentSettingsDict()
		{
			string fname = ddController.GetContents().ToString();
			ReadSettingsDict(fname);
            experimentName = Path.GetFileNameWithoutExtension(fname);
        }

        void ReadSettingsDict(string fname)
        {
			string settingsPath = Path.Combine(settingsFolder, fname);
			PlayerPrefs.SetString(settingsFileKey, fname);
            session.ReadSettingsFile(settingsPath, new System.Action<Dictionary<string, object>>((dict) => HandleSettingsDict(dict)));
        }

		void HandleSettingsDict(Dictionary<string, object> dict)
		{
			settingsDict = dict;
		}

		public Settings GetSettings()
		{
			return new Settings(settingsDict);
		}

	}
}
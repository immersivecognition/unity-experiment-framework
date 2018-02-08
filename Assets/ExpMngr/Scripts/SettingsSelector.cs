using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpMngr{
	public class SettingsSelector : MonoBehaviour {

		public DropDownController ddController;
		public ExperimentSession session;
		List<string> settingsNames;
		string settingsFolder;
		Dictionary<string, object> settingsDict;
		[HideInInspector]
		public string experimentName;

		// Use this for initialization
		void Start () {
			settingsFolder = Application.streamingAssetsPath;

			settingsNames = Directory.GetFiles(settingsFolder, "*.json")
								.ToList()
								.Select(f => Path.GetFileName(f))
								.ToList();

			ddController.SetItems(settingsNames);

			LoadCurrentSettingsDict();
		}
		

		public void LoadCurrentSettingsDict()
		{
			string fname = ddController.GetContents().ToString();
			ReadSettingsDict(fname);
            experimentName = Path.GetFileNameWithoutExtension(fname);
        }

        void ReadSettingsDict(string settingsName)
        {
			string settingsPath = Path.Combine(settingsFolder, settingsName);
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
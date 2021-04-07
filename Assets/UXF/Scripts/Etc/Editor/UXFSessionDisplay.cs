using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UXF;

namespace UXF.EditorUtils
{

    [InitializeOnLoad]
    public class UXFSessionDisplay : EditorWindow
    {
        static Vector2 scrollPos;
        static Session session;
        static Dictionary<string, object> settingsDict;
		static bool parsed;


        [MenuItem("UXF/Show session debugger")]
        static void Init()
        {
            FetchReferences();
            var window = (UXFSessionDisplay)EditorWindow.GetWindow(typeof(UXFSessionDisplay));
            window.minSize = new Vector2(300, 500);
            window.titleContent = new GUIContent("UXF Session Debugger");
            window.Show();
        }

		static UXFSessionDisplay()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode)
			{
                FetchReferences();
            }
		}

		static void FetchReferences()
		{
            session = FindObjectOfType<Session>();
            if (!session)
			{
                settingsDict = null;
				return;
			}
            if (session.settings == null)
            {
                settingsDict = null;
                return;
            }

            settingsDict = new Dictionary<string, object>();
            // log each session setting
            foreach (string key in session.settings.Keys)
                settingsDict.Add(key, session.settings.GetObject(key).ToString());

            List<Dictionary<string, object>> blockList = new List<Dictionary<string, object>>();
            foreach (var block in session.blocks)
            {
                Dictionary<string, object> blockDict = new Dictionary<string, object>();
                // log each block setting
                foreach (string key in block.settings.Keys)
                    blockDict.Add(key, block.settings.GetObject(key).ToString());
				
				List<Dictionary<string, object>> trialList = new List<Dictionary<string, object>>();
                // log each trial
                foreach (var trial in block.trials)
                {
                    Dictionary<string, object> trialDict = new Dictionary<string, object>();
                    // log each trial setting
                    foreach (string key in trial.settings.Keys)
                    {
                        var val = trial.settings.GetObject(key);
                        trialDict.Add(key, val?.ToString() ?? "(null)");
                    }

                    // add trial to block
                    trialList.Add(trialDict);
                }
				blockDict.Add("_____trials", trialList);
                blockList.Add(blockDict);
            }
            settingsDict.Add("_____blocks", blockList);
		}

        public void OnGUI()
        {
            EditorGUILayout.Space();
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.wordWrap = true;
            labelStyle.margin = new RectOffset(6, 6, 0, 0);

            GUILayout.Label("Press the button below after generating your blocks and trials to list all blocks and trials with their associated setting in the session. This helps you make sure settings are being applied to the correct trials. ", labelStyle);
            if (GUILayout.Button("Fetch session info"))
            {
                FetchReferences();
            }

			if (settingsDict != null)
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				ParseSettings();
                EditorGUILayout.EndScrollView();
				return;
			}
			else if (!session)
            {
                EditorGUILayout.HelpBox("Did not find a UXF.Session component in the scene! Press button above to try again.", UnityEditor.MessageType.Error);
                return;
            }
            else if (!session.hasInitialised)
            {
                EditorGUILayout.HelpBox("Session has not yet started! This debugging tool can be used only when the session has started. Press button above to try again.", UnityEditor.MessageType.Warning);
                return;
            }
            GUILayout.Label("Something went wrong.", labelStyle);
            return;	           

        }


        static void ParseSettings()
        {
            EditorGUILayout.HelpBox("Remember, Settings requests cascade upwards: That means accessing a settings in a trial will first look inside the trial, if it is then not found, will look inside the block, then the session.", UnityEditor.MessageType.Info);
			
            GUILayout.Label("Session .settings", EditorStyles.boldLabel);

            // add more info...
            EditorGUILayout.BeginVertical("box");

            // log each session setting
            GUIKeyValuePairColumns(settingsDict);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");

			var blockList = (List<Dictionary<string, object>>) settingsDict["_____blocks"];

			int b = 0;
            foreach (var block in blockList)
            {
                GUILayout.Label(string.Format("Block {0} .settings", ++b), EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                GUIKeyValuePairColumns(block);
                EditorGUILayout.Space();

                var trialList = (List<Dictionary<string, object>>) block["_____trials"];
				
				int t = 0;
                foreach (var trial in trialList)
                {
                    GUILayout.Label(string.Format("Trial {0} .settings", ++t), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    GUIKeyValuePairColumns(trial);

                    EditorGUILayout.EndVertical();
                }


                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

        }

        static void GUIKeyValuePairColumns(Dictionary<string, object> dict)
        {
            if (dict.Count == 0)
            {
                EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
                return;
            }

            foreach (KeyValuePair<string, object> pair in dict)
            {
                string k = pair.Key;
                if (k.StartsWith("_____")) continue;

				GUILayout.BeginHorizontal();
                string v;
                if (pair.Value == null)
                {
                    v = "null";
                }
                else
                {
                    v = Truncate(pair.Value.ToString(), 100);
                }
                EditorGUILayout.LabelField(string.Format("[\"{0}\"]: {1}", k, v));
                GUILayout.EndHorizontal();
            }

        }


        static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }

}
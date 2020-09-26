using UnityEditor;
using UnityEngine;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(Session), true)]
    [CanEditMultipleObjects]
    public class SessionEditor : SubjectNerd.Utilities.ReorderableArrayInspector
    {
        Session session;
        
        int tabSelection = 0;
        string[] tabTexts = new string[]{ "Behaviour", "Data logging", "Events", "Monitor" };

        protected override void InitInspector()
        {
            base.InitInspector();
		
            // Always call DrawInspector function
            alwaysDrawInspector = true;
            session = (Session) target;

            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        Object obj;

        protected override void DrawInspector()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
			{
				EditorGUILayout.ObjectField(
                    "Script",
                    MonoScript.FromMonoBehaviour(session),
                    typeof(MonoScript),
                    false
                );
			}
			EditorGUI.EndDisabledGroup();
            EditorGUILayout.Separator();
            
            tabSelection = GUILayout.Toolbar(tabSelection, tabTexts);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(":: ", EditorStyles.boldLabel);
            GUILayout.Label(tabTexts[tabSelection], EditorStyles.boldLabel);
            GUILayout.Label(" ::", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            switch (tabSelection)
            {
                case 0:
                    DrawPropertiesFromUpTo("endOnQuit", "copySessionSettings");
                    break;
                case 1:
                    EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);                
                    DrawPropertiesFromUpTo("copySessionSettings", "onSessionBegin");
                    EditorGUI.EndDisabledGroup();
                    break;
                case 2:
                    DrawPropertiesFromUpTo("onSessionBegin", "_hasInitialised");
                    break;
                case 3:
                    DrawMonitorTab();
                    break;
            }

            serializedObject.ApplyModifiedProperties();          
        }

        void DrawMonitorTab()
        {

            if (EditorApplication.isPlaying && session.hasInitialised)
            {

                GUILayout.Label("Currently running session!");
                EditorGUILayout.LabelField("Experiment name", session.experimentName);
                EditorGUILayout.LabelField("PPID", session.ppid);
                EditorGUILayout.LabelField("Session number", session.FolderName);
                EditorGUILayout.Separator();

                GUILayout.Label("Full path");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(session.FullPath);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Separator();

                int currentBlockNum = serializedObject.FindProperty("currentBlockNum").intValue;
                int maxBlockNum = session.blocks[session.blocks.Count - 1].number;
                string blockProgressText = string.Format("Block {0}/{1}", currentBlockNum, maxBlockNum);
                float blockProgress = (float) currentBlockNum / maxBlockNum;

                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, blockProgress, blockProgressText);
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();


                int currentTrialNum = serializedObject.FindProperty("currentTrialNum").intValue;
                int maxTrialNum = session.LastTrial.number;
                string trialProgressText = string.Format("Trial {0}/{1}", currentTrialNum, maxTrialNum);
                float trialProgress = (float) currentTrialNum / maxTrialNum;

                r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, trialProgress, trialProgressText);
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("You can monitor the progress of the Session here.\nWaiting for Session to begin...", MessageType.Info);
            }

        }

        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                tabSelection = 3;
            }
        }

    }

}


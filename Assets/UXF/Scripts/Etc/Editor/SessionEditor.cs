using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(Session), true)]
    [CanEditMultipleObjects]
    public class SessionEditor : SubjectNerd.Utilities.ReorderableArrayInspector
    {
        Session session;
        
        static int tabSelection;
        string[] tabTexts = new string[]{ "Behaviour", "Data collection", "Data handling", "Events" };


        protected override void InitInspector()
        {
            base.InitInspector();

            // Always call DrawInspector function
            alwaysDrawInspector = true;
            session = (Session) target;
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



            Rect rectBox = EditorGUILayout.BeginVertical("Box");
            float lw = EditorGUIUtility.labelWidth;

            switch (tabSelection)
            {
                case 0:
                    EditorGUIUtility.labelWidth = rectBox.width - 36f;
                    EditorGUI.indentLevel++;
                    DrawPropertiesFromUpTo("endOnQuit", "setAsMainInstance");
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle(new GUIContent("Ad Hoc Header Add", "Now permanently enabled. Results that are not listed in Custom Headers can be added at any time. If disabled, adding results that are not listed in Custom Headers will throw an error."), false);
                    EditorGUI.EndDisabledGroup();
                    DrawPropertiesFromUpTo("setAsMainInstance", "storeSessionSettings");
                    EditorGUIUtility.labelWidth = lw;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(16);
                    GUILayout.Label("User Interface");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Configure"))
                    {
                        Object ui = session.GetComponentInChildren<UI.UIController>();
                        Selection.activeObject = ui;
                        InternalEditorUtility.SetIsInspectorExpanded(ui, true);
                    }
                    GUILayout.Space(14);
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                    GUILayout.Space(3);
                    break;
                case 1:
                    EditorGUIUtility.labelWidth = rectBox.width - 36f;
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(session.hasInitialised);                
                    DrawPropertiesFromUpTo("storeSessionSettings", "customHeaders");
                    EditorGUIUtility.labelWidth = lw;
                    DrawPropertiesFromUpTo("customHeaders", "onSessionBegin");
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                    break;
                case 2:
                    EditorGUILayout.HelpBox(
                        "The Data Handlers below define how your data will be stored."
                        + "You can select which Data Handlers will be used."
                        + "You may need to activate/deactivate different handlers depending on your build target (Windows, Web, Quest).", MessageType.Info);
                    EditorGUI.indentLevel++;
                    DrawPropertiesFrom("dataHandlers");
                    EditorGUI.indentLevel--;
                    break;
                case 3:
                    EditorGUILayout.HelpBox("These events are raised when the session/trial begins/ends. Use them to manipulate the scene to create your experiment manipulation.", MessageType.Info);
                    DrawPropertiesFromUpTo("onSessionBegin", "_hasInitialised");                    
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            DrawMonitorTab();
            //GUILayout.Button("Create a GitHub issue");
            //GUILayout.Button("Get help on the Wiki");

            serializedObject.ApplyModifiedProperties();          
        }

        void DrawMonitorTab()
        {

            if (EditorApplication.isPlaying && session.hasInitialised)
            {

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Currently running session!");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Experiment name:", session.experimentName);
                EditorGUILayout.LabelField("PPID:", session.ppid);
                EditorGUILayout.LabelField("Session number:", session.number.ToString());
                EditorGUILayout.Separator();

                // GUILayout.Label("Full path:");
                // EditorGUI.BeginDisabledGroup(true);
                // EditorGUILayout.TextArea(session.FullPath);
                // EditorGUI.EndDisabledGroup();
                // EditorGUILayout.Separator();

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

    }

}


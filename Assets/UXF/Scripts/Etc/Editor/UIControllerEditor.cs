using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UXF.UI;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(UIController), true)]
    [CanEditMultipleObjects]
    public class UIControllerEditor : SubjectNerd.Utilities.ReorderableArrayInspector
    {
        UIController uiController;
        
        static int tabSelection;
        static string[] tabTexts = new string[]{ "Startup", "Datapoints", "Instructions" };
        static Dictionary<StartupMode, string> startupModeDescriptionMapping = new Dictionary<StartupMode, string>()
        {
            { StartupMode.BuiltInUI, "The user interface will display before the session starts, allowing the researcher or participant to enter some basic details, read the instructions, and start the session." },
            { StartupMode.Automatic, "There will be no user interface displayed, and the session will start immediately and automatically, with an optional settings .json file (stored in the StreamingAssets path)." },
            { StartupMode.Manual, "The session will not start automatically - you must start it manually with code. You can start a session by calling the session.Begin() method, supplying the session information." }
        };

        protected override void InitInspector()
        {
            base.InitInspector();

            // Always call DrawInspector function
            alwaysDrawInspector = true;
            uiController = (UIController) target;
        }

        protected override void DrawInspector()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
			{
				EditorGUILayout.ObjectField(
                    "Script",
                    MonoScript.FromMonoBehaviour(uiController),
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
                    DrawPropertiesFromUpTo("startupMode", "settingsMode");
                    EditorGUILayout.HelpBox(uiController.startupMode.ToString() + ": " + startupModeDescriptionMapping[uiController.startupMode], UnityEditor.MessageType.Info);
                    EditorGUILayout.Separator();
                    
                    DrawPropertiesFromUpTo("settingsMode", "aaa");
                    break;
                case 1:
                    GUILayout.Space(3);
                    break;
                case 2:
                    GUILayout.Space(3);
                    break;
                case 3:
                    GUILayout.Space(3);
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();          
        }

    }

}


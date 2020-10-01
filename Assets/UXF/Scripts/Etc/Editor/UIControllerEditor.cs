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
        static string[] tabTexts = new string[] { "Startup", "Datapoints", "Instructions" };
        static Dictionary<StartupMode, string> startupModeDescriptionMapping = new Dictionary<StartupMode, string>()
        {
            { StartupMode.BuiltInUI, "The user interface will display before the session starts, allowing the researcher or participant to enter some basic details, read the instructions, and start the session." },
            { StartupMode.Automatic, "There will be no user interface displayed, and the session will start immediately and automatically, with an optional settings .json file (stored in the StreamingAssets path)." },
            { StartupMode.Manual, "The session will not start automatically - you must start it manually with code. You can start a session by calling the session.Begin() method, supplying the session information." }
        };
        static Dictionary<SessionSettingsMode, string> settingsModeDescriptionMapping = new Dictionary<SessionSettingsMode, string>()
        {
            { SessionSettingsMode.SelectWithUI, "TODO" },
            { SessionSettingsMode.DownloadFromURL, "TODO" },
            { SessionSettingsMode.None, "TODO" }
        };

        protected override void InitInspector()
        {
            base.InitInspector();

            // Always call DrawInspector function
            alwaysDrawInspector = true;
            uiController = (UIController)target;
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

            if (tabSelection != 0 && uiController.startupMode != StartupMode.BuiltInUI) DrawUIDisabledMessage();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.Separator();
            float lw = EditorGUIUtility.labelWidth;

            switch (tabSelection)
            {
                case 0:
                    string reasonText;
                    if (!uiController.SettingsAreCompatible(out reasonText))
                    {
                        string errorText = "Incompatibility Error: " + reasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }

                    this.DrawProperty("startupMode");
                    EditorGUILayout.HelpBox(uiController.startupMode.ToString() + ": " + startupModeDescriptionMapping[uiController.startupMode], UnityEditor.MessageType.Info);
                    EditorGUILayout.Separator();

                    this.DrawProperty("settingsMode");
                    EditorGUILayout.HelpBox(uiController.settingsMode.ToString() + ": " + settingsModeDescriptionMapping[uiController.settingsMode], UnityEditor.MessageType.Info);
                    switch (uiController.settingsMode)
                    {
                        case SessionSettingsMode.SelectWithUI:
                            this.DrawProperty("settingsSearchPattern");
                            break;
                        case SessionSettingsMode.DownloadFromURL:
                            if (MiddleButton("Test URL")) throw new NotImplementedException("TODO");
                            this.DrawProperty("jsonURL");
                            break;
                        case SessionSettingsMode.None:
                            break;
                    }
                    EditorGUILayout.Separator();
                    break;
                case 1:
                    EditorGUILayout.HelpBox(
                        "Below you can enter a series of datapoints to be collected about a participant" +
                        "- such as their age, gender, or even things that could affect the task such as " +
                        "the preferred hand of the participant. Press the Generate button to update the UI " +
                        "to reflect your items.", UnityEditor.MessageType.Info);
                    bool valid = uiController.DatapointsAreValid(out reasonText);
                    if (!valid)
                    {
                        string errorText = "Participant Datapoints Error: " + reasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }
                    if (MiddleButton("Generate", enabled: valid)) throw new NotImplementedException("TODO");
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel++;
                    this.DrawProperty("showPPIDElement");
                    DrawPropertiesFromUpTo("participantDataPoints", "termsAndConditions");
                    EditorGUI.indentLevel--;
                    break;
                case 2:
                    EditorGUILayout.HelpBox(
                        "You can edit the content that displays in the right side of the UI. This can " +
                        "be used for instructions to the participant or researcher on how the study should " +
                        "be performed. You can add any UI elements such as text, images, or even buttons to  " +
                        "the Instructions Panel Content GameObject. You can select it with the button below.", UnityEditor.MessageType.Info);
                    if (MiddleButton("Select Content GameObject"))
                    {
                        if (uiController.instructionsContentGameObject.transform.childCount > 0)
                            Selection.activeObject = uiController.instructionsContentGameObject.transform.GetChild(0);
                        else
                            Selection.activeObject = uiController.instructionsContentGameObject;
                    }
                    EditorGUILayout.HelpBox(
                        "You can also change the 'terms & conditions' style message that must be ticked " +
                        "for the session to begin.", UnityEditor.MessageType.Info);
                    this.DrawProperty("termsAndConditions");
                    this.DrawProperty("tsAndCsInitialState");

                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }

        public void DrawUIDisabledMessage()
        {
            EditorGUILayout.HelpBox("The Built-in UI is not selected as the Startup Setting - these options have no effect.", UnityEditor.MessageType.Warning);
            EditorGUILayout.Separator();
        }

        public bool MiddleButton(string text, bool enabled = true)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool pressed = GUILayout.Button(text, GUILayout.MaxWidth(200), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            return pressed;
        }

    }

}


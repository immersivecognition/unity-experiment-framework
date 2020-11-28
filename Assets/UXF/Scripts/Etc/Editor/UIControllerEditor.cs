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
            { StartupMode.Automatic, "There will be no user interface displayed, and the session will start immediately and automatically, with an optional settings .json file (stored in the StreamingAssets path). Session number will always be set to 1 in this case." },
            { StartupMode.Manual, "The session will not start automatically - you must start it manually with code. You can start a session by calling the session.Begin() method, supplying the session information." }
        };
        static Dictionary<SettingsMode, string> settingsModeDescriptionMapping = new Dictionary<SettingsMode, string>()
        {
            { SettingsMode.AcquireFromUI,
                "A dropdown menu will list .json files in the StreamingAssets folder that match a search pattern. When the researcher or participants selects one of these, it will try to be converted to the session's settings object. " + 
                "The you can create as many .json files as you want, allowing for different sets of variables (e.g. number of trials) to be selected at startup. These .json files can be edited even in a built application (on Windows) by accessing " + 
                "the StreamingAssets folder." },
            { SettingsMode.DownloadFromURL, "Download a json file stored at a URL and try to convert it into the session's settings object. With this, a remotely deployed application can be edited just by modifying the .json file stored at the URL." },
            { SettingsMode.Empty, "The session will start with empty settings. You can still add your own settings to the session with session.settings.SetValue(...)." }
        };

        static Dictionary<PPIDMode, string> ppidModeDescriptionMapping = new Dictionary<PPIDMode, string>()
        {
            { PPIDMode.AcquireFromUI, "Displays a box which allows the participant or researcher to enter a participant ID. A dice button also displays to allow a random ppid to be quickly generated." },
            { PPIDMode.GenerateUnique, "No box will be displayed, instead a unique participant ID will be generated and used in the data output. This is recommended for Web-based experiments. The first part of the generated ID comes from a list of words, to make the PPIDs easier to read." }
        };

        static Dictionary<SessionNumMode, string> sessionNumModeDescriptionMapping = new Dictionary<SessionNumMode, string>()
        {
            { SessionNumMode.AcquireFromUI, "Displays a box which allows the participant or researcher to enter their session number. This allows the same participant to repeat the session multiple times by incrementing the session number." },
            { SessionNumMode.AlwaysSession1, "The session number will always be set to 1. Useful where you know a participant will only perform the session once." }
        };

        protected override void InitInspector()
        {
            base.InitInspector();
              
            // Always call DrawInspector function
            alwaysDrawInspector = true;
            uiController = (UIController)target;

            // we can interrupt the play button press if something isnt right
            EditorApplication.playModeStateChanged += HaltPlayIfUIIsInvalid;
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


                    string ppidReasonText;
                    if (!uiController.PPIDModeIsValid(out ppidReasonText))
                    {
                        string errorText = "Incompatibility Error: " + ppidReasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }

                    string settingsReasonText;
                    if (!uiController.SettingsModeIsValid(out settingsReasonText))
                    {
                        string errorText = "Incompatibility Error: " + settingsReasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }

                    string localPathStateReasonText;
                    if (!uiController.LocalPathStateIsValid(out localPathStateReasonText))
                    {
                        string errorText = "Incompatibility Error: " + localPathStateReasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }

                    EditorGUILayout.HelpBox("Startup Mode " + uiController.startupMode.ToString() + ": " + startupModeDescriptionMapping[uiController.startupMode], UnityEditor.MessageType.Info);
                    this.DrawProperty("startupMode");
                    if (uiController.startupMode == StartupMode.Automatic || uiController.settingsMode != SettingsMode.AcquireFromUI) this.DrawProperty("experimentName");
                    EditorGUILayout.Separator();

                    EditorGUI.BeginDisabledGroup(uiController.startupMode == StartupMode.Manual);
                    if (uiController.startupMode == StartupMode.Manual)
                    {
                        EditorGUILayout.HelpBox("Manual is selected as the Startup Setting - the options below have no effect.", UnityEditor.MessageType.Info);
                        EditorGUILayout.Separator();
                    }

                    EditorGUILayout.HelpBox("PPID Mode " + uiController.ppidMode.ToString() + ": " + ppidModeDescriptionMapping[uiController.ppidMode], UnityEditor.MessageType.Info);
                    this.DrawProperty("ppidMode");
                    if (uiController.ppidMode == PPIDMode.GenerateUnique)
                    {
                        this.DrawProperty("uuidWordList");
                    }
                    else if (uiController.ppidMode == PPIDMode.AcquireFromUI)
                    {
                        EditorGUILayout.Separator();
                        EditorGUILayout.HelpBox("Session Num Mode " + uiController.sessionNumMode.ToString() + ": " + sessionNumModeDescriptionMapping[uiController.sessionNumMode], UnityEditor.MessageType.Info);
                        this.DrawProperty("sessionNumMode");
                        EditorGUILayout.Separator();
                    }
                    EditorGUILayout.Separator();


                    EditorGUILayout.HelpBox("Settings Mode " + uiController.settingsMode.ToString() + ": " + settingsModeDescriptionMapping[uiController.settingsMode], UnityEditor.MessageType.Info);
                    this.DrawProperty("settingsMode");
                    switch (uiController.settingsMode)
                    {
                        case SettingsMode.AcquireFromUI:
                            this.DrawProperty("settingsSearchPattern");
                            break;
                        case SettingsMode.DownloadFromURL:
                            this.DrawProperty("jsonURL");
                            break;
                        case SettingsMode.Empty:
                            break;
                    }

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Separator();

                    if (EditorExtensions.MiddleButton("Force UI Refresh")) uiController.LateValidate();
                    break;
                case 1:
                    string datapointsReasonText;
                    EditorGUILayout.HelpBox(
                        "Below you can enter a series of datapoints to be collected about a participant" +
                        "- such as their age, gender, or even things that could affect the task such as " +
                        "the preferred hand of the participant. Press the Generate button to update the UI " +
                        "to reflect your items.", UnityEditor.MessageType.Info);
                    bool valid = uiController.DatapointsAreValid(out datapointsReasonText);
                    if (!valid)
                    {
                        string errorText = "Participant Datapoints Error: " + datapointsReasonText;
                        EditorGUILayout.HelpBox(errorText, UnityEditor.MessageType.Error);
                        EditorGUILayout.Separator();
                    }
                    if (EditorExtensions.MiddleButton("Generate", enabled: valid)) uiController.GenerateSidebar();
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel++;
                    DrawPropertiesFromUpTo("participantDataPoints", "termsAndConditions");
                    EditorGUI.indentLevel--;
                    break;
                case 2:
                    EditorGUILayout.HelpBox(
                        "You can edit the content that displays in the right side of the UI. This can " +
                        "be used for instructions to the participant or researcher on how the study should " +
                        "be performed, and what types of data will be collected. " + 
                        "You can add any UI elements such as text, images, or even buttons to  " +
                        "the Instructions Panel Content GameObject. You can select it with the button below.", UnityEditor.MessageType.Info);
                    if (EditorExtensions.MiddleButton("Select Content GameObject"))
                    {
                        if (uiController.instructionsContentTransform.childCount > 0)
                            Selection.activeObject = uiController.instructionsContentTransform.GetChild(0);
                        else
                            Selection.activeObject = uiController.instructionsContentTransform;
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
            EditorGUILayout.HelpBox("The Built-in UI is not selected as the Startup Setting - the options below have no effect.", UnityEditor.MessageType.Warning);
            EditorGUILayout.Separator();
        }


        public void HaltPlayIfUIIsInvalid(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string ppidReasonText;
                if (!uiController.PPIDModeIsValid(out ppidReasonText))
                {
                    EditorApplication.isPlaying = false;
                    string errorText = "Incompatibility Error: " + ppidReasonText;
                    Utilities.UXFDebugLogError(errorText);
                }

                string settingsReasonText;
                if (!uiController.SettingsModeIsValid(out settingsReasonText))
                {
                    EditorApplication.isPlaying = false;
                    string errorText = "Incompatibility Error: " + settingsReasonText;
                    Utilities.UXFDebugLogError(errorText);
                }

                string datapointsReasonText;
                if (!uiController.DatapointsAreValid(out datapointsReasonText))
                {
                    EditorApplication.isPlaying = false;
                    string errorText = "Incompatibility Error: " + datapointsReasonText;
                    Utilities.UXFDebugLogError(errorText);
                }

                string localPathStateReasonText;
                if (!uiController.LocalPathStateIsValid(out localPathStateReasonText))
                {
                    string errorText = "Incompatibility Error: " + localPathStateReasonText;
                    Utilities.UXFDebugLogError(errorText);
                }
            }
        }

    }

}


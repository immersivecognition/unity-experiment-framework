using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace UXF.EditorUtils
{

    [InitializeOnLoad]
    public class UXFWizard : EditorWindow
    {
		public Texture2D uxfIcon;
        public static bool forceShow = false;
        // UXF's core and Win32 file-dialog adapter target .NET Standard 2.1.
        // The native dialog calls do not require the legacy .NET Framework profile.
        ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_Standard;
        static string settingsKey { get { return PlayerSettings.productName + ":uxf_seen_wizard"; } }

        static string version;

        Vector2 scrollPos;

        static UXFWizard()
        {
            EditorApplication.projectChanged += OnProjectChanged;
        }


        [MenuItem("UXF/Show setup wizard")]
        static void Init()
        {
            var window = (UXFWizard) EditorWindow.GetWindow(typeof(UXFWizard), false, "UXF Wizard");
            window.minSize = new Vector2(300, 785);
			window.titleContent = new GUIContent("UXF Wizard");
            window.Show();

            if (File.Exists("Packages/com.immersivecognition.uxf/VERSION.txt"))
            {
                version = File.ReadAllText("Packages/com.immersivecognition.uxf/VERSION.txt");
            }
            else
            {
                version = "unknown";
            }
        }

        static void OnProjectChanged()
        {
            bool seen;

            if (EditorPrefs.HasKey(settingsKey))
            {
                seen = EditorPrefs.GetBool(settingsKey);
            }
            else
            {
                seen = false;
            }

            if (forceShow | !seen)
            {
                Init();
                EditorPrefs.SetBool(settingsKey, true);
            }
        }

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetRect(128, 128, GUI.skin.box);
            if (uxfIcon)
                GUI.DrawTexture(rect, uxfIcon, ScaleMode.ScaleToFit);
            GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("UXF: Unity Experiment Framework", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version " + version, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.Label("Platform selector", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click the buttons below to switch to your desired output platform. You will also need to select the Data Handler(s) you wish to use in your UXF Session Component.", MessageType.Info);

            if (GUILayout.Button("Select Windows / PC VR")) SetSettingsWindows();
            if (GUILayout.Button("Select Web Browser")) SetSettingsWebGL();
            if (GUILayout.Button("Select Android VR (e.g. Oculus Quest)")) SetSettingsOculus();
            if (GUILayout.Button("Select Android")) SetSettingsAndroid();

            EditorGUILayout.Separator();

            GUILayout.Label("Help and info", EditorStyles.boldLabel);

            GUILayout.Label("The GitHub page contains the most up-to-date information & release.", labelStyle);
			if (GUILayout.Button("Visit GitHub"))
				Application.OpenURL("https://github.com/immersivecognition/unity-experiment-framework/");

            EditorGUILayout.Space();
            GUILayout.Label("The GitHub Wiki contains documentation and in-depth explanations of concepts.", labelStyle);
            if (GUILayout.Button("Visit Wiki"))
                Application.OpenURL("https://github.com/immersivecognition/unity-experiment-framework/wiki");


            EditorGUILayout.Separator();

            GUILayout.Label("Examples", EditorStyles.boldLabel);
            GUILayout.Label("Import UXF Examples from Package Manager > Samples", labelStyle);

            EditorGUILayout.Separator();

            GUILayout.Label("Cite UXF", EditorStyles.boldLabel);

            if (GUILayout.Button("DOI Link"))
                Application.OpenURL("https://doi.org/10.3758/s13428-019-01242-0");

            EditorGUILayout.Separator();

            GUILayout.Label("Compatibility", EditorStyles.boldLabel);

            var standalone = NamedBuildTarget.Standalone;
            
            bool compatible = PlayerSettings.GetApiCompatibilityLevel(standalone) == targetApiLevel;

            if (compatible)
            {
                EditorGUILayout.HelpBox("API Compatibility Level is set to .NET Standard 2.1.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("UXF targets .NET Standard 2.1. Use .NET Framework only if another dependency in your project requires it.", MessageType.Warning);
                if (GUILayout.Button("Fix"))
                {
                    PlayerSettings.SetApiCompatibilityLevel(standalone, targetApiLevel);
                }
            }

            // When the option toggle is disabled, Unity uses the normal full
            // domain/scene reload behavior. Only an explicitly enabled fast
            // Enter Play Mode configuration bypasses those reloads.
            bool fastEnterPlayMode = EditorSettings.enterPlayModeOptionsEnabled &&
                EditorSettings.enterPlayModeOptions != EnterPlayModeOptions.None;
            if (fastEnterPlayMode)
            {
                EditorGUILayout.HelpBox("UXF's validated configuration uses full domain and scene reloads. Known runtime statics reset safely, but scene reload is still required until fast-play acceptance tests cover persistent components and integrations.", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    EditorSettings.enterPlayModeOptionsEnabled = false;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Full domain and scene reload behavior is enabled for UXF.", MessageType.Info);
            }
            EditorGUILayout.Separator();

            GUILayout.Label("WebGL", EditorStyles.boldLabel);

            const string expected = "PROJECT:UXF WebGL 2020";

            if (PlayerSettings.WebGL.template == expected)
            {
                EditorGUILayout.HelpBox("UXF WebGL template is set correctly. You may still need to enable WebGL in build settings.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Do you plan to run your experiment in a web browser? UXF WebGL template is not selected as the WebGL Template in Player Settings.", MessageType.Warning);
                if (GUILayout.Button("Fix"))
                {
                    PlayerSettings.WebGL.template = expected;
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox("To show this window again go to UXF -> Show setup wizard in the menubar.", MessageType.None);
            
            EditorGUILayout.EndScrollView();
        }

        private static void SetSettingsWindows()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            Utilities.UXFDebugLog("Setup for Windows/PCVR.");
        }

        private static void SetSettingsWebGL()
        {
            const string expected = "PROJECT:UXF WebGL 2020";
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.WebGL.template = expected;
            Utilities.UXFDebugLog("Setup for WebGL.");
        }

        private static void SetSettingsAndroid()
        {
            // Switch to Android build.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // If the current build target is Android, configure the platform defaults.
            // FileSaver uses app-private PersistentDataPath and does not require the
            // legacy External (SDCard) permission.
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Sets the Texture Compression to Default (Don't override)
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;

                Utilities.UXFDebugLog("Setup for Android.");
            }

            // If the build target was not set to Android (it may not be available on the system)
            else
            {
                Utilities.UXFDebugLog("Android build was not set, check if it is available. If it isn't, add it to the Unity Editor version via the Unity Hub.");
            }
        }

        private static void SetSettingsOculus()
        {
            // Switch to Android build.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);        

            // If the current build target is Android, configure API levels and texture compression.
            // FileSaver uses app-private PersistentDataPath and does not require the
            // legacy External (SDCard) permission.
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Sets the Android API Level to 26
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

                // Sets the Android API Level to Automatic (highest installed)
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

                // Sets the Texture Compression to ASTC
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

                Utilities.UXFDebugLog("Setup for Android VR systems (e.g. Oculus Quest).");
            }

            // If the build target was not set to Android (it may not be available on the system)
            else
            {
                Utilities.UXFDebugLog("Android build was not set, check if it is available. If it isn't, add it to the Unity Editor version via the Unity Hub.");
            }
        }

    }

}

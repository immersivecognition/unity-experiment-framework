using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UXF.EditorUtils
{

    [InitializeOnLoad]
    public class UXFWizard : EditorWindow
    {
		public Texture2D uxfIcon;
        public static bool forceShow = false;
#if UNITY_2018_3_OR_NEWER
        ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_4_6;
#else
        ApiCompatibilityLevel targetApiLevel = ApiCompatibilityLevel.NET_2_0;
#endif
        static string settingsKey { get { return PlayerSettings.productName + ":uxf_seen_wizard"; } }

        static string version;

        Vector2 scrollPos;

        static UXFWizard()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.projectChanged += OnProjectChanged;
#else
            EditorApplication.projectWindowChanged += OnProjectChanged;
#endif
        }


        [MenuItem("UXF/Show setup wizard")]
        static void Init()
        {
            var window = (UXFWizard) EditorWindow.GetWindow(typeof(UXFWizard), false, "UXF Wizard");
            window.minSize = new Vector2(300, 785);
			window.titleContent = new GUIContent("UXF Wizard");
            window.Show();

            if (File.Exists("Assets/UXF/VERSION.txt"))
            {
                version = File.ReadAllText("Assets/UXF/VERSION.txt");
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
            GUILayout.Label("Check your Assets > UXF > Examples folder", labelStyle);

            EditorGUILayout.Separator();

            GUILayout.Label("Cite UXF", EditorStyles.boldLabel);

            if (GUILayout.Button("DOI Link"))
                Application.OpenURL("https://doi.org/10.3758/s13428-019-01242-0");

            EditorGUILayout.Separator();

            GUILayout.Label("Compatibility", EditorStyles.boldLabel);

            bool compatible = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == targetApiLevel;

            if (compatible)
            {
                EditorGUILayout.HelpBox("API Compatibility Level is set correctly", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("API Compatibility Level should be set to .NET 2.0 (Older versions of Unity) or .NET 4.x (Unity 2018.3+), expect errors on building", MessageType.Warning);
                if (GUILayout.Button("Fix"))
                {
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, targetApiLevel);
                }
            }

#if UNITY_2019_3_OR_NEWER
            if (!EditorSettings.enterPlayModeOptionsEnabled || EditorSettings.enterPlayModeOptions != EnterPlayModeOptions.None)
            {
                EditorGUILayout.HelpBox("You currently must enable Reload Domain and Reload Scene in the Editor Settings to use UXF. Please enable these options.", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Reload Domain and Reload Scene Editor Settings are correctly enabled.", MessageType.Info);
            }
#endif

            EditorGUILayout.Separator();

            GUILayout.Label("WebGL", EditorStyles.boldLabel);

            string expected;

#if UNITY_2020_1_OR_NEWER
            expected = "PROJECT:UXF WebGL 2020";
#else
            expected = "PROJECT:UXF WebGL 2019";
#endif

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
            string expected;
#if UNITY_2020_1_OR_NEWER
            expected = "PROJECT:UXF WebGL 2020";
#else
            expected = "PROJECT:UXF WebGL 2019";
#endif
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.WebGL.template = expected;
            Utilities.UXFDebugLog("Setup for WebGL.");
        }

        private static void SetSettingsAndroid()
        {
            // Switch to Android build.
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // If the current build target is Android, then set the write permission to external
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Changes Android write permission to external
                PlayerSettings.Android.forceSDCardPermission = true;

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

            // If the current build target is Android, then set the write permission to external, and configure API levels and Texture compression
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Changes Android write permission to external
                PlayerSettings.Android.forceSDCardPermission = true;

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
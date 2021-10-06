using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace UXF.EditorUtils
{
    class UXFBuildPreprocessor : IProcessSceneWithReport
    {
        public int callbackOrder { get { return 0; } }
        
        private BuildTarget[] localFileDataHandlerCompatiblePlatforms = new BuildTarget[]
        {
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.Android
        };

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return;
            Utilities.UXFDebugLogFormat("UXF is pre-processing your built scene '{0}' for platform '{1}' to make sure settings are compatible with the build... ", scene.name, report.summary.platform);

            var uis = scene
                .GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<UXF.UI.UIController>(true));

            string expected;
#if UNITY_2020_1_OR_NEWER
            expected = "PROJECT:UXF WebGL 2020";
#else
            expected = "PROJECT:UXF WebGL 2019";
#endif

            // check webGL
            if (report.summary.platform == BuildTarget.WebGL &&
                    PlayerSettings.WebGL.template != expected)
            {
                Utilities.UXFDebugLogWarning("The UXF WebGL template is not selected in WebGL player settings! This may lead to errors. You can fix the is with the UXF Wizard (press UXF at the top, Show UXF Wizard).");
            }

            foreach (var ui in uis)
            {
                // check local file access requirement
                var localHandlers = ui.ActiveLocalFileDataHandlers;
                if (localHandlers.Count() > 0 && !localFileDataHandlerCompatiblePlatforms.Contains(report.summary.platform))
                {
                    CancelBuild(string.Format(
                        "Cannot build scene {0} for platform '{1}'.\nReason: The Data Handler{2} '{3}' require{4} local file access, which is not compatible with {1}. " +
                        "You can deselect Data Handler{2} '{3}' with the check box in the Data Handling tab of the UXF Session Component." + 
                        "Perhaps try one of the other data handlers that are compatible with this build target.",
                        scene.name,
                        report.summary.platform,
                        localHandlers.Count() > 1 ? "s" : "",
                        string.Join(", ", localHandlers.Select(ldh => ldh.name)),
                        localHandlers.Count() == 1 ? "s" : ""
                        ));
                }
                
                // check UI
                if (ui.settingsMode == UI.SettingsMode.AcquireFromUI && !localFileDataHandlerCompatiblePlatforms.Contains(report.summary.platform))
                {
                    CancelBuild(string.Format(
                        "Cannot build scene {0} for platform '{1}'.\nReason: Settings Mode 'Acquire From UI' is not compatible with {1}. This is because " +
                        "it needs access to the settings profile .json files in the StreamingAssets folder, which is not supported on {1}." + 
                        "You can change the settings mode in the UXF UI configuration.",
                        scene.name,
                        report.summary.platform
                    ));
                }

                // manually test for data handlers that are not compatible with the build target
                Session session = ui.GetComponentInParent<Session>();
                foreach (var dh in session.ActiveDataHandlers)
                {
                    bool compatible = dh.IsCompatibleWith(report.summary.platformGroup);
                    bool incompatible = dh.IsIncompatibleWith(report.summary.platformGroup);

                    if (incompatible)
                        CancelBuild(string.Format(
                            "Cannot build scene {0}. The data handler '{1}' reports it is incompatible with platform group '{2}'. Please disable this data handler or select another build target.",
                            scene.name, 
                            dh.name, 
                            report.summary.platformGroup
                        ));
                    
                    if (!compatible && !incompatible)
                    {
                        Utilities.UXFDebugLogWarningFormat(
                            "Warning: (Scene: {0}) - The data handler '{1}' has not reported either compatibility or incompatibility with platform group '{2}'. Use at your own risk!",
                            scene.name, 
                            dh.name, 
                            report.summary.platformGroup
                        );
                    }
                }

                // check android
                if (report.summary.platform == BuildTarget.Android)
                {
                    foreach (var dh in session.ActiveDataHandlers)
                    {
                        if (dh is FileSaver)
                        {
                            var fileSaver = dh as FileSaver;

                            // check android must be set to persistent data path
                            if (fileSaver.dataSaveLocation != DataSaveLocation.PersistentDataPath)
                            {
                                CancelBuild(string.Format(
                                    "Cannot build scene {0} for platform '{1}'.\nReason: To use the Data Handler '{2}' with {1} " +
                                    "you must set the Data Save Location to '{3}'. Data will then be stored on the internal file system.",
                                    scene.name,
                                    report.summary.platform,
                                    fileSaver.name,
                                    DataSaveLocation.PersistentDataPath
                                ));
                            }

                            // check external SD card access is set in player settings
                            if (!PlayerSettings.Android.forceSDCardPermission)
                            {
                                CancelBuild(string.Format(
                                    "Cannot build scene {0} for platform '{1}'.\nReason: To use the Data Handler '{2}' you must set the Write Permission " +
                                    "in the Player Settings to 'External (SDCard)'.",
                                    scene.name,
                                    report.summary.platform,
                                    fileSaver.name
                                ));
                            }
                        }
                    }
                }
            }
        }


        void CancelBuild(string text)
        {
            EditorUtility.DisplayDialog("Build cancelled", text, "OK");
            throw new BuildFailedException(text);
        }
    }
}

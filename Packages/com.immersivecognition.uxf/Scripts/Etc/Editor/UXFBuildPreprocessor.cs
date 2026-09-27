using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
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

            var sceneObjects = scene
                .GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Transform>(true));

            var uis = sceneObjects
                .Select(t => t.GetComponent<UXF.UI.UIController>())
                .Where(ui => ui != null)
                .ToList();

            var sessions = sceneObjects
                .Select(t => t.GetComponent<Session>())
                .Where(session => session != null)
                .Distinct()
                .ToList();

            const string expected = "PROJECT:UXF WebGL 2020";

            // check webGL
            if (report.summary.platform == BuildTarget.WebGL &&
                    PlayerSettings.WebGL.template != expected)
            {
                Utilities.UXFDebugLogWarning("The UXF WebGL template is not selected in WebGL player settings! This may lead to errors. You can fix the is with the UXF Wizard (press UXF at the top, Show UXF Wizard).");
            }

            // UI is optional. Validate every Session in the scene so a manual or
            // code-driven experiment cannot bypass platform checks simply by
            // omitting the bundled UIController.
            var validatedSessions = new HashSet<Session>();

            foreach (var ui in uis)
            {
                Session session = ui.GetComponentInParent<Session>();
                if (session == null)
                {
                    CancelBuild(string.Format(
                        "Cannot build scene {0}. The UXF UIController '{1}' is not a child of a UXF Session.",
                        scene.name,
                        ui.name
                    ));
                }

                sessions.Add(session);

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

            }

            foreach (var session in sessions.Distinct())
            {
                if (session == null || !validatedSessions.Add(session)) continue;

                // Local file access is a Session/DataHandler constraint, not a
                // UI constraint. Check it here so manual-start and UI-free
                // Sessions receive the same build validation.
                var localHandlers = session.ActiveDataHandlers
                    .OfType<LocalFileDataHander>()
                    .ToList();
                if (localHandlers.Count > 0 && !localFileDataHandlerCompatiblePlatforms.Contains(report.summary.platform))
                {
                    CancelBuild(string.Format(
                        "Cannot build scene {0} for platform '{1}'.\nReason: The Data Handler{2} '{3}' require{4} local file access, which is not compatible with {1}. " +
                        "You can deselect Data Handler{2} '{3}' with the check box in the Data Handling tab of the UXF Session Component. " +
                        "Perhaps try one of the other data handlers that are compatible with this build target.",
                        scene.name,
                        report.summary.platform,
                        localHandlers.Count > 1 ? "s" : "",
                        string.Join(", ", localHandlers.Select(ldh => ldh.name)),
                        localHandlers.Count == 1 ? "s" : ""
                    ));
                }

                // Manually test data handlers that report compatibility with the
                // selected target. This also runs for UI-free Sessions.
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
                    foreach (var fileSaver in session.ActiveDataHandlers.OfType<FileSaver>())
                    {
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

                        // PersistentDataPath is app-private storage on Android and does not
                        // require the legacy External (SDCard) permission. Keep this check
                        // focused on the storage location; projects that deliberately use a
                        // different native storage provider own any additional permission.
                    }
                }
            }
        }


        void CancelBuild(string text)
        {
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Build cancelled", text, "OK");
            throw new BuildFailedException(text);
        }
    }
}

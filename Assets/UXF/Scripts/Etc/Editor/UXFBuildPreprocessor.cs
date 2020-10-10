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
            BuildTarget.StandaloneLinux,
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64
        };

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null) return;
            Debug.LogFormat("UXF is pre-processing your built scene '{0}' for platform '{1}' to make sure settings are compatible with the build... ", scene.name, report.summary.platform);

            var uis = scene
                .GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<UXF.UI.UIController>(true));

            foreach (var ui in uis)
            {
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
        }


        void CancelBuild(string text)
        {
            EditorUtility.DisplayDialog("Build cancelled", text, "OK");
            throw new BuildFailedException(text);
        }
    }
}

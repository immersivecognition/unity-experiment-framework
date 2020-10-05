using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace UXF.EditorUtils
{
    class UXFBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.LogFormat("UXF is pre-processing your build for platform '{0}' to make sure settings are compatible with the build... ", report.summary.platform);
        }
    }
}

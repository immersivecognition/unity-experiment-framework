using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UXF
{
    /// <summary>
    /// Useful methods
    /// </summary>
    public static class Utilities
    {

        public static void UXFDebugLog(string message) { Debug.Log(UXFFormatString(message)); }
        public static void UXFDebugLogWarning(string message) { Debug.LogWarning(UXFFormatString(message)); }
        public static void UXFDebugLogError(string message) { Debug.LogError(UXFFormatString(message)); }
        public static void UXFDebugLogFormat(string format, params object[] args) { Debug.LogFormat(UXFFormatString(format), args); }
        public static void UXFDebugLogWarningFormat(string format, params object[] args) { Debug.LogWarningFormat(UXFFormatString(format), args); }
        public static void UXFDebugLogErrorFormat(string format, params object[] args) { Debug.LogErrorFormat(UXFFormatString(format), args); }


        private static string UXFFormatString(string message)
        {
            return string.Format(
                "{0} {1}",
                Application.platform != RuntimePlatform.WebGLPlayer ?
                    "[<b><color=#AD477C>UXF</color></b>]" :
                    "[UXF]",
                message
            );
        }

    }       

}
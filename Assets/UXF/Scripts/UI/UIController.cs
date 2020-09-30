using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SubjectNerd.Utilities;

namespace UXF.UI
{
    public class UIController : MonoBehaviour
    {
        public StartupMode startupMode = StartupMode.BuiltInUI;
        public SessionSettingsMode settingsMode = SessionSettingsMode.SelectWithUI;
        public string aaa;

        private Session session;


        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            if (session == null) session = GetComponentInParent<Session>();
        }


    }


    public enum StartupMode
    {
        BuiltInUI, Automatic, Manual 
    }

    public enum SessionSettingsMode
    {
        SelectWithUI, DownloadFromURL, None 
    }
}
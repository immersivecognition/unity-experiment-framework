using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Threading;
using System.Linq;


namespace UXF
{
    public abstract class LocalFileDataHander : DataHandler
    {
        [Space, Tooltip("Should the location the data is stored in be: Acquired via the UI, or, a fixed path?")]
        public DataSaveLocation dataSaveLocation;

        /// <summary>
        /// Local path where the data should be stored.
        /// </summary>
        [Tooltip("If fixed path is selected, where should the data be stored? You could set this value by writing a script that writes to this field in Awake()."), SerializeField]
        [BasteRainGames.HideIfEnumValue("dataSaveLocation", BasteRainGames.HideIf.NotEqual, (int)DataSaveLocation.Fixed)]
        public string storagePath = "~";

        [HideInInspector]
        public UnityEvent onValidateEvent = new UnityEvent();

        public string StoragePath
        {
            get
            {
                if (dataSaveLocation == DataSaveLocation.PersistentDataPath)
                {
                    return Application.persistentDataPath;
                }
                else
                {
                    return storagePath;
                }
            }
            set
            {
                if (dataSaveLocation == DataSaveLocation.PersistentDataPath)
                {
                    throw new InvalidOperationException("Cannot set storagePath - dataSaveLocation is set to PersistentDataPath.");
                }
                storagePath = value;
            }
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            onValidateEvent.Invoke();
        }


# if UNITY_EDITOR
        /// <summary>
        /// Returns true if this data handler is definitely compatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTargetGroup.Standalone:
                case UnityEditor.BuildTargetGroup.Android:
                    return true;
                default:
                    return false;
            }
        }

         /// <summary>
        /// Returns true if this data handler is definitely incompatible with this build target.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public override bool IsIncompatibleWith(UnityEditor.BuildTargetGroup buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTargetGroup.WebGL:
                case UnityEditor.BuildTargetGroup.iOS:
                    return true;
                default:
                    return false;
            }
        }
# endif

    }

    public enum DataSaveLocation
    {
        AcquireFromUI, Fixed, PersistentDataPath
    }



}

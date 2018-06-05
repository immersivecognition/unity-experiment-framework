using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

    public class TaskManager : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
			// disable the whole task initially to give time for the experimenter to use the UI
			gameObject.SetActive(false);
        }

        public void QuitIfLastTrial(Trial trial)
        {
            if (trial == trial.session.lastTrial)
            {                
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }


    }

}
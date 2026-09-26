using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

    public class Example_TaskManager : MonoBehaviour
    {

        void Start()
        {
			// disable the whole task initially to give time for the experimenter to use the UI
			gameObject.SetActive(false);
        }

        public void EndIfLastTrial(Trial trial)
        {
            if (trial == trial.session.LastTrial)
            {
                trial.session.End();
            }
        }

    }

}
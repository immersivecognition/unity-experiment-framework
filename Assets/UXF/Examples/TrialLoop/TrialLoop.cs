using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// add the UXF namespace
using UXF;

namespace UXFExamples
{

    public class TrialLoop : MonoBehaviour
    {

        UXF.Session session;

        public void GenerateExperiment(Session experimentSession)
        {
            // save reference to session
            session = experimentSession;
            // This function can be called using the Session inspector OnSessionBegin() event, or otherwise

            // retrieve the n_practice_trials setting, which was loaded from our .json file
            int numPracticeTrials = Convert.ToInt32(session.settings["n_practice_trials"]);
            // create block 1
            Block practiceBlock = session.CreateBlock(numPracticeTrials);
            practiceBlock.settings["practice"] = true;

            // retrieve the n_main_trials setting, which was loaded from our .json file into our session settings
            int numMainTrials = Convert.ToInt32(session.settings["n_main_trials"]);
            // create block 2
            Block mainBlock = session.CreateBlock(numMainTrials); // block 2

            // here we set a setting for the 2nd trial of the main block as an example.
            mainBlock.GetRelativeTrial(2).settings["size"] = 10;
            mainBlock.GetRelativeTrial(1).settings["color"] = Color.red;
        }


        public void StartLoop()
        {
            // called from OnSessionBegin, hence starting the trial loop when the session starts
            StartCoroutine(Loop());
        }

        IEnumerator Loop()
        {
            foreach (Trial trial in session.Trials)
            {
                trial.Begin();
                PresentStimulus(trial);
                yield return new WaitForSeconds(1f);
                trial.End();
            }

            session.End();
        }

        
        void PresentStimulus(Trial trial)
        {
            // here we can imagine presentation of some stimulus

            Debug.Log("Running trial!");

            // we can access our settings to (e.g.) modify our scene
            // for more information about retrieving settings see the documentation

            float size = Convert.ToInt32(trial.settings["size"]);
            Debug.LogFormat("The 'size' for this trial is: {0}", size);

            // record custom values...
            string observation = UnityEngine.Random.value.ToString();
            Debug.Log(string.Format("We observed: {0}", observation));
            trial.result["some_variable"] = observation;
        }


    }

}
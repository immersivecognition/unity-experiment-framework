using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Example script used to test functionality of the Experiment Manager
/// </summary>
public class BasicExampleScript : MonoBehaviour {

    ExpMngr.ExperimentSession session;
    float startNextTime;

    
    void Start()
    {
        // disable this behavior so the Update() function doesnt run just yet.
        enabled = false;
    }
    
    public void GenerateExperiment(ExpMngr.ExperimentSession expSession)
    {
        // save reference to session
        session = expSession;
        /// This function can be called using the ExperimentSession inspector OnSessionBegin() event, or otherwise


        // / In the StreamingAssets folder we have a several .json files that contain settings, e.g.
        // / that looks like this:
        // 
        //  {
        //
        //  "n_practice_trials": 5,
        //  "n_main_trials": 10,
        //  "size": 1        
        //
        //  }        
        //
        /* You can add any new settings to the JSON file
        it will automatically be loaded into the settings property
        of an ExperimentSession component as .settings */

        // create our blocks & trials
        
        // practice block

        // retrieve the n_practice_trials setting, which was loaded from our .json file
        int numPracticeTrials = Convert.ToInt32(session.settings["n_practice_trials"]);
        // create block 1
        ExpMngr.Block practiceBlock = session.CreateBlock(numPracticeTrials);


        // retrieve the n_main_trials setting, which was loaded from our .json file
        int numMainTrials = Convert.ToInt32(session.settings["n_main_trials"]);
        // create block 2
        ExpMngr.Block mainBlock = session.CreateBlock(numMainTrials); // block 2

        // here we set a setting for the 2nd trial of the main block as an example.
        mainBlock.GetRelativeTrial(2).settings["size"] = 10;

        // setting this script to enabled allows the MonoBehaviour scripts to run e.g. Update()
        enabled = true;
    }

    void Update()
    {
        // here we are mimicking some experiment behaviour, e.g waiting for user to interact with scene
        if (Time.time > startNextTime && session.inTrial)
        {
            Debug.Log("Ending trial");
            session.EndCurrentTrial();
            
            if (session.currentTrial == session.lastTrial)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
                Application.OpenURL(webplayerQuitURL);
#else
                Application.Quit();
#endif
            }
        }
    }

    public void PresentStimulus(ExpMngr.Trial trial)
    {
        // we can call this function via the event "On Trial Begin", which is called when the trial starts
        // here we can imagine presentation of some stimulus

        Debug.Log("Running trial!");
        
        // we can access our settings to (e.g.) modify our scene
        // for more information about retrieving settings see the documentation
        float size = Convert.ToSingle(trial.settings["size"]);
        Debug.LogFormat("The 'size' for this trial is: {0}", size);

        // record custom values...
        string observation = UnityEngine.Random.value.ToString();
        Debug.Log(string.Format("We observed: {0}", observation));
        trial.result["some_variable"] = observation;

        // wait 1 second until we end trial and run the next one
        startNextTime = Time.time + 1;
    }
}



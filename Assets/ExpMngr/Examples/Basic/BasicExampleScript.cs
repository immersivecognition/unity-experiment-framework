using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Example script used to test functionality of the Experiment Manager
/// </summary>
public class BasicExampleScript : MonoBehaviour {

    ExpMngr.ExperimentSession exp;
    float startNextTime;

    
    void Start()
    {
        // disable this behavior so the Update() function doesnt run just yet.
        enabled = false;
    }
    
    public void GenerateAndRunExperiment(ExpMngr.ExperimentSession expSession)
    {

        exp = expSession;
        /// This function can be called using the ExperimentSession inspector OnSessionStart() event, or otherwise


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
        // / You can add any new settings to the JSON file
        // / It will automatically be loaded into the settings property
        // / of an ExperimentSession component as a dictionary 

        // create our blocks & trials
        
        // practice block
        var practiceBlock = new ExpMngr.Block(exp); // block 1
        for (int i = 0; i < Convert.ToInt32(exp.settings["n_practice_trials"]); i++)
            new ExpMngr.Trial(practiceBlock);
        // main block
        var mainBlock = new ExpMngr.Block(exp); // block 2
        for (int i = 0; i < Convert.ToInt32(exp.settings["n_main_trials"]); i++)
            new ExpMngr.Trial(mainBlock);


        // here we set a setting for the 2nd trial of the main block as an example.
        exp.GetBlock(2).GetRelativeTrial(2).settings["size"] = 10;

        // setting this script to enabled allows the MonoBehaviour scripts to run e.g. Update()
        enabled = true;

        // begin first trial
        exp.BeginNextTrial();
    }

    void Update()
    {
        // here we are mimicking some experiment behaviour, e.g waiting for user to interact with scene
        if (Time.time > startNextTime && exp.inTrial)
        {
            Debug.Log("Ending trial");
            exp.currentTrial.End();
            
            if (exp.currentTrial == exp.lastTrial)
            {
                // end, then quit
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #elif UNITY_WEBPLAYER
                    Application.OpenURL(webplayerQuitURL);
                #else
                    Application.Quit();
                #endif
            }
            else
            {
                // start next trial
                exp.BeginNextTrial();
            }
        }
    }

    public void RunTrial()
    {

        // we call this function via the event "On Trial Begin", which is called when the trial starts
        Debug.Log("Running trial!");
        
        // we can access our settings to (e.g.) modify our scene
        Debug.LogFormat("The 'size' for this trial is: {0}", Convert.ToSingle(exp.currentTrial.settings["size"]));

        // record custom values...
        string observation = UnityEngine.Random.value.ToString();
        Debug.Log(string.Format("We observed: {0}", observation));
        exp.currentTrial.result["some_variable"] = observation;

        // wait 1 second until we end trial and run the next one
        startNextTime = Time.time + 1;
    }
}



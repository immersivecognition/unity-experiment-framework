using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// add the UXF namespace
using UXF;

/// <summary>
/// Example script used to test functionality of the Experiment Manager
/// </summary>
public class BasicExampleScript : MonoBehaviour {

    UXF.Session session;
    
    /// <summary>
    /// generates the trials and blocks for the session
    /// </summary>
    /// <param name="experimentSession"></param>
    public void GenerateExperiment(Session experimentSession)
    {
        // save reference to session
        session = experimentSession;
        // This function can be called using the Session inspector OnSessionBegin() event, or otherwise


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

        // retrieve the n_practice_trials setting, which was loaded from our .json file
        int numPracticeTrials = session.settings.GetInt("n_practice_trials");
        // create block 1
        Block practiceBlock = session.CreateBlock(numPracticeTrials);
        practiceBlock.settings.SetValue("practice", true);

        // retrieve the n_main_trials setting, which was loaded from our .json file into our session settings
        int numMainTrials = session.settings.GetInt("n_main_trials");
        // create block 2
        Block mainBlock = session.CreateBlock(numMainTrials); // block 2

        // here we set a setting for the 2nd trial of the main block as an example.
        mainBlock.GetRelativeTrial(2).settings.SetValue("size", 10);
        mainBlock.GetRelativeTrial(1).settings.SetValue("color", Color.red);

    }

    /// <summary>
    /// Example method presenting a stimulus to a user
    /// </summary>
    /// <param name="trial"></param>
    public void PresentStimulus(Trial trial)
    {
        // we can call this function via the event "On Trial Begin", which is called when the trial starts
        // here we can imagine presentation of some stimulus

        Debug.Log("Running trial!");
        
        // we can access our settings to (e.g.) modify our scene
        // for more information about retrieving settings see the documentation

        float size = trial.settings.GetFloat("size");
        Debug.LogFormat("The 'size' for this trial is: {0}", size);

        // record custom values...
        string observation = UnityEngine.Random.value.ToString();
        Debug.Log(string.Format("We observed: {0}", observation));
        trial.result["some_variable"] = observation;

        // end trial and prepare next trial in 1 second
        Invoke("EndAndPrepare", 1);
    }


    void EndAndPrepare()
    {
        Debug.Log("Ending trial");
        session.CurrentTrial.End();

        if (session.CurrentTrial == session.LastTrial)
        {
            session.End();
        }
        else
        {
            session.BeginNextTrial();
        }

    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Example script used to test functionality of the Experiment Manager
/// </summary>
public class TestScript : MonoBehaviour {

    Dictionary<string, object> sessionInfo;
    ExpMngr.ExperimentSession exp;
    float startNextTime;


    // Use this for initialization
    void Start () {
                    
        // get our experiment script
         exp = GetComponent<ExpMngr.ExperimentSession>();

        // set details of our session here, for example we could create a dictionary with participant ID, gender, age etc
        sessionInfo = new Dictionary<string, object>()
        {
            { "participant_id", 1 },
            { "gender", "f" },
            { "age", 30 }
        };
        // we need a unique session ID to store our data in
        string sessionID = string.Format("P{0}S1", sessionInfo["participant_id"]);
        // initialise the session
        exp.InitSession(sessionID, sessionInfo);


        // / In the StreamingAssets/experiment/ folder we have a settings.json
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
        for (int i = 0; i < (long)exp.settings["n_practice_trials"]; i++)
            new ExpMngr.Trial(exp, practiceBlock);
        // main block
        var mainBlock = new ExpMngr.Block(exp); // block 2
        for (int i = 0; i < (long) exp.settings["n_main_trials"]; i++)
            new ExpMngr.Trial(exp, mainBlock);


        // here we set a setting for the 2nd trial of the main block as an example.
        exp.GetBlock(2).GetRelativeTrial(2).settings["size"] = 2;

        // start first trial. we start on trialnum = 0 so we must get the next trial
        RunNextTrial();
    }

    void Update()
    {
        // here we are mimicking some experiment behaviour 
        if (Time.time > startNextTime)
        {
            exp.currentTrial.End();
            if (exp.currentTrial == exp.lastTrial)
            {
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
                RunNextTrial();
            }            
        }
    }

    void RunNextTrial()
    {
        exp.nextTrial.Begin();
        // record custom values...
        exp.currentTrial.result["some_variable"] = UnityEngine.Random.value.ToString();
        // resume game for 1 second
        startNextTime = Time.time + 1;
    }

}



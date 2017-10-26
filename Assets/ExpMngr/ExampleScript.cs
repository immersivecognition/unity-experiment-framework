using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Example script used to test functionality of the Experiment Manager
/// </summary>
public class ExampleScript : MonoBehaviour {

    public ExpMngr.ExperimentSession exp;
    float startNextTime;

    
    public void GenerateAndRunExperiment() {
        /// This function can be called using the ExperimentSession inspector OnSessionStart() event, or otherwise


        // / In the example_output/experiment_name/ folder we have a settings.json
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

        // start first trial
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
        Debug.Log("Starting trial!");
        exp.nextTrial.Begin();
        
        // record custom values...
        string observation = UnityEngine.Random.value.ToString();
        Debug.Log(string.Format("We observed: {0}", observation));
        exp.currentTrial.result["some_variable"] = observation;

        // wait 1 second until we end trial and run the next one
        startNextTime = Time.time + 1;
    }
}



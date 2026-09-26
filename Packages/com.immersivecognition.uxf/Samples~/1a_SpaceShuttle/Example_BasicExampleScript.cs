using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// add the UXF namespace
using UXF;

namespace UXFExamples
{
    /// <summary>
    /// Example script used to test functionality of the Experiment Manager
    /// </summary>
    public class Example_BasicExampleScript : MonoBehaviour
    {

        public GameObject spaceShuttle;
        public Example_Engine spaceShuttleEngine;

        /// <summary>
        /// generates the trials and blocks for the session
        /// </summary>
        /// <param name="session"></param>
        public void GenerateExperiment(Session session)
        {
            // This function can be called using the Session inspector OnSessionBegin() event, or otherwise


            // / In the StreamingAssets folder we have a several .json files that contain settings, e.g.
            // / that looks like this:
            // 
            //  {
            //
            //  "n_trials": 10,
            //  "size": 1        
            //
            //  }        
            //
            /* You can add any new settings to the JSON file
            it will automatically be loaded into the settings property
            of an ExperimentSession component as .settings */

            // create our blocks & trials

            // retrieve the n_trials setting, which was loaded from our .json file into our session settings
            // if we dont supply a settings profile (e.g. when running in a web browser, or Oculus Quest) it will revert to valueIfNotFound (10)
            int numMainTrials = session.settings.GetInt("n_trials", valueIfNotFound: 10);
            // create the block
            Block mainBlock = session.CreateBlock(numMainTrials);

            // here we set a setting for the 2nd trial of the main block as an example.
            // all other trials will have whatever value is set in the session settings (basic_example_*.json file)
            mainBlock.GetRelativeTrial(2).settings.SetValue("size", 1.5f);
            mainBlock.lastTrial.settings.SetValue("size", 6f);

            // lets set every trial to have a random thrust between two values
            foreach (var trial in mainBlock.trials)
            {
                trial.settings.SetValue("thrust", Random.Range(0.5f, 2f));
            }
        }

        /// <summary>
        /// Example method presenting a stimulus to a user
        /// </summary>
        /// <param name="trial"></param>
        public void PresentStimulus(Trial trial)
        {
            // we can call this function via the event "On Trial Begin", which is called when the trial starts
            // here we can imagine presentation of some stimulus
            Debug.LogFormat("Running trial {0}", trial.number);

            // we can access our settings to (e.g.) modify our scene
            // for more information about retrieving settings see the documentation
            float thrust = trial.settings.GetFloat("thrust");
            
            // and set the engine's thrust.
            spaceShuttleEngine.thrust = trial.settings.GetFloat("thrust");
            Debug.LogFormat("The 'thrust' for this trial is: {0}", thrust);

            // end trial and prepare next trial in 5 seconds
            Invoke("EndAndPrepare", 5);
        }


        void EndAndPrepare()
        {
            // record the altitude of the space shuttle
            float altitude = spaceShuttle.transform.position.y;
            Debug.LogFormat("The space shuttle got an altitude of {0}!", altitude);

            // adding data to the .result of a trial will automatically log it to the trial results (by default a CSV)
            Session.instance.CurrentTrial.result["altitude"] = altitude;

            // end the trial
            Debug.Log("Ending trial");
            Session.instance.CurrentTrial.End();

            // reset the shuttle's position and thrust
            spaceShuttle.transform.localPosition = Vector3.zero;
            spaceShuttleEngine.thrust = 0f;

            // if last trial, end session.
            if (Session.instance.CurrentTrial == Session.instance.LastTrial)
            {
                Session.instance.End();
            }
            else
            {
                // begin next after 2 second delay
                Invoke("BeginNext", 2);
            }
        }

        void BeginNext()
        {
            Session.instance.BeginNextTrial();
        }
    }
}


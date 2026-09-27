using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UXF namespace
using UXF;


namespace UXFExamples
{
    /// <summary>
    /// This script controls the set of cubes
    /// </summary>
    public class Example_CorsiCubeGroup: MonoBehaviour
    {
        
        public Example_CorsiCube cubePrefab;

        public Button confirmButton;

        List<int> clicks = new List<int>();

        int numSequence;

        public void CreateCubesAndPlay(Trial trial)
        {
            StopAllCoroutines();
            // run in a coroutines to take advantage of delays
            StartCoroutine(CreateCubesAndPlaySequence(trial));
        }

        public IEnumerator CreateCubesAndPlaySequence(Trial trial)
        {
            // clear clicks from last trial and destory all previous cubes (children)
            clicks.Clear();
            foreach (Transform child in transform) Destroy(child.gameObject);

            // after a delay, create the cubes
            yield return new WaitForSeconds(0.25f);

            // grab the positions from the trial settings
            Vector3[] positions = (Vector3[]) trial.settings.GetObject("positions");

            // create a list to store the cubes
            List<Example_CorsiCube> cubes = new List<Example_CorsiCube>(9);

            int cubeNum = 0;
            foreach (var pos in positions)
            {
                // create a cube
                Example_CorsiCube newCube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                newCube.id = cubeNum;
                // give it a reference to this script, so it can tell this script when it has been clicked
                newCube.cubeGroup = this;
                cubes.Add(newCube);

                cubeNum++;                
            }

            // because the items are shuffled, we can just take the first N as the sequence that the user should recall
            // now we play the first N cubes in the sequence with a delay between each one
            numSequence = trial.settings.GetInt("num_sequence");

            foreach (var cube in cubes)
            {
                if (cube.id < numSequence)
                {
                    yield return new WaitForSeconds(0.5f); // wait half second
                    cube.LightUp();
                }
            }

            // after a short delay set them all to clickable
            yield return new WaitForSeconds(0.25f);
            foreach (var cube in cubes)
            {
                cube.clickable = true;
            }

            // show confirm button
            confirmButton.gameObject.SetActive(true);
        }        

        /// <summary>
        /// Called when user clicks on any cube
        /// </summary>
        public void RegisterClick(int cubeID)
        {
            Debug.LogFormat("Clicked cube number {0}!", cubeID);
            clicks.Add(cubeID);
        }


        /// <summary>
        /// Called when user clicks confirm button.
        /// </summary>
        public void Confirm()
        {
            // if somehow the button is clicked when we are not in a trial, do nothing
            if (!Session.instance.InTrial) return;

            // compare the clicks vs the expected input
            bool allCorrect = true;
            if (clicks.Count == numSequence)
            {
                // we know the first N are the ones we should have clicked
                // e.g. 1, 2, 3, 4 - if there was 4 in the sequence
                for (int n = 0; n < numSequence; n++)
                {
                    if (clicks[n] != n)
                    {
                        allCorrect = false;
                        break;
                    }
                }
            }
            else // if wrong length we know it's wrong immediately
            {
                allCorrect = false;
            }


            // record the outcome
            Session.instance.CurrentTrial.result["correct"] = allCorrect;

            // lets also build a Dictionary which UXF can save as JSON for us
            // it will store which items were clicked, and where the cubes were positioned
            // we do this because the data cannot be easily expressed in normal CSV format
            
            Dictionary<string, object> customCorsiResults = new Dictionary<string, object>()
            {
                { "positions", Session.instance.CurrentTrial.settings.GetObject("positions") },
                { "clicks", clicks }
            };

            // this will make UXF automatically save this data as a json file, once per trial, organised into folders (with the FileSaver Data Handler) 
            Session.instance.CurrentTrial.SaveJSONSerializableObject(customCorsiResults, "corsi_results", UXFDataType.OtherTrialData);

            // hide confirm button
            confirmButton.gameObject.SetActive(false);

            // end trial
            Session.instance.CurrentTrial.End();
        }

    }
}
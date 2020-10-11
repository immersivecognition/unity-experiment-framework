using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UXF namespace
using UXF;

// LINQ - for .OrderBy, etc
using System.Linq;

namespace UXFExamples
{
    /// <summary>
    /// This script defines the IVs in the Corsi Block task
    /// </summary>
    public class Example_CorsiBlockTask: MonoBehaviour
    {
        /// <summary>
        /// Store an array of transforms which defines the possible positons that they lie in.
        /// </summary>
        public Transform[] possiblePositions;

        /// <summary>
        /// We will offset the position of the cubes by a little in 2D, to make them not appear in a perfect grid.
        /// </summary>
        public Vector2 randomOffsetMax;


        /// <summary>
        /// reference to feedback script
        /// </summary>
        public Example_CorsiFeedback feedback;


        /// <summary>
        /// Generate blocks and trials. We can call this from OnSessionBegin 
        /// </summary>
        /// <param name="session"></param>
        public void GenerateExperiment(Session session)
        {
            // start at a sequence of 2 squares, then incrementally up to the entire 9
            for (int numSquares = 2; numSquares <= 9; numSquares++)
            {
                // create block of three trials
                Block newBlock = session.CreateBlock(3);
                
                // so just record the number of squares in the sequence. its the same for all 3 trials in the block
                newBlock.settings.SetValue("num_sequence", numSquares);

                // across the three trials, generate the positions the squares will sit in.
                // we can store them in an array of Vector3s
                foreach (Trial trial in newBlock.trials)
                {
                    // shuffle the positions, and take 9
                    Vector3[] squarePositions = possiblePositions
                        .OrderBy(n => Random.value)
                        .Take(9)
                        .Select(t => t.position + new Vector3(RandomBetweenMinus1And1() * randomOffsetMax.x, RandomBetweenMinus1And1() * randomOffsetMax.y))
                        .ToArray();

                    // store them in settings
                    trial.settings.SetValue("positions", squarePositions);
                }
            }
        }

        float RandomBetweenMinus1And1()
        {
            return (2f * Random.value) - 1f;
        }


        public void CheckStatusOrStartNext(Trial trial)
        {
            // end now if this is the last trial in the session
            bool endNow = trial == Session.instance.LastTrial;

            // if last trial in current block
            Block currentBlock = trial.block;
            if (trial == currentBlock.lastTrial && !endNow)
            {
                bool atLeastOneCorrect = false;
                // check if all three trials in current block were incorrect
                foreach (var t in currentBlock.trials)
                {
                    if (!trial.result.ContainsKey("correct"))
                    {
                        // result missing for some reason
                        break;
                    }
                    else if ((bool)trial.result["correct"])
                    {
                        // trial was correct
                        atLeastOneCorrect = true;
                        break;
                    }
                }
                endNow = !atLeastOneCorrect;
            }
            
            // if we didnt get at least one correct, or this is the last trial, end now
            if (endNow)
            {
                // show info, and end
                int span = currentBlock.settings.GetInt("num_sequence");
                feedback.ShowFeedback(string.Format("The end! Your Corsi Block Span is {0}.", span - 1), delay: float.PositiveInfinity);
                Session.instance.Invoke("End", 5); // 5 second delay
            }
            else
            {
                if (!trial.result.ContainsKey("correct")) return;
                // show normal feedback for this trial, and start next trial
                string feedbackText = (bool) trial.result["correct"] ? "Correct! :)" : "Incorrect! :(";
                feedback.ShowFeedback(feedbackText);
                Session.instance.Invoke("BeginNextTrialSafe", 1); // 1 second delay
            }
        }
    }
}
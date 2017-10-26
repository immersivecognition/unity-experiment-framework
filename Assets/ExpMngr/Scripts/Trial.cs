using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Specialized;


namespace ExpMngr
{
    /// <summary>
    /// The base unit of experiments. A Trial is usually a singular attempt at a task by a participant after/during the presentation of a stimulus.
    /// </summary>
    [Serializable]
    public class Trial {

        /// <summary>
        /// Returns non-zero indexed trial number.
        /// </summary>
        public int number { get { return experiment.trials.IndexOf(this) + 1; } }
        /// <summary>
        /// Returns non-zero indexed trial number for the current block.
        /// </summary>
        public int numberInBlock { get { return block.trials.IndexOf(this) + 1; } }
        /// <summary>
        /// Status of the trial (enum)
        /// </summary>
        public TrialStatus status = TrialStatus.NotDone;
        /// <summary>
        ///  The block the trial belongs to
        /// </summary>
        [NonSerialized] public Block block;
        float startTime, endTime;
        ExperimentSession experiment;
        
        /// <summary>
        /// Trial settings. These will override block settings if set.
        /// </summary>
        public Settings settings = Settings.empty;

        /// <summary>
        /// Ordered dictionary of results in a order.
        /// </summary>
        public OrderedResultDict result;

        /// <summary>
        /// Create trial with an associated block
        /// </summary>
        /// <param name="exp">The experiment session the trial belongs to.</param>
        /// <param name="trialBlock">The block the trial belongs to.</param>
        public Trial(ExperimentSession exp, Block trialBlock)
        {
            block = trialBlock;
            block.trials.Add(this);
            experiment = exp;
            settings.SetParent(block.settings);
        }

        /// <summary>
        /// Create trial without an associated block
        /// </summary>
        /// <param name="exp">The experiment session the trial belongs to.</param>
        public Trial(ExperimentSession exp)
        {
            experiment = exp;
            settings.SetParent(experiment.settings);
        }

        /// <summary>
        /// Begins the trial, updating the current trial and block number, setting the status to in progress, starting the timer for the trial, and beginning recording positions of every object with an attached tracker
        /// </summary>
        public void Begin()
        {
            experiment.trialNum = number;
            experiment.blockNum = block.number;

            status = TrialStatus.InProgress;
            startTime = Time.time;
            result = new OrderedResultDict();
            foreach (string h in experiment.headers)
                result.Add(h, string.Empty);

            result["ppid"] = experiment.ppid;
            result["session_num"] = experiment.sessionNum;
            result["trial_num"] = number;
            result["block_num"] = block.number;
            result["trial_num_in_block"] = numberInBlock;
            result["start_time"] = startTime;

            foreach (Tracker tracker in experiment.trackedObjects)
            {
                tracker.StartRecording();
            }

            OnBegin();
        }
        
        /// <summary>
        /// Override this method to create custom behavior on trial start
        /// </summary>
        public virtual void OnBegin()
        {

        }

        /// <summary>
        /// Ends the rial, queues up saving results to output file, stops and saves tracked object data.
        /// </summary>
        public void End()
        {
            status = TrialStatus.Done;
            endTime = Time.time;
            result["end_time"] = endTime;            

            // log tracked objects
            foreach (Tracker tracker in experiment.trackedObjects)
            {
                var trackingData = tracker.StopRecording();
                string dataName = experiment.SaveTrackingData(tracker.objectName, trackingData);
                result[tracker.objectNameHeader] = dataName;
            }

            // log any settings we need to for this trial
            foreach (string s in experiment.settingsToLog)
            {
                result[s] = settings[s];
            }

            OnEnd();
        }

        /// <summary>
        /// Override this method to create custom behavior for after a trial ends.
        /// </summary>
        public virtual void OnEnd()
        {

        }

    }

    /// <summary>
    /// Status of a trial
    /// </summary>
    public enum TrialStatus { NotDone, Preparing, InProgress, Done }


}
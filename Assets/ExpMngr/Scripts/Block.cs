using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpMngr
{
    /// <summary>
    /// A set of trials – often used to group a number of consecutive Trial objects that share something in common.
    /// </summary>
    [Serializable]
    public class Block
    {
        /// <summary>
        /// List of trials associated with this block
        /// </summary>
        public List<Trial> trials = new List<Trial>();
        /// <summary>
        /// Return the first trial in this block
        /// </summary>
        public Trial firstTrial { get { return trials.Count > 0 ? trials[0] : null; } }
        /// <summary>
        /// Return the last trial in this block
        /// </summary>
        public Trial lastTrial { get { return trials.Count > 0 ? trials[trials.Count-1] : null; } }
        /// <summary>
        /// Returns the block number of this block within the experiment session
        /// </summary>
        public int number { get { return experiment.blocks.IndexOf(this) + 1; } }

        /// <summary>
        /// Block settings. These will be overidden by trial settings if set.
        /// </summary>
        public Settings settings = Settings.empty;

        ExperimentSession experiment;

        /// <summary>
        /// Creates a block for an associated experement session
        /// </summary>
        /// <param name="exp">Experiment session</param>
        public Block(ExperimentSession exp)
        {
            experiment = exp;
            experiment.blocks.Add(this);
            settings.SetParent(experiment.settings);
        }

        /// <summary>
        /// Get a trial in this block by relative trial number (non-zero indexed)
        /// </summary>
        /// <param name="relativeTrialNumber">Trial number relative to block (non zero indexed)</param>
        /// <returns></returns>
        public Trial GetRelativeTrial(int relativeTrialNumber)
        {
            return trials[relativeTrialNumber - 1];
        }

    }

}

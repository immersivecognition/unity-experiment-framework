using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UXF
{
    /// <summary>
    /// A set of trials, often used to group a number of consecutive Trial objects that share something in common.
    /// </summary>
    public class Block : IExperimentUnit
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
        /// Returns the block number of this block, based on its position in the session.
        /// </summary>
        public int number { get { return session.blocks.IndexOf(this) + 1; } }

        /// <summary>
        /// Block settings. These will be overridden by trial settings if set.
        /// </summary>
        public Settings settings { get; protected set; }

        /// <summary>
        /// The session associated with this block
        /// </summary>
        public Session session { get; private set; }

        /// <summary>
        /// Should data be saved for this session?
        /// </summary>
        public bool saveData
        {
            get => settings.GetBool(Constants.SAVE_DATA_SETTING_NAME, true);
            set => settings.SetValue(Constants.SAVE_DATA_SETTING_NAME, value);
        }

        /// <summary>
        /// Create a block with a given number of trials under a given session
        /// </summary>
        /// <param name="numberOfTrials"></param>
        /// <param name="session"></param>
        public Block(uint numberOfTrials, Session session)
        {
            settings = Settings.empty;
            this.session = session;
            this.session.blocks.Add(this);
            settings.SetParent(session);
            for (int i = 0; i < numberOfTrials; i++)
            {
                var t = new Trial(this);
                trials.Add(t);
            }
        }

        /// <summary>
        /// Create a trial within this block
        /// </summary>
        /// <returns></returns>
        public Trial CreateTrial()
        {
            var t = new Trial(this);
            trials.Add(t);
            return t;
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

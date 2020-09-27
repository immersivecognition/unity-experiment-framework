using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UXF.UI
{

    public class InfoBarController : MonoBehaviour
    {

        public Session session;

        public Text trialStatus, folder, blockNum, trialNum;

        /// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		void Awake()
        {
            session.onSessionBegin.AddListener(SessionBegin);
            session.onTrialBegin.AddListener(TrialBegin);
            session.onTrialEnd.AddListener(TrialEnd);
            session.onSessionEnd.AddListener(SessionEnd);
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            ResetToNormal();
        }

        void ResetToNormal()
        {
            folder.text = "experiment > ppid > 0";
            trialStatus.text = "Awaiting session start";
            trialNum.text = FormatProgress("Trial", 0, 0);
			blockNum.text = FormatProgress("Block", 0, 0);
        }

        void SessionBegin(Session session)
        {
            trialStatus.text = "Awaiting trial start";
            folder.text = session.experimentName + " > " + session.ppid + " > " + session.number.ToString();
        }

        void TrialBegin(Trial trial)
        {
            trialStatus.text = "Trial in progress";
			trialNum.text = FormatProgress("Trial", trial.number, trial.session.Trials.ToList().Count);
			blockNum.text = FormatProgress("Block", trial.block.number, trial.session.blocks.Count);
        }

        void TrialEnd(Trial trial)
        {
            trialStatus.text = "Trial finished";
        }

        void SessionEnd(Session session)
        {
            ResetToNormal();
        }

        string FormatProgress(string variable, int current, int max)
        {
            string fmt = "{0} {1}/{2}";
            if (max == 0)
            {
                return string.Format(fmt, variable, "??", "??");
            }
            else
            {
                return string.Format(fmt, variable, current, max);
            }
        }

    }

}
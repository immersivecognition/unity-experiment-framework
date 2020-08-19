using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{
	public class Example_TargetManager : MonoBehaviour {

		public Example_TargetController leftTarget;
		public Example_TargetController rightTarget;

		public Session session;

        public void SetupTargets(Trial trial) // can be called from OnTrialBegin in the Session inspector
        {
			TargetPosition correctTargetPosition = (TargetPosition) trial.settings.GetObject("correct_target_position");

            leftTarget.Setup(correctTargetPosition == TargetPosition.Left);
            rightTarget.Setup(correctTargetPosition == TargetPosition.Right);

            bool inverted = trial.settings.GetBool("inverted");

            if (inverted)
			{
				// light up the opposite target
				if (correctTargetPosition == TargetPosition.Left) rightTarget.Highlight();
                if (correctTargetPosition == TargetPosition.Right) leftTarget.Highlight();
			}
			else
			{
                // light up the correct target
                if (correctTargetPosition == TargetPosition.Left) leftTarget.Highlight();
                if (correctTargetPosition == TargetPosition.Right) rightTarget.Highlight();
			}			
 
        }

		public void TargetHit(Example_TargetController target)
		{
			Trial currentTrial = session.CurrentTrial;
			currentTrial.result["correct"] = target.isCorrect;
            currentTrial.End();
		}

        public void ResetToNormal(Trial trial) // can be called from OnTrialEnd in the Session inspector
        {
            leftTarget.ResetToNormal();
            rightTarget.ResetToNormal();
        }

    }


	public enum TargetPosition
	{
		Left, Right
	}

}
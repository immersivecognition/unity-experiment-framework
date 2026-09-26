using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

namespace UXFExamples
{
    public class Example_ReactionTask : MonoBehaviour
    {

		public Example_ReactionStim stim;
		public Text feedbackText;

		public void StartReactionTaskTrial(Trial trial)
		{
			stim.BeginTimer(
				trial.settings.GetFloat("press_delay"),
				trial.settings.GetFloat("fake_delay")
			);
		}

		public void EndReactionTaskTrial(Trial trial)
		{
			// sometimes we quit the application mid session, and that ends the current trial. If that is the case, we don't need to do anything here. 
			if (Session.instance.isEnding) return;

            // show some feedback
			string outcomeText = (string) trial.result["outcome"];
			float rt = (float) trial.result["reaction_time"];

			if (outcomeText == "success")
			{
				feedbackText.text = string.Format("Nice! RT = {0:0}ms", rt * 1000);
			}
			else
			{
				feedbackText.text = "Too early!";
			}
			

			// clear text after 1 second
			Invoke("ClearText", 1f);

			// start next trial after 1.5 seconds
			Invoke("NextTrial", 1.5f); 
		}

		void ClearText()
		{
			feedbackText.text = "";
		}


		void NextTrial()
		{
			if (Session.instance.CurrentTrial == Session.instance.LastTrial)
			{
#if UNITY_EDITOR
            	UnityEditor.EditorApplication.isPlaying = false;
#else
            	Application.Quit();
#endif	
			}
			else
			{
				Session.instance.BeginNextTrial();
			}
		}

    }
}
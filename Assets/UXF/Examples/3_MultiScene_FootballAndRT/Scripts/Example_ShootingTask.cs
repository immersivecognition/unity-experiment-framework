using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

namespace UXFExamples
{
    public class Example_ShootingTask : MonoBehaviour
    {
		public Example_Shooter shooter;
		public Example_ShootTarget target;

		public Text feedbackText;

		public void StartShootingTaskTrial(Trial trial)
		{
			shooter.Ready();
			target.SetSize(trial.settings.GetFloat("target_size"));
		}	


		public void EndShootingTaskTrial(Trial trial)
		{
			string outcomeText = (string) trial.result["outcome"];

			feedbackText.text = outcomeText.ToUpper() + "!!!";

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
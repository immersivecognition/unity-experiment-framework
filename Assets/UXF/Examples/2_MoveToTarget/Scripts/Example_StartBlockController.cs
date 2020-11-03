using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

	/// <summary>
	/// Controls the start block gameobject
	/// </summary>
    public class Example_StartBlockController : MonoBehaviour
    {

		public Color mainColor = Color.red;
        public Color readyColor = new Color(1f, 0.5f, 0f); // orange
        public Color goColor = Color.green;

		/// <summary>
		/// Reference to the associated session.
		/// </summary>
		public Session session;

        StartBlockState state = StartBlockState.Waiting;
		SpriteRenderer spriteRenderer;
		Coroutine runningSequence;


		void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetState(StartBlockState.Waiting);
        }


		IEnumerator RunSequence()
		{
			GetReady();

            // Take the delay time (seconds) for the next trial, wait for that time
            // If we move from the start block too early, StopCoroutine(runningSequence); will halt the execution of this coroutine
            // System.Convert: Safely convert to single (float)
            float delayTime = session.NextTrial.settings.GetFloat("delay_time", 0.5f);
			yield return new WaitForSeconds(delayTime);

			Go();
		}

		void SetState(StartBlockState newState)
		{
			state = newState;

			// modify colour based on state
			switch (state)
			{
				// could be dictionary
				case StartBlockState.Waiting:
                    spriteRenderer.color = mainColor;
					break;
                case StartBlockState.GetReady:
                    spriteRenderer.color = readyColor;
                    break;
                case StartBlockState.Go:
                    spriteRenderer.color = goColor;
                    break;
			}
		}

        public void ResetToNormal()
        {
			Debug.Log("Resetting");
			SetState(StartBlockState.Waiting);
        }

		void GetReady()
		{
            Debug.Log("Get ready...");
			SetState(StartBlockState.GetReady);
		}

		void Go()
		{
            Debug.Log("Go!");
            SetState(StartBlockState.Go);

			// now begin the next trial.
			session.BeginNextTrial();
		}

		void OnMouseEnter()
		{
            switch (state)
            {
				case StartBlockState.Waiting:
					// begin the sequence
				    runningSequence = StartCoroutine(RunSequence());
					break;
			}	
		}

		void OnMouseExit()
		{
            switch (state)
            {
                case StartBlockState.GetReady:
					// stop the sequence
                    Debug.Log("Moved too early!");
                    StopCoroutine(runningSequence);
                    ResetToNormal();
                    break;
				case StartBlockState.Go:
					// record the time we moved
                    session.CurrentTrial.result["moved_time"] = Time.time;
					break;
            }
        }
		

    }

	public enum StartBlockState 
	{
		Waiting, GetReady, Go
	}


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

	/// <summary>
	/// Controls the start block gameobject
	/// </summary>
    public class StartBlockController : MonoBehaviour
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

		void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetState(StartBlockState.Waiting);
        }

		public void SetState(StartBlockState newState)
		{
            StartBlockState oldState = state;
			state = newState;

			switch (state)
			{
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

			// Take the delay time (seconds) for the next trial, and invoke the "Go" method after that time
			// safely convert to single (float)
			float delayTime = System.Convert.ToSingle(session.nextTrial.settings["delay_time"]);
			Invoke("Go", delayTime);
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
                    GetReady();
					break;
			}	
		}

		void OnMouseExit()
		{
            switch (state)
            {
                case StartBlockState.GetReady:
                    CancelInvoke("Go");
                    ResetToNormal();
                    break;
				case StartBlockState.Go:
					// record the time we moved
                    session.currentTrial.result["moved_time"] = Time.time;
					break;
            }
        }
		

    }

	public enum StartBlockState 
	{
		Waiting, GetReady, Go
	}


}
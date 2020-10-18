using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{
    public class Example_ReactionStim : MonoBehaviour
    {

        public Color pressColor = Color.green;
        public Color fakeColor = Color.red;
        public MeshRenderer rend;
        ReactionState currentState;
        float shownTime;
        Color originalColor;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            originalColor = rend.material.color;
        }

        public void BeginTimer(float pressDelay, float fakeDelay)
        {
            currentState = ReactionState.Waiting;
            StartCoroutine(TaskSequence(pressDelay, fakeDelay));
        }

        // a sequence that runs as a coroutine
        IEnumerator TaskSequence(float pressDelay, float fakeDelay)
        {
            if (fakeDelay < pressDelay)
            {
                // wait then show the fake color
                yield return new WaitForSeconds(fakeDelay);
                rend.material.color = fakeColor;
                currentState = ReactionState.Fake;
                
                // wait some more time then show the press color
                yield return new WaitForSeconds(pressDelay - fakeDelay);
                rend.material.color = pressColor;
                currentState = ReactionState.Press;
                shownTime = Time.time;
            }
            else
            {
                // wait then show the real color
                yield return new WaitForSeconds(fakeDelay);
                rend.material.color = pressColor;
                currentState = ReactionState.Press;
                shownTime = Time.time;
            }
        }




        /// <summary>
        /// OnMouseDown is called when the user has pressed the mouse button while
        /// over the GUIElement or Collider.
        /// </summary>
        void OnMouseDown()
        {
            // stops the running task sequence
            StopAllCoroutines();

            // reset color
            rend.material.color = originalColor;

            // calculate reaction time
            float rt = Time.time - shownTime;
            Session.instance.CurrentTrial.result["reaction_time"] = rt;

            // what state are we in? 
            if (currentState == ReactionState.Press)
            {
                Session.instance.CurrentTrial.result["outcome"] = "success";                
            }
            else
            {
                Session.instance.CurrentTrial.result["outcome"] = "too_early";
            }

            Session.instance.CurrentTrial.End();
        }

    }

    public enum ReactionState
    {
        Waiting, Fake, Press
    }

}
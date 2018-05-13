using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UXF
{	
	[ExecuteInEditMode]
	public class SessionDebugger : MonoBehaviour
	{
		[InspectorButton("DisplaySessionProtocol")]
		public bool displaySessionProtocol;

		[TextArea]
		public string sessionProtocol;

		Session session;

		void Awake()
		{
			if (!Application.isEditor)
			{
                Destroy(this);
            }
			else
			{
                ResetDisplay();
                session = GetComponent<Session>();
            }
		}

		private void DisplaySessionProtocol()
		{
#if UNITY_EDITOR
			CancelInvoke("ResetDisplay");

			if(!Application.isPlaying)
			{
                sessionProtocol = "Make sure you are in play mode before displaying the session protocol";
                Invoke("ResetDisplay", 5);
				return;
			}
			else if (session.blocks.Count == 0)
			{
                sessionProtocol = "No blocks have been added to your session yet!";
                Invoke("ResetDisplay", 5);
                return;
            }

            sessionProtocol = "Serializing...";

            Dictionary<string, object> protocol = new Dictionary<string, object>();
            // log each session setting
            foreach (string key in session.settings.Keys)
                protocol.Add(key, session.settings[key]);

            foreach (var block in session.blocks)
			{
                Dictionary<string, object> blockDict = new Dictionary<string, object>();
				// log each block setting
                foreach (string key in block.settings.Keys)
					blockDict.Add(key, block.settings[key]);

				// log each trial
                foreach (var trial in block.trials)
				{
                    Dictionary<string, object> trialDict = new Dictionary<string, object>();
                    // log each trial setting
                    foreach (string key in trial.settings.Keys)
                        trialDict.Add(key, trial.settings[key]);

					// add trial to block
                    blockDict.Add(
                        string.Format("trial_{0}", trial.number),
                        trialDict
					);
				}
                    
				
                protocol.Add(
					string.Format("block_{0}", block.number),
					blockDict
				);
			}
			
            sessionProtocol = MiniJSON.Json.Serialize(protocol);
			
#endif
		}

		private void ResetDisplay()
		{
			sessionProtocol = "Press the above button after generating your blocks and trials to list all blocks and trials with their associated setting in the session.";
		}


	}
}
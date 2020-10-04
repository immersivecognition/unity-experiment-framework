using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXFExamples
{

    public class Example_InitialiseSessionManually : MonoBehaviour
    {

        public UXF.Session session;

		private const string jsonString =
        @"{
			""n_practice_trials"": 5,
			""n_main_trials"": 10,
			""size"": 1
		}";

        void Start()
        {
            // here we can programatically obtain experiment name, ppid, etc
            // we could acquire this information some other way, such as your own UI.
            string experimentName = "example_manual";
            string ppid = "example_ppid";

			// here we make a settings object from a JSON string - this could, for example, be pulled from a web API.
            UXF.Settings sessionSettings = new UXF.Settings(
				(Dictionary<string, object>) MiniJSON.Json.Deserialize(jsonString)
			);		

            session.Begin(experimentName, ppid, settings: sessionSettings);

            // herein everything is the same
        }

    }

}
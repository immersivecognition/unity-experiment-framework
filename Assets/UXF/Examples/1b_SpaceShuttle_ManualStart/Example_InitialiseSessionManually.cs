using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXFExamples
{

    public class Example_InitialiseSessionManually : MonoBehaviour
    {

        public UXF.Session session;

		public InputField ppidField;
        public InputField favouriteColourField;
        public InputField favouriteFoodField;
        public InputField numTrialsField;


        public void BeginSessionManually()
        {
            // 
            // :: We will call this method by pressing the Begin Session button on the UI.
            //

            // here we can programatically obtain ppid from the UI.
            string ppid = ppidField.text;

            // if ppid empty, throw an error
            if (ppid.Trim() == string.Empty) throw new System.Exception("Error! PPID is blank!");

            // we take the text from the input boxes and store it in participant details.
            Dictionary<string, object> myParticipantDetails = new Dictionary<string, object>()
            {
                { "favourite_colour", favouriteColourField.text },
                { "favourite_food", favouriteFoodField.text }
            };

            // we take the text from the num trials input and convert to int
            int numTrials = System.Convert.ToInt32(numTrialsField.text);

            // if less than or equal to zero, throw an error
            if (numTrials <= 0) throw new System.Exception("Error! Number of trials must be greater than 0!");

            // store the value in a Settings object
            UXF.Settings mySettings = new UXF.Settings();
            mySettings.SetValue("n_trials", numTrials);

            // begin the session with our new values.
            // settings and participant details are optional
            string experimentName = "example_manual_start";
            session.Begin(experimentName, ppid, settings: mySettings, participantDetails: myParticipantDetails);
        }

    }

}
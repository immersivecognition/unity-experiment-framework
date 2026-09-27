using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// add the UXF namespace
using UXF;

namespace UXFExamples
{
    /// <summary>
    /// Example script used to test functionality of the Experiment Manager
    /// </summary>
    public class Example_DisplayEndSessionMessage : MonoBehaviour
    {
        
        public Text displayText;

        public void DisplayEndSessionMessage(Session session)
        {

            gameObject.SetActive(true);

            // UXF can be used with local files (e.g. on PC), or without local files
            // we check for that, and display a message if there is a local data handler

            // using some LINQ

            var localFileDataHanders = session
                .ActiveDataHandlers
                .Where(dh => dh is LocalFileDataHander);

            if (localFileDataHanders.Count() > 0)
            {
                string path = localFileDataHanders
                    .Cast<LocalFileDataHander>()
                    .First()
                    .StoragePath;

                displayText.text = "Session complete! The data are stored in " + path;
            }
            else
            {
                displayText.text = "Session complete!";
            }

        }

    }
}


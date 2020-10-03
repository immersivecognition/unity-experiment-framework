using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    [ExecuteAlways]
    public class TextElementSetup : MonoBehaviour
    {

        public InputField content;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            Func<object> get = () => { return content.text; };
            Action<object> set = (value) => { content.text = (string) value; };

            GetComponent<FormElement>().Initialise(get, set);
        }

    } 
}
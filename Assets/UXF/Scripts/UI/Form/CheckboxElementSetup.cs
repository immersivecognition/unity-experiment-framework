using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    [ExecuteAlways]
    public class CheckboxElementSetup : MonoBehaviour
    {

        public Toggle content;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            Func<object> get = () => { return content.isOn; };
            Action<object> set = (value) => { content.isOn = (bool) value; };
            GetComponent<FormElement>().Initialise(get, set);
        }

    } 
}
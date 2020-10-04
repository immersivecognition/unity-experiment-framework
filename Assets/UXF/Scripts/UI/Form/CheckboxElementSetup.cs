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
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            Func<object> get = () => { return content.isOn; };
            Action<object> set = (value) => { content.isOn = (bool) value; };
            GetComponent<FormElement>().Initialise(get, set);
        }
    } 
}
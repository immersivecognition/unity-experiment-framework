using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    [ExecuteAlways]
    public class DropDownElementSetup : MonoBehaviour
    {

        public Dropdown content;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            Func<object> get = () => { return content.options[content.value].text; };
            Action<object> set = (value) => { UpdateOptions((IEnumerable<string>) value); };

            GetComponent<FormElement>().Initialise(get, set);
        }

        public void UpdateOptions(IEnumerable<string> options)
        {
            content.options = new List<Dropdown.OptionData>();
            content.AddOptions(options.ToList());
        }

    } 
}
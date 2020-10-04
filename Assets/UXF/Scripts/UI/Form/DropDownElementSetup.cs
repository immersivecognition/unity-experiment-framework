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
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            Func<object> get = () => { return content.options[content.value].text; };
            Action<object> set = (value) => {
                if (value is IEnumerable<string> valueList)
                    UpdateOptions(valueList);
                else if (value is Tuple<int, IEnumerable<string>> selectedAndValueList)
                    UpdateOptions(selectedAndValueList.Item1, selectedAndValueList.Item2);
                else throw new InvalidCastException();
            };

            GetComponent<FormElement>().Initialise(get, set);
        }

        public void UpdateOptions(IEnumerable<string> options)
        {
            content.options = new List<Dropdown.OptionData>();
            content.AddOptions(options.ToList());
            content.RefreshShownValue();
        }

        public void UpdateOptions(int selectedIdx, IEnumerable<string> options)
        {
            content.options = new List<Dropdown.OptionData>();
            content.AddOptions(options.ToList());
            content.value = selectedIdx;
            content.RefreshShownValue();
        }

    } 
}
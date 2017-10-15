using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExpMngr
{
    public class DropDownController : FormElementController
    {
        public Dropdown dropdown;
        protected override void Setup()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(entry.dropDownValues);
        }

        public override object GetContents()
        {
            return entry.dropDownValues[dropdown.value];
        }

    }
}
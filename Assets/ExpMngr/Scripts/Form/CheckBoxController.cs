using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ExpMngr
{
    public class CheckBoxController : FormElementController
    {
        public Toggle toggle;

        public override object GetContents()
        {
            return toggle.isOn;
        }

        public override void SetContents(object newContents)
        {
            toggle.isOn = ((string) newContents).ToLower() == "true";
        }

        public override void Clear()
        {
            toggle.isOn = false;
        }
    }
}
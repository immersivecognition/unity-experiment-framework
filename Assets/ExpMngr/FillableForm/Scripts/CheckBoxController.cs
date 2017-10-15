using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBoxController : FormElementController
{
    public Toggle toggle;

    public override object GetContents()
    {
        return toggle.isOn;
    }

}

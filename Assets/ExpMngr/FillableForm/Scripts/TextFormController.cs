using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFormController : FormElementController
{
    public Text placeholder;
    public Text enteredText;

    public override object GetContents()
    {
        switch (dataType)
        {
            case FormDataType.Float:
                return float.Parse(enteredText.text);
            case FormDataType.Int:
                return int.Parse(enteredText.text);
            case FormDataType.String:
                return enteredText.text;
            default:
                throw new System.Exception("Datatype undefined");
        }
    }

    protected override void Setup()
    {
        string pText;
        switch (dataType)
        {
            case FormDataType.Float:
                pText = string.Format("Enter number... [float]");
                break;
            case FormDataType.Int:
                pText = string.Format("Enter number... [int]");
                break;
            case FormDataType.String:
                pText = string.Format("Enter text... [string]");
                break;
            default:
                throw new System.Exception("Datatype undefined");
        }
        placeholder.text = pText;
    }

}

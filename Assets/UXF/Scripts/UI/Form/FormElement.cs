using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF.UI
{
    public abstract class FormElement : MonoBehaviour
    {
        public abstract object GetContents();
        public abstract void DisplayFault();
    } 
}
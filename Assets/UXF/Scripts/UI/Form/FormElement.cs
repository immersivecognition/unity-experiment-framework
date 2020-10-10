using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    public class FormElement : MonoBehaviour
    {
        public Text title;

        [SerializeField]
        private UnityEvent onDisplayFault = new UnityEvent();
        private Func<object> getContentsAction = () => { return null; };
        private Action<object> setContentsAction = (o) => { };
        private Action<FormDataType> setDataTypeAction = (o) => { };

        public void Initialise(Func<object> getContentsAction, Action<object> setContentsAction, Action<FormDataType> setDataTypeAction = null)
        {
            this.getContentsAction = getContentsAction;
            this.setContentsAction = setContentsAction;
            if (setDataTypeAction != null) this.setDataTypeAction = setDataTypeAction;
        }

        public void DisplayFault()
        {
            onDisplayFault.Invoke();
        }

        public object GetContents()
        {
            return getContentsAction.Invoke();
        }

        public void SetContents(object contents)
        {
            setContentsAction.Invoke(contents);
        }

        public void SetDataType(FormDataType formDataType)
        {
            setDataTypeAction.Invoke(formDataType);
        }

    }

    [System.Serializable]
    public class FormElementEntry
    {
        [Tooltip("The name displayed in the UI.")]
        public string displayName = "Enter your age";

        [Tooltip("The name used to access the value internally from the session.participantDetails dictionary.")]
        public string internalName = "Age";

        [Tooltip("The type of data you want this form element to collect.")]
        public FormDataType dataType = FormDataType.Int;
        public List<string> dropDownOptions = new List<string>() { "Option 1", "Option 2" };

        public FormElement element { get; set; }

    }


    public enum FormDataType
    {
        String, Float, Int, Bool, DropDown
    }

}
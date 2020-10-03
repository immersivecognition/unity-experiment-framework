using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UXF.UI
{
    public abstract class FormElementController : MonoBehaviour
    {

        public RectTransform rectTransform;
        public Text title;
        [HideInInspector]
        public FormDataType dataType;
        protected string originalTitle;

        [HideInInspector]
        public FormElementEntry entry;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Setup(string titleText, FormDataType dType, FormElementEntry entry)
        {
            this.entry = entry;
            title.text = titleText;
            originalTitle = titleText;
            dataType = dType;
            Setup();
        }

        virtual protected void Setup() { }

        abstract public void Clear();

        abstract public object GetContents();


        abstract public void SetContents(object newContents);

        virtual public object GetDefault()
        {
            switch (dataType)
            {
                case FormDataType.Float:
                    return new float();
                case FormDataType.Int:
                    return new int();
                case FormDataType.String:
                    return "";
                case FormDataType.Bool:
                    return false;
                case FormDataType.DropDown:
                    return 0;
                default:
                    throw new System.Exception("Datatype undefined");
            }
        }

        public void DisplayFault()
        {
            title.text = string.Format("{0} <b><color=red>(!) Error</color></b>", originalTitle);
            Invoke("ResetTitle", 5);
        }

        public void DisplayFault(string message)
        {
            title.text = string.Format("{0} <b><color=red>(!) Error: {1}</color></b>", originalTitle, message);
            Invoke("ResetTitle", 8);
        }

        public void ResetTitle()
        {
            title.text = originalTitle;
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
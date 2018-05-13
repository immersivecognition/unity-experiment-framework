using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UXF
{
    public class FormElementController : MonoBehaviour
    {

        public RectTransform rectTransform;
        public Text title;
        [HideInInspector]
        public FormDataType dataType;
        protected string originalTitle;

        public float height { get { return rectTransform.rect.height; } }

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

        virtual protected void Setup()
        {

        }

        virtual public void Clear()
        {
            
        }

        virtual public object GetContents()
        {
            return null;
        }

        virtual public void SetContents(object newContents)
        {
            
        }

        public virtual object GetDefault()
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

        public void ResetTitle()
        {
            title.text = originalTitle;
        }

    }
}
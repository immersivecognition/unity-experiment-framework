using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UXF
{
    public class FillableFormController : MonoBehaviour
    {

        [Space]

        public GameObject contentParent;
        public GameObject textPrefab;
        public GameObject checkBoxPrefab;
        public GameObject dropDownPrefab;

        List<FormElementEntry> formElements;

        [HideInInspector]
        public FormElementEntry ppidElement;

        public void Clear()
        {
            foreach (var form in formElements)
            {
                form.controller.Clear();
            }
        }

        public void Generate(List<FormElementEntry> formElements, bool insertPpidElement)
        {
            if (insertPpidElement)
            {
                ppidElement = new FormElementEntry();
                ppidElement.displayName = "Participant ID";
                ppidElement.internalName = "ppid";
                ppidElement.dataType = FormDataType.String;
                formElements.Insert(0, ppidElement);

            }

            this.formElements = formElements;
            while (contentParent.transform.childCount != 0)
            {
                DestroyImmediate(contentParent.transform.GetChild(0).gameObject);
            }

            List<string> names = new List<string>();
            foreach (FormElementEntry f in formElements)
            {
                if (f.displayName.Replace(" ", string.Empty) == string.Empty)
                    throw new System.Exception("Bad display name in form element");

                if (f.internalName.Replace(" ", string.Empty) == string.Empty)
                    throw new System.Exception("Bad internal name in form element");

                if (!names.Contains(f.internalName))
                {
                    names.Add(f.internalName);
                    GameObject createdElement = CreateElement(f);
                    f.Initialise(createdElement.GetComponent<FormElementController>());
                }
                else
                {
                    throw new System.Exception("Duplicated internal names. They must be unique.");
                }
            }

            if (insertPpidElement)
            {
                // set ppid field to current time
                TextFormController ppidText = (TextFormController)ppidElement.controller;
                ppidText.SetToTimeNow();
            }

        }



        GameObject CreateElement(FormElementEntry formElementEntry)
        {
            switch (formElementEntry.dataType)
            {
                case FormDataType.String:
                case FormDataType.Int:
                case FormDataType.Float:
                    return Instantiate(textPrefab, contentParent.transform);
                case FormDataType.Bool:
                    return Instantiate(checkBoxPrefab, contentParent.transform);
                case FormDataType.DropDown:
                    return Instantiate(dropDownPrefab, contentParent.transform);
                default:
                    return null;
            }
        }

        public void CompleteForm()
        {
            GetCompletedForm();
        }

        public Dictionary<string, object> GetCompletedForm()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            bool fault = false;
            foreach (FormElementEntry f in formElements)
            {
                try
                {
                    dict.Add(f.internalName, f.controller.GetContents());
                    f.controller.ResetTitle();
                }
                catch (System.FormatException)
                {
                    fault = true;
                    f.controller.DisplayFault();
                }
            }
            if (!fault)
                return dict;
            else
                return null;
        }
    }

    [System.Serializable]
    public class FormElementEntry
    {
        public string displayName;
        public string internalName;
        public FormDataType dataType;
        public List<string> dropDownValues = new List<string>() { "Option 1", "Option 2" };

        public FormElementController controller { get; set; }


        public void Initialise(FormElementController formElementController)
        {
            controller = formElementController;
            controller.Setup(displayName, dataType, this);
        }

    }


    public enum FormDataType
    {
        String, Float, Int, Bool, DropDown
    }
}
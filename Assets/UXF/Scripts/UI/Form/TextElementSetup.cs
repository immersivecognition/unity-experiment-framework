using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UXF.UI
{
    [ExecuteAlways]
    public class TextElementSetup : MonoBehaviour
    {

        public InputField content;

        public string initialText = "";

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        void OnValidate()
        {
            if (initialText != string.Empty) content.text = initialText;
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if (initialText != string.Empty) content.text = initialText;
            Func<object> get = () => { return content.text; };
            Action<object> set = (value) => { content.text = (string) value; };

            Action<FormDataType> setDType = (dType) => {
                switch (dType)
                {
                    case FormDataType.Float:
                        content.contentType = InputField.ContentType.DecimalNumber;
                        break;
                    case FormDataType.Int:
                        content.contentType = InputField.ContentType.IntegerNumber;
                        break;
                    case FormDataType.String:
                        content.contentType = InputField.ContentType.Standard;
                        break;
                    default:
                        throw new Exception("Data type incompatible with TextElement");
                }
            };

            GetComponent<FormElement>().Initialise(get, set, setDType);
        }

    } 
}
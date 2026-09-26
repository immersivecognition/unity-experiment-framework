using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UXF.UI
{
    /// <summary>
    /// A script that can be added to an object with an input field, when enter is pressed while editing the input field a UnityEvent will be invoked
    /// </summary>
    public class InputFieldKeyboardReturnEvent : MonoBehaviour
    {
        private InputField inputField;

        public UnityEvent onReturn;

        void Start()
        {
            inputField = GetComponent<InputField>();
        }

        void OnGUI()
        {
            if (inputField.isFocused &&
            	!string.IsNullOrEmpty(inputField.text) &&
				(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                onReturn.Invoke();
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
    }
}

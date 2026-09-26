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

        void OnEnable()
        {
            inputField = GetComponent<InputField>();
#if ENABLE_INPUT_SYSTEM
            // OnGUI does not receive keyboard events in Input System-only players.
            inputField.onSubmit.AddListener(Submit);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        void OnDisable()
        {
            if (inputField != null) inputField.onSubmit.RemoveListener(Submit);
        }

        void Submit(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            onReturn.Invoke();
            StartCoroutine(RefocusNextFrame());
        }

        IEnumerator RefocusNextFrame()
        {
            // InputField finishes deactivation after invoking onSubmit.
            yield return null;
            if (inputField == null || !inputField.isActiveAndEnabled) yield break;
            inputField.Select();
            inputField.ActivateInputField();
        }
#else
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
#endif
    }
}

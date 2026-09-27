using System;
#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
#endif
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UXF
{
    /// <summary>
    /// Pointer and key input used by UXF components and samples. Uses the Input
    /// System when it is active, including projects with both backends enabled.
    /// </summary>
    public static class UXFInput
    {
#if ENABLE_INPUT_SYSTEM
        private static readonly Dictionary<KeyCode, Key> keyMap = new Dictionary<KeyCode, Key>();
#endif
        public static Vector2 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null) return Mouse.current.position.ReadValue();
                return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }

        public static bool GetMouseButtonDown(int button)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                switch (button)
                {
                    case 0: return mouse.leftButton.wasPressedThisFrame;
                    case 1: return mouse.rightButton.wasPressedThisFrame;
                    case 2: return mouse.middleButton.wasPressedThisFrame;
                    default: return false;
                }
            }
            return button == 0 && Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(button);
#endif
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            if (keyCode == KeyCode.Mouse0) return GetMouseButtonDown(0);
            if (keyCode == KeyCode.Mouse1) return GetMouseButtonDown(1);
            if (keyCode == KeyCode.Mouse2) return GetMouseButtonDown(2);

            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (!keyMap.TryGetValue(keyCode, out var key))
            {
                if (!TryMapKey(keyCode, out key)) key = Key.None;
                keyMap[keyCode] = key;
            }
            if (key == Key.None) return false;
            var control = keyboard[key];
            return control != null && control.wasPressedThisFrame;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static bool TryMapKey(KeyCode keyCode, out Key key)
        {
            var name = keyCode.ToString();
            switch (keyCode)
            {
                case KeyCode.Return: name = "Enter"; break;
                case KeyCode.KeypadEnter: name = "NumpadEnter"; break;
                case KeyCode.LeftControl: name = "LeftCtrl"; break;
                case KeyCode.RightControl: name = "RightCtrl"; break;
                case KeyCode.Numlock: name = "NumLock"; break;
                case KeyCode.Print: name = "PrintScreen"; break;
                case KeyCode.KeypadPeriod: name = "NumpadPeriod"; break;
                case KeyCode.KeypadDivide: name = "NumpadDivide"; break;
                case KeyCode.KeypadMultiply: name = "NumpadMultiply"; break;
                case KeyCode.KeypadMinus: name = "NumpadMinus"; break;
                case KeyCode.KeypadPlus: name = "NumpadPlus"; break;
                case KeyCode.KeypadEquals: name = "NumpadEquals"; break;
                default:
                    if (name.StartsWith("Alpha", StringComparison.Ordinal)) name = "Digit" + name.Substring(5);
                    else if (name.StartsWith("Keypad", StringComparison.Ordinal)) name = "Numpad" + name.Substring(6);
                    break;
            }
            return Enum.TryParse(name, out key) && key != Key.None;
        }
#endif
    }
}

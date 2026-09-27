using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UXF.EditorUtils
{
    public static class EditorExtensions
    {
        public static void DrawProperty(this Editor editor, string propertyName, bool includeChildren = true)
        {
            EditorGUILayout.PropertyField(editor.serializedObject.FindProperty(propertyName), includeChildren: includeChildren);
        }

        public static bool MiddleButton(string text, bool enabled = true)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool pressed = GUILayout.Button(text, GUILayout.MaxWidth(200), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            return pressed;
        }
    }
}
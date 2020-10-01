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
    }
}
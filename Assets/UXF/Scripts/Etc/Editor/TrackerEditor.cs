using UnityEditor;
using UnityEngine;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(UXF.Tracker), true)]
    [CanEditMultipleObjects]
    public class TrackerEditor : Editor
    {
        SerializedProperty customHeader, measurementDescriptor;
        
        void OnEnable()
        {
            customHeader = serializedObject.FindProperty("customHeader");
            measurementDescriptor = serializedObject.FindProperty("measurementDescriptor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(customHeader);
            EditorGUI.indentLevel += 1;

            foreach (SerializedProperty element in customHeader)
            {
                GUI.enabled = false;
                EditorGUILayout.TextField(element.stringValue);
                GUI.enabled = true;
            }
            EditorGUI.indentLevel -= 1;
            
            EditorGUILayout.Space();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(measurementDescriptor);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
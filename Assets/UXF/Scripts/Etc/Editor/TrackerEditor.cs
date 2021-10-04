using UnityEditor;
using UnityEngine;
using System.Reflection;

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
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();
            
            serializedObject.Update();

            FieldInfo[] childFields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // draw all default fields
            foreach (FieldInfo field in childFields)
            {
                if (field.IsPublic || field.GetCustomAttribute(typeof(SerializeField)) != null)
                {                    
                    if (field.Name != measurementDescriptor.name && field.Name != customHeader.name)
                    {
                        var prop = serializedObject.FindProperty(field.Name);
                        EditorGUILayout.PropertyField(prop);
                    }
                }
            }

            EditorGUILayout.Space();
            GUI.enabled = false;

            EditorGUILayout.LabelField(customHeader.displayName);
            EditorGUI.indentLevel += 1;

            foreach (SerializedProperty element in customHeader)
            {
                EditorGUILayout.TextField(element.stringValue);
            }
            EditorGUI.indentLevel -= 1;
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(measurementDescriptor.displayName);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.TextField(measurementDescriptor.stringValue);
            EditorGUI.indentLevel -= 1;
            
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
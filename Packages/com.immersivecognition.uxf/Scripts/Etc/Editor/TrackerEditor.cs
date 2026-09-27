using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(Tracker), true)]
    [CanEditMultipleObjects]
    public class TrackerEditor : Editor
    {
        GUIStyle smallText = new GUIStyle();
        Tracker thisTracker;
        
        void OnEnable()
        {
            smallText.font = EditorStyles.miniFont;
            smallText.fontSize = 9;
            thisTracker = (Tracker)serializedObject.targetObject;
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
                    var prop = serializedObject.FindProperty(field.Name);
                    EditorGUILayout.PropertyField(prop);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Custom Header");
            EditorGUILayout.TextField("(\"time\" is added automatically)", smallText);
            EditorGUI.indentLevel += 1;
            GUI.enabled = false;
            EditorGUILayout.TextField(string.Join(", ", thisTracker.CustomHeader));
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Measurement Descriptor");
            EditorGUI.indentLevel += 1;
            GUI.enabled = false;
            EditorGUILayout.TextField(thisTracker.MeasurementDescriptor);
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Example Filename");
            EditorGUI.indentLevel += 1;
            GUI.enabled = false;
            EditorGUILayout.TextField(string.Format("{0}_T001.csv", thisTracker.DataName));
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace UXF.EditorUtils
{
    // IngredientDrawer
    [CustomPropertyDrawer(typeof(DataHandler))]
    public class DataHandlerDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            Rect fieldBox = position;
            fieldBox.width = position.width*0.75f - 5;

            Rect btnBox = position;
            btnBox.width *= 0.25f;
            btnBox.x += position.width*0.75f;

            DataHandler dh = ((DataHandler)property.objectReferenceValue);
            string name = dh == null ? "(Null)" : dh.name;
            EditorGUI.PropertyField(fieldBox, property, new GUIContent(name, "A reference to the data handler instance."));
            
            EditorGUI.BeginDisabledGroup(dh == null);
            if (GUI.Button(btnBox, "Configure"))
            {
                
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }
    }
}


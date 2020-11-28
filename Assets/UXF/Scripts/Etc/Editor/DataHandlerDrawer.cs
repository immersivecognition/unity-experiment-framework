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
            
            EditorGUIUtility.labelWidth = 135f; 

            Rect activeBox = position;
            activeBox.width = 25;
            activeBox.y += 1;

            Rect btnBox = position;
            btnBox.width = 78;
            btnBox.x = position.x + position.width - 78;            

            Rect fieldBox = position;
            fieldBox.width = position.width - (78 + 20 + 5);
            fieldBox.x = position.x + 20;
            fieldBox.y += 1;
            fieldBox.height -= 2;


            DataHandler dh = ((DataHandler)property.objectReferenceValue);
            string name;
            if (dh == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.Toggle(activeBox, GUIContent.none, false);
                EditorGUI.EndDisabledGroup();
                name = " ";
            }
            else
            {
                var obj = new SerializedObject(property.objectReferenceValue);
                var prop = obj.FindProperty("active");
                if (EditorGUI.PropertyField(activeBox, prop, GUIContent.none)) { }
                obj.ApplyModifiedProperties();
                name = dh.name;
            }

            EditorGUI.PropertyField(fieldBox, property, new GUIContent(name, "A reference to a data handler instance."));
            
            EditorGUI.BeginDisabledGroup(dh == null);
            if (GUI.Button(btnBox, "Configure"))
            {
                Selection.activeGameObject = dh.gameObject;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18;
        }
    }
}


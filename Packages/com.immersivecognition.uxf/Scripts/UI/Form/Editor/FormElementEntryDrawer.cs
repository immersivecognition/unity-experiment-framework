using UnityEditor;
using UnityEngine;
using UXF.UI;

namespace UXF.EditorUtils
{
    // IngredientDrawer
    [CustomPropertyDrawer(typeof(FormElementEntry))]
    public class FormElementEntryDrawer : PropertyDrawer
    {


        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty displayNameProp = property.FindPropertyRelative("displayName");
            SerializedProperty internalNameProp = property.FindPropertyRelative("internalName");
            SerializedProperty dataTypeProp = property.FindPropertyRelative("dataType");
            SerializedProperty dropDownValuesProp = property.FindPropertyRelative("dropDownOptions");

            position.height = EditorGUI.GetPropertyHeight(displayNameProp);
            EditorGUI.PropertyField(position, displayNameProp);

            position.y += EditorGUI.GetPropertyHeight(displayNameProp);
            position.height = EditorGUI.GetPropertyHeight(internalNameProp);
            EditorGUI.PropertyField(position, internalNameProp);

            position.y += EditorGUI.GetPropertyHeight(internalNameProp);
            position.height = EditorGUI.GetPropertyHeight(dataTypeProp);
            EditorGUI.PropertyField(position, dataTypeProp);

            FormDataType dataType = (FormDataType) dataTypeProp.intValue;
            if (dataType == FormDataType.DropDown)
            {
                position.y += EditorGUI.GetPropertyHeight(dataTypeProp);
                position.height = EditorGUI.GetPropertyHeight(dropDownValuesProp);
                EditorGUI.PropertyField(position, dropDownValuesProp, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float extra = 4f;
            SerializedProperty displayNameProp = property.FindPropertyRelative("displayName");
            SerializedProperty internalNameProp = property.FindPropertyRelative("internalName");
            SerializedProperty dataTypeProp = property.FindPropertyRelative("dataType");
            SerializedProperty dropDownValuesProp = property.FindPropertyRelative("dropDownOptions");
            FormDataType dataType = (FormDataType) dataTypeProp.intValue;
            
            if (dataType == FormDataType.DropDown)
            {
                return EditorGUI.GetPropertyHeight(displayNameProp)
                    + EditorGUI.GetPropertyHeight(internalNameProp)
                    + EditorGUI.GetPropertyHeight(dataTypeProp)
                    + EditorGUI.GetPropertyHeight(dropDownValuesProp)
                    + extra;
            } 
            else
            {
                return EditorGUI.GetPropertyHeight(displayNameProp)
                    + EditorGUI.GetPropertyHeight(internalNameProp)
                    + EditorGUI.GetPropertyHeight(dataTypeProp)
                    + extra;
            }
        }
    }
}


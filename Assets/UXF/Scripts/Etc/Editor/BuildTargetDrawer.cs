using UnityEditor;
using UnityEngine;

namespace UXF.EditorUtils
{
    // IngredientDrawer
    [CustomPropertyDrawer(typeof(BuildTarget))]
    public class BuildTargetDrawer : PropertyDrawer
    {

        BuildTarget buildTarget;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            
            buildTarget = (BuildTarget)EditorGUI.EnumFlagsField(position, buildTarget);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18;
        }
    }
}


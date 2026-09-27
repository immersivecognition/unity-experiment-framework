using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UXF.EditorUtils
{
    /// <summary>
    /// Small shared base for UXF's two tabbed inspectors.
    ///
    /// Unity 6 renders serialized arrays, events and object references with
    /// the native inspector controls. Keeping only the property-range helpers
    /// here avoids a package-wide fallback editor and reflection over Unity's
    /// inspector internals.
    /// </summary>
    public abstract class UXFEditorBase : Editor
    {
        protected virtual void InitInspector() { }

        protected virtual void DrawInspector()
        {
            DrawPropertiesAll();
        }

        protected virtual void OnEnable()
        {
            InitInspector();
        }

        protected virtual void OnDisable() { }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawInspector();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawProperty(string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, true);
        }

        protected void DrawPropertiesAll()
        {
            DrawProperties(property => true);
        }

        protected void DrawPropertiesFrom(string propertyStart)
        {
            bool started = false;
            DrawProperties(property =>
            {
                if (property.name == propertyStart)
                    started = true;
                return started;
            });
        }

        protected void DrawPropertiesUpTo(string propertyStop)
        {
            DrawProperties(property => property.name != propertyStop);
        }

        protected void DrawPropertiesFromUpTo(string propertyStart, string propertyStop)
        {
            bool started = false;
            DrawProperties(property =>
            {
                if (property.name == propertyStop)
                    return false;
                if (property.name == propertyStart)
                    started = true;
                return started;
            });
        }

        private void DrawProperties(System.Predicate<SerializedProperty> shouldDraw)
        {
            foreach (SerializedProperty property in TopLevelProperties())
            {
                if (property.name == "m_Script")
                    continue;
                if (shouldDraw(property))
                    EditorGUILayout.PropertyField(property, true);
            }
        }

        private IEnumerable<SerializedProperty> TopLevelProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }
    }
}

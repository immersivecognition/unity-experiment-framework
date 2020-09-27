using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BasteRainGames
{
    public class HideIfExampleScript : MonoBehaviour
    {

        public bool showS1;

        [HideIf("showS1", false)]
        public string s1 = "S1";
        [HideIf("showS1", true)]
        public string s2 = "S2 instead";

        [Space(10f)]
        public Object obj;

        [HideIfNotNull("obj")]
        public string objIsNull = "Shown as obj is null!";

        [HideIfNull("obj")]
        public string objIsNotNull = "Shown as obj is not null!";

        [Space(10f)]
        public TestEnum testEnum;

        [HideIfEnumValue("testEnum", HideIf.Equal, (int)TestEnum.Val1)]
        public string isNotVal1 = "Val 1 not selected";

        [HideIfEnumValue("testEnum", HideIf.NotEqual, (int)TestEnum.Val1)]
        public string isVal1 = "Val 1 selected";

        [HideIfEnumValue("testEnum", HideIf.Equal, (int)TestEnum.Val2, (int)TestEnum.Val3)]
        public string isNotVal2Or3 = "Neither Val 2 nor 3 are selected";

        [HideIfEnumValue("testEnum", HideIf.NotEqual, (int)TestEnum.Val2, (int)TestEnum.Val3)]
        public string isVal2Or3 = "Val 2 or 3 are selected";

        public enum TestEnum
        {
            Val1,
            Val2,
            Val3
        }

        //Showing that this works for PropertyDrawers, with inheritance
        public bool hide;

        public TestData a;
        [HideIf("hide", true)]
        public TestData b;
        [HideIf("hide", true)]
        public TestDataParent c;
        public TestDataParent d;

    }



    [Serializable]
    public class TestDataParent
    {
        public int a;
        public string b;
    }

    [Serializable]
    public class TestData : TestDataParent
    {

    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TestDataParent), true)]
    public class TestDataDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var startingX = position.x;

            position.height = EditorGUIUtility.singleLineHeight;
            position.width /= 2f;
            EditorGUI.LabelField(position, "WOO");

            position.x = startingX + position.width;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("a"));

            position.x = startingX;
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.LabelField(position, "Hoo");
            position.x = startingX + position.width;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("b"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 2f;
        }
    }
    #endif

}

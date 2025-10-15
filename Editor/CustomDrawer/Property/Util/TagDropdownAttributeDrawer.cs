using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(TagDropdownAttribute))]
    public class TagDropdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use StringDropdown with string.");
                return;
            }

            property.serializedObject.Update();

            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();

            string[] tagList = InternalEditorUtility.tags;
            int selectedIndex = Array.IndexOf(tagList, property.stringValue);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, tagList.Length - 1);
            property.stringValue = EditorGUI.TagField(position, label.text, tagList[selectedIndex]);

            if (check.changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
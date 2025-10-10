using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(EffectBase), true)]
    public class EffectBaseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float yPos = position.y + spacing;
            bool enterChildren = true;

            for (var iterator = property.Copy(); iterator.NextVisible(enterChildren);)
            {
                if (SerializedProperty.EqualContents(iterator, property.GetEndProperty()))
                {
                    break;
                }

                float fieldHeight = EditorGUI.GetPropertyHeight(iterator, false);
                Rect propRect = new Rect(position.x, yPos, position.width, fieldHeight);
                EditorGUI.PropertyField(propRect, iterator, false);

                yPos += fieldHeight + spacing;
                enterChildren = false;
            }

            EditorGUI.EndProperty();
        }



        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, includeChildren: true);
            height = Mathf.Max(height - EditorGUIUtility.singleLineHeight, 0);
            height += EditorGUIUtility.standardVerticalSpacing;
            return height;
        }
    }
}
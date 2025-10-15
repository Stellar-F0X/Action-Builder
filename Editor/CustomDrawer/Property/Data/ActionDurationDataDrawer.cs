using System;
using System.Linq;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(ActionDurationData))]
    public class ActionDurationDataDrawer : PropertyDrawer
    {
        private const float _LINE_HEIGHT = 20f;

        private const float _SPACING = 2f;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty durationTypeProperty = property.FindPropertyRelative("durationType");
            SerializedProperty durationProperty = property.FindPropertyRelative("duration");
            SerializedProperty cooldownTimeProperty = property.FindPropertyRelative("cooldownTime");

            float currentY = position.y;
            Rect lineRect = new Rect(position.x, currentY, position.width, _LINE_HEIGHT);

            EditorGUI.PropertyField(lineRect, durationTypeProperty);
            currentY += _LINE_HEIGHT + _SPACING;

            ActionDuration durationType = (ActionDuration)durationTypeProperty.enumValueIndex;

            lineRect = new Rect(position.x, currentY, position.width, _LINE_HEIGHT);

            switch (durationType)
            {
                case ActionDuration.Instant:
                {
                    durationProperty.floatValue = 0f;
                    break;
                }

                case ActionDuration.Infinite:
                {
                    durationProperty.floatValue = Mathf.Infinity;
                    break;
                }

                case ActionDuration.Duration:
                {
                    EditorGUI.PropertyField(lineRect, durationProperty);
                    currentY += _LINE_HEIGHT + _SPACING;
                    break;
                }
            }

            lineRect = new Rect(position.x, currentY, position.width, _LINE_HEIGHT);
            EditorGUI.PropertyField(lineRect, cooldownTimeProperty);
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty durationTypeProperty = property.FindPropertyRelative("durationType");

            switch ((ActionDuration)durationTypeProperty.enumValueIndex)
            {
                case ActionDuration.Instant: return _LINE_HEIGHT * 2 + _SPACING * 2;

                case ActionDuration.Infinite: return _LINE_HEIGHT * 2 + _SPACING * 2;

                case ActionDuration.Duration: return _LINE_HEIGHT * 3 + _SPACING * 3;

                default: return _LINE_HEIGHT + _SPACING;
            }
        }
    }
}
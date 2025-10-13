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
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.PropertyField(lineRect, durationProperty);
                    EditorGUI.EndDisabledGroup();
                    break;
                }
                case ActionDuration.Infinite:
                {
                    EditorGUI.BeginDisabledGroup(true);
                    durationProperty.floatValue = Mathf.Infinity;
                    EditorGUI.PropertyField(lineRect, durationProperty);
                    EditorGUI.EndDisabledGroup();
                    break;
                }

                case ActionDuration.Duration:
                {
                    EditorGUI.PropertyField(lineRect, durationProperty);
                    break;
                }
            }

            currentY += _LINE_HEIGHT + _SPACING;
            
            lineRect = new Rect(position.x, currentY, position.width, _LINE_HEIGHT);
            EditorGUI.PropertyField(lineRect, cooldownTimeProperty);

            EditorGUI.EndProperty();
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Duration Type + Duration + Cooldown Time + End Condition = 3줄
            return (_LINE_HEIGHT * 3) + (_SPACING * 2);
        }
    }
}
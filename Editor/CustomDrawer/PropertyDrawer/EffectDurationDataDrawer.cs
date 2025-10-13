using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(EffectDurationData))]
    public class EffectDurationDataDrawer : PropertyDrawer
    {
        private const float _LINE_HEIGHT = 18f;
        
        private const float _SPACING = 2f;

        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty durationData = property.serializedObject.FindProperty("durationData");
            SerializedProperty parentDurationProp = durationData.FindPropertyRelative("duration");
            SerializedProperty durationTypeProp = durationData.FindPropertyRelative("durationType");

            SerializedProperty durationProp = property.FindPropertyRelative("duration");
            SerializedProperty maxAppCountProp = property.FindPropertyRelative("maxApplicationCount");


            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                float parentDuration = parentDurationProp.floatValue;
                ActionDuration duration = (ActionDuration)durationTypeProp.enumValueIndex;
                this.DrawEffectDurations(position, duration, durationProp, maxAppCountProp, parentDuration);

                if (check.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }


        private void DrawEffectDurations(Rect position, ActionDuration type, SerializedProperty durationProp, SerializedProperty maxAppCountProp, float maxDuration)
        {
            Rect maxCountRect = new Rect(position.x, position.y, position.width, _LINE_HEIGHT);
            Rect durationRect = new Rect(position.x, position.y + _LINE_HEIGHT + _SPACING, position.width, _LINE_HEIGHT);

            switch (type)
            {
                case ActionDuration.Instant:
                {
                    maxAppCountProp.intValue = 1;
                    durationProp.floatValue = 0f;

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.IntField(maxCountRect, "Max Count", maxAppCountProp.intValue);
                        EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                    }

                    break;
                }

                case ActionDuration.Infinite:
                {
                    maxAppCountProp.intValue = Mathf.Clamp(maxAppCountProp.intValue, 1, int.MaxValue);
                    durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, 0f, maxDuration);
                    
                    maxAppCountProp.intValue = EditorGUI.IntField(maxCountRect, "Max Count", maxAppCountProp.intValue);
                    durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                    break;
                }

                case ActionDuration.Duration:
                {
                    maxAppCountProp.intValue = Mathf.Clamp(maxAppCountProp.intValue, 1, int.MaxValue);
                    durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, 0f, maxDuration);
                    
                    maxAppCountProp.intValue = EditorGUI.IntField(maxCountRect, "Max Count", maxAppCountProp.intValue);
                    durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);

                    break;
                }
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (_LINE_HEIGHT + _SPACING) * 2 - _SPACING; // 2 lines with spacing
        }
    }
}
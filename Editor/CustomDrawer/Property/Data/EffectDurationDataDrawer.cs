using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(EffectDurationData))]
    public class EffectDurationDataDrawer : PropertyDrawer
    {
        private const float _LINE_HEIGHT = 18f;

        private const float _SPACING = 2f;

        private SerializedObject _actionSerializedObject;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_actionSerializedObject is null)
            {
                SerializedProperty actionProp = property.serializedObject.FindProperty("_action");
                this._actionSerializedObject = new SerializedObject(actionProp.objectReferenceValue);
            }

            Assert.IsNotNull(_actionSerializedObject);

            _actionSerializedObject.Update();

            SerializedProperty parentDurationDataProp = _actionSerializedObject.FindProperty("durationData");
            SerializedProperty parentDurationProp = parentDurationDataProp.FindPropertyRelative("duration");
            SerializedProperty durationTypeProp = parentDurationDataProp.FindPropertyRelative("durationType");

            SerializedProperty delayProp = property.FindPropertyRelative("delay");
            SerializedProperty durationProp = property.FindPropertyRelative("duration");
            SerializedProperty applyCountProp = property.FindPropertyRelative("applyCount");
            SerializedProperty applyIntervalProp = property.FindPropertyRelative("applyInterval");

            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                float parentDuration = parentDurationProp.floatValue;
                ActionDuration duration = (ActionDuration)durationTypeProp.enumValueIndex;

                Rect delayRect = new Rect(position.x, position.y, position.width, _LINE_HEIGHT);
                Rect durationRect = new Rect(position.x, position.y + _LINE_HEIGHT + _SPACING, position.width, _LINE_HEIGHT);
                Rect maxCountRect = new Rect(position.x, position.y + (_LINE_HEIGHT + _SPACING) * 2, position.width, _LINE_HEIGHT);
                Rect intervalRect = new Rect(position.x, position.y + (_LINE_HEIGHT + _SPACING) * 3, position.width, _LINE_HEIGHT);

                EffectBase effect = property.serializedObject.targetObject as EffectBase;
                Assert.IsNotNull(effect);


                switch (duration)
                {
                    case ActionDuration.Instant:
                    {
                        delayProp.floatValue = 0f;

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.FloatField(delayRect, "Delay", delayProp.floatValue);
                        }

                        durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, effect.durationLimit.min, effect.durationLimit.max);
                        break;
                    }

                    case ActionDuration.Infinite:
                    case ActionDuration.Duration:
                    {
                        applyCountProp.intValue = Mathf.Clamp(applyCountProp.intValue, 1, int.MaxValue);
                        durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, 0f, parentDuration);
                        durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, effect.durationLimit.min, effect.durationLimit.max);
                        delayProp.floatValue = EditorGUI.FloatField(delayRect, "Delay", delayProp.floatValue);
                        break;
                    }
                }
                
                durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                applyCountProp.intValue = Mathf.Max(EditorGUI.IntField(maxCountRect, "Apply Count", applyCountProp.intValue), 1);
                
                using (new EditorGUI.DisabledScope(true))
                {
                    bool isDurationZero = Mathf.Approximately(durationProp.floatValue, 0f);
                    applyIntervalProp.floatValue = isDurationZero ? float.NaN : durationProp.floatValue / applyCountProp.intValue;
                    EditorGUI.FloatField(intervalRect, "Apply Interval", applyIntervalProp.floatValue);
                }

                if (check.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (_LINE_HEIGHT + _SPACING) * 4 - _SPACING; // 2 lines with spacing
        }
    }
}
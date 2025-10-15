using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(ExecutionData))]
    public class ExecutionDataDrawer : PropertyDrawer
    {
        private const float _LINE_HEIGHT = 18f;

        private const float _SPACING = 2f;

        private SerializedObject _actionSerializedObject;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_actionSerializedObject is null)
            {
                SerializedProperty actionProp = property.serializedObject.FindProperty("_referencedAction");
                this._actionSerializedObject = new SerializedObject(actionProp.objectReferenceValue);
            }
            
            Assert.IsNotNull(_actionSerializedObject);
            
            _actionSerializedObject.Update();
            
            SerializedProperty parentDurationDataProp = _actionSerializedObject.FindProperty("durationData");
            SerializedProperty parentDurationProp = parentDurationDataProp.FindPropertyRelative("duration");
            SerializedProperty durationTypeProp = parentDurationDataProp.FindPropertyRelative("durationType");

            SerializedProperty durationProp = property.FindPropertyRelative("duration");
            SerializedProperty applyCountProp = property.FindPropertyRelative("applyCount");
            SerializedProperty applyIntervalProp = property.FindPropertyRelative("applyInterval");
            
            property.serializedObject.Update();
            
            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                float parentDuration = parentDurationProp.floatValue;
                ActionDuration duration = (ActionDuration)durationTypeProp.enumValueIndex;

                Rect durationRect = new Rect(position.x, position.y, position.width, _LINE_HEIGHT);
                Rect maxCountRect = new Rect(position.x, position.y + _LINE_HEIGHT + _SPACING, position.width, _LINE_HEIGHT);
                Rect intervalRect = new Rect(position.x, position.y + (_LINE_HEIGHT + _SPACING) * 2, position.width, _LINE_HEIGHT);

                switch (duration)
                {
                    case ActionDuration.Instant:
                    {
                        applyCountProp.intValue = 1;
                        durationProp.floatValue = 0f;
                        applyIntervalProp.floatValue = float.NaN;

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                            EditorGUI.IntField(maxCountRect, "Apply Count", applyCountProp.intValue);
                            EditorGUI.FloatField(intervalRect, "Apply Interval", applyIntervalProp.floatValue);
                        }

                        break;
                    }

                    case ActionDuration.Infinite:
                    {
                        applyCountProp.intValue = Mathf.Clamp(applyCountProp.intValue, 1, int.MaxValue);
                        durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, 0f, parentDuration);

                        durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                        applyCountProp.intValue = EditorGUI.IntField(maxCountRect, "Apply Count", applyCountProp.intValue);

                        using (new EditorGUI.DisabledScope(true))
                        {
                            bool isDurationZero = Mathf.Approximately(durationProp.floatValue, 0f);
                            applyIntervalProp.floatValue = isDurationZero ? float.NaN : durationProp.floatValue / applyCountProp.intValue;
                            EditorGUI.FloatField(intervalRect, "Apply Interval", applyIntervalProp.floatValue);
                        }

                        break;
                    }

                    case ActionDuration.Duration:
                    {
                        applyCountProp.intValue = Mathf.Clamp(applyCountProp.intValue, 1, int.MaxValue);
                        durationProp.floatValue = Mathf.Clamp(durationProp.floatValue, 0f, parentDuration);

                        durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration", durationProp.floatValue);
                        applyCountProp.intValue = EditorGUI.IntField(maxCountRect, "Apply Count", applyCountProp.intValue);

                        using (new EditorGUI.DisabledScope(true))
                        {
                            bool isDurationZero = Mathf.Approximately(durationProp.floatValue, 0f);
                            applyIntervalProp.floatValue = isDurationZero ? float.NaN : durationProp.floatValue / applyCountProp.intValue;
                            EditorGUI.FloatField(intervalRect, "Apply Interval", applyIntervalProp.floatValue);
                        }

                        break;
                    }
                }

                if (check.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (_LINE_HEIGHT + _SPACING) * 3 - _SPACING; // 2 lines with spacing
        }
    }
}
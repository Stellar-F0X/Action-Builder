using StatController.Runtime;
using UnityEditor;
using UnityEngine;

namespace StatController.Tool
{
    [CustomPropertyDrawer(typeof(MinMaxStat))]
    public class MinMaxStatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty value = prop.FindPropertyRelative("_baseValue");
            SerializedProperty min = prop.FindPropertyRelative("min");
            SerializedProperty max = prop.FindPropertyRelative("max");

            EditorGUI.BeginProperty(pos, label, prop);
            pos.height = EditorGUIUtility.singleLineHeight;
            pos.y += 2;

            float[] values = new float[3]
            {
                value.floatValue,
                min.floatValue,
                max.floatValue
            };

            GUIContent[] contents = new GUIContent[3]
            {
                new GUIContent("Value"),
                new GUIContent("Min"),
                new GUIContent("Max"),
            };

            EditorGUI.MultiFloatField(pos, contents, values);
            
            float inputValue = values[0];
            float inputMin = values[1];
            float inputMax = values[2];
            
            if (inputMin > inputMax)
            {
                float temp = inputMin;
                inputMin = inputMax;
                inputMax = temp;
            }
            
            value.floatValue = Mathf.Clamp(inputValue, inputMin, inputMax);
            min.floatValue = inputMin;
            max.floatValue = inputMax;

            EditorGUI.EndProperty();
        }
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
    }
}
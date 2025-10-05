using StatController.Runtime;
using UnityEditor;
using UnityEngine;

namespace StatController.Tool
{
    [CustomPropertyDrawer(typeof(Stat))]
    public class StatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty value = prop.FindPropertyRelative("_value");

            if (SerializedProperty.DataEquals(value, null))
            {
                return;
            }
            
            EditorGUI.BeginProperty(pos, label, prop);
            
            pos.height = EditorGUIUtility.singleLineHeight;
            pos.y += 2;
            
            GUIContent content = new GUIContent("Value");
            value.floatValue = EditorGUI.FloatField(pos, content, value.floatValue);
            
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
    }
}
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(ShowRichTextAttribute), true)]
    public class ShowRichTextAttributeDrawer : PropertyDrawer
    {
        private const int _HEIGHT = 40;
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;
            style.wordWrap = true;

            EditorGUILayout.TextField(property.stringValue, style, GUILayout.Height(_HEIGHT));
        }
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _HEIGHT + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
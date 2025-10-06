using StatController.Runtime;
using UnityEditor;
using UnityEngine;

namespace StatController.Tool
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (((ReadOnlyAttribute)attribute).isRuntimeOnly)
            {
                GUI.enabled = !Application.isPlaying;
            }
            else
            {
                GUI.enabled = false;
            }
            
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
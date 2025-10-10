using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;

namespace ActionBuilder.Tool
{
    [CustomEditor(typeof(ActionBase), true)]
    public class ActionBaseDrawer : Editor
    {
        private const string _EFFECTS_FIELD_NAME = "_effects";
        
        private const string _SCRIPT_FIELD_NAME = "m_Script";

        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool enterChildren = true;
            
            for (var iterator = serializedObject.GetIterator(); iterator.NextVisible(enterChildren);)
            {
                if (ShouldSkipProperty(iterator.name))
                {
                    enterChildren = false;
                    continue;
                }

                if (IsScriptProperty(iterator.name))
                {
                    DrawScriptProperty(iterator);
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                enterChildren = false;
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        

        private static bool ShouldSkipProperty(string propertyName)
        {
            return propertyName.Equals(_EFFECTS_FIELD_NAME, StringComparison.OrdinalIgnoreCase);
        }
        

        private static bool IsScriptProperty(string propertyName)
        {
            return propertyName.Equals(_SCRIPT_FIELD_NAME, StringComparison.OrdinalIgnoreCase);
        }

        
        private static void DrawScriptProperty(SerializedProperty iterator)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(iterator, false);
            }
        }
    }
}
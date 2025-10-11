using System;
using ActionBuilder.Runtime;
using UnityEditor;

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
                if (this.ShouldSkipProperty(iterator.name))
                {
                    enterChildren = false;
                    continue;
                }

                if (this.IsScriptProperty(iterator.name))
                {
                    this.DrawScriptProperty(iterator);
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                enterChildren = false;
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        

        private bool ShouldSkipProperty(string propertyName)
        {
            return propertyName.Equals(_EFFECTS_FIELD_NAME, StringComparison.OrdinalIgnoreCase);
        }
        

        private bool IsScriptProperty(string propertyName)
        {
            return propertyName.Equals(_SCRIPT_FIELD_NAME, StringComparison.OrdinalIgnoreCase);
        }

        
        private void DrawScriptProperty(SerializedProperty iterator)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(iterator, false);
            }
        }
    }
}
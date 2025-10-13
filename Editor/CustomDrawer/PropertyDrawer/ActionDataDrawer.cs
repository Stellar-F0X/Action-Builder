using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(ActionData))]
    public class ActionDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();


            SerializedProperty iconProp = property.FindPropertyRelative("icon");
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty tagProp = property.FindPropertyRelative("tag");
            SerializedProperty developNameProp = property.FindPropertyRelative("developName");
            SerializedProperty descriptionProp = property.FindPropertyRelative("description");


            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    this.RenameActionAsset(property, nameProp);

                    developNameProp.stringValue = EditorGUILayout.TextField("Development Name", developNameProp.stringValue);
                    tagProp.stringValue = EditorGUILayout.TextField("Tag", tagProp.stringValue);
                }

                this.RenderIconField(iconProp);
            }
            
            this.RenderDescriptionField(property, descriptionProp);
        }



        private void RenderDescriptionField(SerializedProperty property, SerializedProperty descriptionProp)
        {
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();

            GUILayoutOption heightOption = GUILayout.Height(40);
            descriptionProp.stringValue = EditorGUILayout.TextField("Description", descriptionProp.stringValue, heightOption);

            if (check.changed == false)
            {
                return;
            }

            property.serializedObject.ApplyModifiedProperties();
        }



        private void RenderIconField(SerializedProperty iconProp)
        {
            Object sprite = iconProp.objectReferenceValue;
            GUILayoutOption widthOption = GUILayout.Width(60);

            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();
            sprite = EditorGUILayout.ObjectField(GUIContent.none, sprite, typeof(Sprite), false, widthOption);

            if (check.changed)
            {
                iconProp.objectReferenceValue = sprite;
                iconProp.serializedObject.ApplyModifiedProperties();
                ActionBuildEditor.Instance.actionListView.RefreshItems();
            }
        }



        private void RenameActionAsset(SerializedProperty property, SerializedProperty nameProp)
        {
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();

            string newName = EditorGUILayout.DelayedTextField("Name", nameProp.stringValue);

            if (check.changed == false)
            {
                return;
            }

            Object targetObject = property.serializedObject.targetObject;
            string path = AssetDatabase.GetAssetPath(targetObject);

            nameProp.stringValue = newName;
            property.serializedObject.ApplyModifiedProperties();
            string message = AssetDatabase.RenameAsset(path, newName);

            if (string.IsNullOrEmpty(message))
            {
                targetObject.name = newName;
                EditorUtility.SetDirty(targetObject);
            }
            else
            {
                Debug.LogError($"Rename failed: {message}");
            }

            AssetDatabase.SaveAssets();
            ActionBuildEditor.Instance.actionListView.RefreshItems();
        }
    }
}
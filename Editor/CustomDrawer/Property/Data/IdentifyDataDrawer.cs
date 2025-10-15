using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(IdentifyData))]
    public class IdentifyDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            
            SerializedProperty iconProp = property.FindPropertyRelative("icon");
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty tagProp = property.FindPropertyRelative("tag");
            SerializedProperty hashProp = property.FindPropertyRelative("hash");
            SerializedProperty descriptionProp = property.FindPropertyRelative("description");


            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    this.RenameActionAsset(property, nameProp, hashProp);

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.IntField("Hash", hashProp.intValue);
                    }
                    
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
                ActionBuildEditor.Instance.UpdateListView();
            }
        }



        private void RenameActionAsset(SerializedProperty property, SerializedProperty nameProp, SerializedProperty hashProp)
        {
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();

            string previousName = nameProp.stringValue;
            EditorGUILayout.DelayedTextField(nameProp);

            if (check.changed == false)
            {
                return;
            }
            
            Object target = property.serializedObject.targetObject;
            property.serializedObject.ApplyModifiedProperties();
            
            string newName = nameProp.stringValue;
            string path = AssetDatabase.GetAssetPath(target);
            string message = AssetDatabase.RenameAsset(path, newName);

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            
            if (string.IsNullOrEmpty(message))
            {
                target.name = newName;
                nameProp.stringValue = newName;
                hashProp.intValue = Animator.StringToHash(newName);
            }
            else
            {
                target.name = previousName;
                nameProp.stringValue = previousName;
                Debug.LogError($"Rename failed: {message}");
            }
            
            property.serializedObject.ApplyModifiedProperties();
            ActionBuildEditor.Instance.UpdateListView();
        }
    }
}
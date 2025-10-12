using System;
using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace ActionBuilder.Tool
{
    [CustomPropertyDrawer(typeof(StatModifierSelector))]
    public class StatModifierSelectorDrawer : PropertyDrawer
    {
        private readonly float _lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private readonly float _totalHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;


        private Rect _statKeyRect;
        
        private Rect _valueRect;

        private Rect _priorityRect;

        private Rect _modifierRect;

        private SerializedObject _serializedObject;


        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _serializedObject = property.serializedObject;
            SerializedProperty actionDataProp = _serializedObject.FindProperty("_actionData");
            SerializedProperty statSetTemplateProp = actionDataProp.FindPropertyRelative("usingStatsTemplate");


            if (statSetTemplateProp.objectReferenceValue == null)
            {
                label.text = "Missing";
            }


            // Foldout 헤더 그리기
            Rect foldoutRect = new Rect(position.x, position.y, position.width, _lineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);


            if (property.isExpanded == false)
            {
                return; //접힌 상태면 내용을 그리지 않음
            }


            EditorGUI.indentLevel++;


            if (this.InvalidModifierSelector(position, statSetTemplateProp, out string[] keys))
            {
                return;
            }


            SerializedProperty keyProp = property.FindPropertyRelative("key");
            Assert.IsNotNull(keyProp);
            
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            Assert.IsNotNull(valueProp);

            SerializedProperty priorityProp = property.FindPropertyRelative("priority");
            Assert.IsNotNull(priorityProp);

            SerializedProperty modifierTypeProp = property.FindPropertyRelative("modifierType");
            Assert.IsNotNull(modifierTypeProp);


            _statKeyRect = new Rect(position.x, position.y + _lineHeight, position.width, EditorGUIUtility.singleLineHeight);
            _valueRect = new Rect(position.x, position.y + _lineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
            _priorityRect = new Rect(position.x, position.y + _lineHeight * 3, position.width, EditorGUIUtility.singleLineHeight);
            _modifierRect = new Rect(position.x, position.y + _lineHeight * 4, position.width, EditorGUIUtility.singleLineHeight);

            
            
            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                int index = Mathf.Max(0, Array.IndexOf(keys, keyProp.stringValue));
                int selectedIdx = EditorGUI.Popup(_statKeyRect, "Key", index, keys);
                
                Assert.IsTrue(selectedIdx >= 0 && selectedIdx < keys.Length);
                
                keyProp.stringValue = keys[selectedIdx];
                
                
                EditorGUI.PropertyField(_valueRect, valueProp);
                
                EditorGUI.PropertyField(_priorityRect, priorityProp);
                
                EditorGUI.PropertyField(_modifierRect, modifierTypeProp);

                
                if (check.changed == false)
                {
                    return;
                }
                
                _serializedObject.ApplyModifiedProperties();
            }


            EditorGUI.indentLevel--;
        }

        

        private bool InvalidModifierSelector(Rect position, SerializedProperty usingStatsTemplateProp, out string[] keys)
        {
            position.y += _lineHeight;

            
            if (usingStatsTemplateProp.objectReferenceValue is not StatSet statSet)
            {
                Rect helpRect = new Rect(position.x, position.y, position.width, _totalHeight);
                EditorGUI.HelpBox(helpRect, "No keys available", MessageType.Info);
                EditorGUI.indentLevel--;
                keys = null;
                return true;
            }

            
            keys = this.GetKeyOptions(statSet);

            
            if (keys.Length == 0)
            {
                Rect helpRect = new Rect(position.x, position.y, position.width, _totalHeight);
                EditorGUI.HelpBox(helpRect, "No keys available", MessageType.Info);
                EditorGUI.indentLevel--;
                return true;
            }
            

            return false;
        }

        

        private string[] GetKeyOptions(StatSet usingStatsTemplate)
        {
            if (usingStatsTemplate is null)
            {
                return Array.Empty<string>();
            }
            

            List<string> keys = ListPool<string>.Get();

            
            foreach (KeyValuePair<string, Stat> pair in usingStatsTemplate.GetStatPairs())
            {
                keys.Add(pair.Key);
            }
            

            string[] result = keys.ToArray();
            ListPool<string>.Release(keys);
            
            return result;
        }

        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return _lineHeight * 5 + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                return _lineHeight;
            }
        }
    }
}
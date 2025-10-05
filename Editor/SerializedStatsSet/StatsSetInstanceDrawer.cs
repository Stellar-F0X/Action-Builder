using StatController.Runtime;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Tool
{
    [CustomPropertyDrawer(typeof(StatsSetInstance))]
    public class StatsSetInstanceDrawer : PropertyDrawer
    {
        private readonly float _lineHeight = EditorGUIUtility.singleLineHeight;

        private const float _SPACING = 2f;


        private SerializedObject _serializedObject;
        private SerializedProperty _pairListProp;
        private ReorderableList _listProp;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _pairListProp = property.FindPropertyRelative("_pairList");
            _serializedObject = property.serializedObject;

            if (_listProp == null)
            {
                _listProp = new ReorderableList(_serializedObject, _pairListProp, true, true, false, false);
                _listProp.drawHeaderCallback = DrawHeaderCallback;
                _listProp.drawElementCallback = DrawElementCallback;
                _listProp.elementHeightCallback = ElementHeightCallback;
            }

            _serializedObject.Update();
            {
                float totalHeight = this.CalculateTotalListHeight();
                _listProp.DoList(new Rect(position.x, position.y, position.width, totalHeight));
            }

            _serializedObject.ApplyModifiedProperties();
        }


#region List View Method

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_pairListProp.arraySize <= index || index < 0)
            {
                return;
            }

            SerializedProperty property = _pairListProp.GetArrayElementAtIndex(index);
            Assert.IsNotNull(property);

            SerializedProperty keyProp = property.FindPropertyRelative("statKey");
            SerializedProperty valueProp = property.FindPropertyRelative("stat");

            float keyWidth = rect.width * 0.35f;
            float valueWidth = rect.width * 0.65f;
            float currentY = rect.y + 2f;
            float currentX = rect.x + keyWidth + 15;
            float indentOffset = 20;

            GUI.enabled = false;
            Rect keyRect = new Rect(rect.x, currentY, keyWidth, _lineHeight);
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            GUI.enabled = true;

            Rect statFoldoutRect = new Rect(currentX, currentY, valueWidth - 5, _lineHeight);
            valueProp.isExpanded = EditorGUI.Foldout(statFoldoutRect, valueProp.isExpanded, new GUIContent("Stat Data"), true);
            currentY += _lineHeight + _SPACING;

            if (valueProp.isExpanded == false)
            {
                return;
            }

            Rect statValueRect = new Rect(currentX, currentY, valueWidth - indentOffset, _lineHeight);
            EditorGUI.PropertyField(statValueRect, valueProp, GUIContent.none);
            currentY += _lineHeight + _SPACING * 2f;
            
            statValueRect = new Rect(currentX, currentY, valueWidth - indentOffset, _lineHeight);
            SerializedProperty finalValueProp = valueProp.FindPropertyRelative("_finalValue");
            finalValueProp.floatValue = EditorGUI.FloatField(statValueRect, new GUIContent("Final Value"), finalValueProp.floatValue);
            currentY += _lineHeight + _SPACING * 2f;

            GUI.enabled = false;
            SerializedProperty modifiersProp = valueProp.FindPropertyRelative("_modifiers");
            float height = EditorGUI.GetPropertyHeight(modifiersProp, true);
            Rect modifiersRect = new Rect(currentX, currentY, valueWidth - indentOffset, height);
            EditorGUI.PropertyField(modifiersRect, modifiersProp, new GUIContent("Modifiers"), true);
            GUI.enabled = true;
        }


        private float ElementHeightCallback(int index)
        {
            if (_pairListProp == null || _pairListProp.arraySize <= index || index < 0)
            {
                return EditorGUIUtility.singleLineHeight + 4f;
            }

            SerializedProperty property = _pairListProp.GetArrayElementAtIndex(index);
            SerializedProperty valueProp = property.FindPropertyRelative("stat");
            SerializedProperty modifiersProp = valueProp.FindPropertyRelative("_modifiers");

            float height = EditorGUIUtility.singleLineHeight + 4f;

            if (valueProp.isExpanded == false)
            {
                return height;
            }

            height += _lineHeight * 2 + _SPACING * 3; //Value 필드 길이 더함.
            height += EditorGUI.GetPropertyHeight(modifiersProp, true);
            return height;
        }


        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Stat List", EditorStyles.boldLabel);
        }

#endregion


        
#region Calculate Height Methods

        private float CalculateTotalListHeight()
        {
            float height = 0f;

            height += _lineHeight;
            height += _SPACING;

            if (_pairListProp == null)
            {
                return Mathf.Max(height + _lineHeight, _lineHeight * 3f);
            }

            for (int i = 0; i < _pairListProp.arraySize; i++)
            {
                height += this.ElementHeightCallback(i);
                height += _SPACING;
            }

            if (_pairListProp.arraySize > 0)
            {
                height -= _SPACING;
            }

            return height + _lineHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _pairListProp = property.FindPropertyRelative("_pairList");

            if (_pairListProp == null)
            {
                return _lineHeight * 3f;
            }
            else
            {
                return this.CalculateTotalListHeight();
            }
        }

#endregion
    }
}
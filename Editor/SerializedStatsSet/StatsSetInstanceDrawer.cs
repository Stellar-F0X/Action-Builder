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

            // 전체 리스트 높이 계산
            float totalHeight = CalculateTotalListHeight();

            // 요청받은 position.width를 그대로 사용하고, 계산된 높이로 Rect 생성
            Rect listRect = new Rect(position.x, position.y, position.width, totalHeight);

            // DoList(Rect)로 그리기
            _listProp.DoList(listRect);

            _serializedObject.ApplyModifiedProperties();
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Inspector가 이 프로퍼티의 높이를 묻는 경우에도 동일한 계산을 사용
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

            // 마지막 여분 spacing 제거
            if (_pairListProp.arraySize > 0)
            {
                height -= _SPACING;
            }
            
            return height + _lineHeight;
        }


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
            SerializedProperty modifiersProp = valueProp.FindPropertyRelative("_modifiers");

            float keyWidth = rect.width * 0.35f;
            float valueWidth = rect.width * 0.65f;
            float currentY = rect.y + 2f;
            float currentX = rect.x + keyWidth + 15;
            float indentOffset = 20;

            Rect keyRect = new Rect(rect.x, currentY, keyWidth, _lineHeight);
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);

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

            // Modifiers 리스트 (하위 foldout 제거)
            float height = EditorGUI.GetPropertyHeight(modifiersProp, true);
            Rect modifiersRect = new Rect(currentX, currentY, valueWidth - indentOffset, height);
            EditorGUI.PropertyField(modifiersRect, modifiersProp, new GUIContent("Modifiers"), true);
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

            height += _lineHeight; //Value 필드 길이 더함.
            height += EditorGUI.GetPropertyHeight(modifiersProp, true);
            return height;
        }


        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Stat List", EditorStyles.boldLabel);
        }
    }
}
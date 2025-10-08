using System;
using StatController.Runtime;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Tool
{
    [CustomEditor(typeof(StatSet<>), true)]
    public class StatSetDrawer : Editor
    {
        private readonly string _ERROR_MESSAGE = "Key is already included.";
        
        // 클래스 파일의 인스펙터에 등록되어, 주입됨
        public VisualTreeAsset visualTreeAsset;

        private StatSet _statSet;
        
        private SerializedProperty _previewStatKeyProp;
        private SerializedProperty _previewStatProp;
        private SerializedProperty _statListProp;

        private VisualElement _rootVisualElement;
        private IMGUIContainer _imguiContainer;
        private ReorderableList _statListView;
        


        public override VisualElement CreateInspectorGUI()
        {
            _statSet = (StatSet)target;
            Assert.IsNotNull(_statSet);
            
            _previewStatKeyProp = serializedObject.FindProperty("_previewKey");
            _previewStatProp = serializedObject.FindProperty("_previewStat");
            _statListProp = serializedObject.FindProperty("_statPairs");

            _rootVisualElement = this.visualTreeAsset.CloneTree();
            _rootVisualElement.Bind(serializedObject);

            Button addButton = _rootVisualElement.Q<Button>("add-button");
            Button valueBindButton = _rootVisualElement.Q<Button>("value-bind-button");

            addButton.clicked += this.AddButtonOnClicked;
            valueBindButton.clickable.clickedWithEventInfo += this.OnClickedBindValueButton;

            _imguiContainer = _rootVisualElement.Q<IMGUIContainer>("element-container");
            _imguiContainer.onGUIHandler += this.OnGUIHandler;
            return _rootVisualElement;
        }
        
        
        private bool IsKeyDuplicatedExcludingIndex(object keyValue, int excludeIndex)
        {
            Assert.IsNotNull(_statListProp);
            Assert.IsNotNull(keyValue);
            
            for (int i = 0; i < _statListProp.arraySize; i++)
            {
                if (i == excludeIndex)
                {
                    continue;
                }
                
                SerializedProperty otherProperty = _statListProp.GetArrayElementAtIndex(i);
                SerializedProperty otherKeyProp = otherProperty.FindPropertyRelative("statKey");
                
                if (otherKeyProp.boxedValue != null && otherKeyProp.boxedValue.Equals(keyValue))
                {
                    return true;
                }
            }
            
            return false;
        }


#region List Draw Methods

        private void OnGUIHandler()
        {
            _statListView = new ReorderableList(serializedObject, _statListProp, true, true, false, false);
            _statListView.drawHeaderCallback += this.DrawHeaderCallback;
            _statListView.drawElementCallback += this.DrawElementCallback;
            _statListView.elementHeightCallback += this.ElementHeightCallback;

            EditorGUILayout.Space(5);
            _statListView.DoLayoutList();
        }


        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_statListProp.arraySize <= index || _statListProp.arraySize < 0)
            {
                return;
            }
            
            serializedObject.Update();
            
            SerializedProperty property = _statListProp.GetArrayElementAtIndex(index);
            Assert.IsNotNull(property);
            SerializedProperty keyProp = property.FindPropertyRelative("statKey");
            Assert.IsNotNull(keyProp);
            SerializedProperty valueProp = property.FindPropertyRelative("stat");
            Assert.IsNotNull(valueProp);

            const float btnWidth = 20; //button width
            
            float keyWidth = rect.width * 0.35f;
            float valueWidth = rect.width * 0.65f;
            float halfBtnWidth = btnWidth * 0.5f;

            Rect keyRect = new Rect(rect.x, rect.y + 2, keyWidth - halfBtnWidth - 3, rect.height);
            Rect valueRect = new Rect(rect.x + keyRect.width + 3, rect.y, valueWidth - halfBtnWidth, rect.height);
            Rect buttonRect = new Rect(valueRect.x + valueRect.width + 3, rect.y, btnWidth, rect.height);

            // 키 값 변경 전 이전 값을 저장
            object previousKeyValue = keyProp.boxedValue;
            
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
            EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Stat Data"), true);

            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("CrossIcon")))
            {
                _statListProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                return; //삭제됐다면 여기서 빠른 종료.
            }

            //변경사항이 없다면 빠른 종료.
            if (check.changed == false)
            {
                return;
            }

            //변경사항이 있다면 중복 키를 검사한다.
            object newKeyValue = keyProp.boxedValue;
                
            if (this.IsKeyDuplicatedExcludingIndex(newKeyValue, index))
            {
                keyProp.boxedValue = previousKeyValue;
                _rootVisualElement.Add(new HelpBox(_ERROR_MESSAGE, HelpBoxMessageType.Error));
            }
            else
            {
                _rootVisualElement.Query<HelpBox>().ForEach(hb => hb.RemoveFromHierarchy());
            }
                
            serializedObject.ApplyModifiedProperties();
        }


        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Stat List", EditorStyles.boldLabel);
        }
        
        
        private float ElementHeightCallback(int index)
        {
            SerializedProperty property = _statListProp.GetArrayElementAtIndex(index);
            Assert.IsNotNull(property);
            SerializedProperty keyProp = property.FindPropertyRelative("statKey");
            Assert.IsNotNull(keyProp);
            SerializedProperty valueProp = property.FindPropertyRelative("stat");
            Assert.IsNotNull(valueProp);

            // includeChildren를 true로 설정하여 자식 프로퍼티들의 높이도 계산에 포함
            float height1 = EditorGUI.GetPropertyHeight(keyProp, true);
            float height2 = EditorGUI.GetPropertyHeight(valueProp, true);
    
            return Mathf.Max(height1, height2);
        }

#endregion


#region Button Events

        private void AddButtonOnClicked()
        {
            Type keyType = target.GetType().BaseType?.GenericTypeArguments[0];
            Stat stat = _previewStatProp.managedReferenceValue as Stat;
            object value = _previewStatKeyProp.boxedValue;

            Assert.IsNotNull(keyType);
            Assert.IsNotNull(stat);

            int arraySize = _statListProp.arraySize;
            int insertIdx = Mathf.Max(arraySize - 1, 0);

            _statListProp.InsertArrayElementAtIndex(insertIdx);
            SerializedProperty prop = _statListProp.GetArrayElementAtIndex(arraySize);
            Assert.IsNotNull(prop);

            object clonedKey = Utility.CreateStatKeyInstanceByType(keyType, value);
            bool contained = _statSet.ContainsKey(clonedKey);

            if (contained)
            {
                _rootVisualElement.Add(new HelpBox(_ERROR_MESSAGE, HelpBoxMessageType.Error));
                return;
            }
            else
            {
                _rootVisualElement.Query<HelpBox>().ForEach(hb => hb.RemoveFromHierarchy());
            }
            
            Assert.IsNotNull(clonedKey);
            Stat clonedStat = stat.Clone() as Stat;
            Assert.IsNotNull(clonedStat);

            SerializedProperty keyProp = prop.FindPropertyRelative("statKey");
            Assert.IsNotNull(keyProp);
            SerializedProperty valueProp = prop.FindPropertyRelative("stat");
            Assert.IsNotNull(valueProp);

            keyProp.boxedValue = clonedKey;
            valueProp.managedReferenceValue = clonedStat;
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }


        private void OnClickedBindValueButton(EventBase clickEvent)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Stat")
                                                       .AddFactoryModule(
                                                           () => new StatFactoryModule("Stats"),
                                                           () => new TypeTreeProvider(true, true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<Stat>)(s => this.ApplyPreview(_previewStatProp, s)));

            window.OpenWindow(clickEvent.originalMousePosition);
        }


        private void ApplyPreview(SerializedProperty property, Stat data)
        {
            Assert.IsNotNull(property);
            Assert.IsNotNull(serializedObject);

            property.managedReferenceValue = data;

            serializedObject.ApplyModifiedProperties();
            _rootVisualElement.Bind(serializedObject);
            EditorUtility.SetDirty(target);
        }

#endregion
    }
}
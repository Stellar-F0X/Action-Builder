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
    [CustomEditor(typeof(StatsSet))]
    public class StatDictionaryDrawer : Editor
    {
        // 클래스 파일의 인스펙터에 등록되어, 주입됨
        public VisualTreeAsset visualTreeAsset;

        private SerializedProperty _previewStatKeyProp;
        private SerializedProperty _previewStatProp;
        private SerializedProperty _statListProp;

        private VisualElement _rootVisualElement;
        private IMGUIContainer _imguiContainer;
        private ReorderableList _statListView;


        public override VisualElement CreateInspectorGUI()
        {
            _previewStatKeyProp = serializedObject.FindProperty("_previewKey");
            _previewStatProp = serializedObject.FindProperty("_previewStat");
            _statListProp = serializedObject.FindProperty("_statPairs");

            _rootVisualElement = this.visualTreeAsset.CloneTree();
            _rootVisualElement.Bind(serializedObject);

            Button addButton = _rootVisualElement.Q<Button>("add-button");
            Button keyBindButton = _rootVisualElement.Q<Button>("key-bind-button");
            Button valueBindButton = _rootVisualElement.Q<Button>("value-bind-button");

            addButton.clicked += this.AddButtonOnClicked;
            keyBindButton.clickable.clickedWithEventInfo += this.OnClickedBindKeyButton;
            valueBindButton.clickable.clickedWithEventInfo += this.OnClickedBindValueButton;

            _imguiContainer = _rootVisualElement.Q<IMGUIContainer>("element-container");
            _imguiContainer.onGUIHandler += this.OnGUIHandler;
            return _rootVisualElement;
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
            
            SerializedProperty property = _statListProp.GetArrayElementAtIndex(index);
            Assert.IsNotNull(property);
            SerializedProperty keyProp = property.FindPropertyRelative("statKey");
            Assert.IsNotNull(keyProp);
            SerializedProperty valueProp = property.FindPropertyRelative("stat");
            Assert.IsNotNull(valueProp);

            const float btnWidth = 15; //button width
            
            float width = rect.width * 0.5f - btnWidth;

            Rect keyRect = new Rect(rect.x, rect.y, width, rect.height);
            Rect valueRect = new Rect(rect.x + width, rect.y, width, rect.height);
            Rect buttonRect = new Rect(rect.x + width * 2, rect.y, btnWidth * 2, rect.height);

            EditorGUI.PropertyField(keyRect, keyProp, true);
            EditorGUI.PropertyField(valueRect, valueProp, true);

            if (GUI.Button(buttonRect, "X"))
            {
                _statListProp.DeleteArrayElementAtIndex(index);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
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
            IStatKey key = _previewStatKeyProp.managedReferenceValue as IStatKey;
            Stat stat = _previewStatProp.managedReferenceValue as Stat;

            Assert.IsNotNull(key);
            Assert.IsNotNull(stat);

            int arraySize = _statListProp.arraySize;
            int insertIdx = Mathf.Max(arraySize - 1, 0);

            _statListProp.InsertArrayElementAtIndex(insertIdx);
            SerializedProperty prop = _statListProp.GetArrayElementAtIndex(arraySize);
            Assert.IsNotNull(prop);

            IStatKey clonedKey = key;
            Assert.IsNotNull(clonedKey);
            Stat clonedStat = stat.Clone() as Stat;
            Assert.IsNotNull(clonedStat);

            SerializedProperty keyProp = prop.FindPropertyRelative("statKey");
            Assert.IsNotNull(keyProp);
            SerializedProperty valueProp = prop.FindPropertyRelative("stat");
            Assert.IsNotNull(valueProp);

            keyProp.managedReferenceValue = clonedKey;
            valueProp.managedReferenceValue = clonedStat;
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }


        private void OnClickedBindKeyButton(EventBase clickEvent)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Stat Key")
                                                       .AddFactoryModule(
                                                           () => new StatKeyFactoryModule("Stat Keys"),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<IStatKey>)(s => this.ApplyPreview(_previewStatKeyProp, s)));

            window.OpenWindow(clickEvent.originalMousePosition);
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


        private void ApplyPreview<T>(SerializedProperty property, T data)
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
using System;
using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ActionBuilder.Tool
{
    public class ActionBuildEditor : EditorWindow
    {
        public static ActionBuildEditor Instance
        {
            get;
            private set;
        }

        private readonly List<ActionBase> _actionList = new List<ActionBase>();

        [SerializeField]
        private VisualTreeAsset _visualTreeAsset;

        private Button _createActionButton;
        private Button _deleteActionButton;
        private Button _addEffectButton;

        private ListView _actionListView;
        private ListView _effectListView;
        private VisualElement _dataView;
        private IMGUIContainer _actionView;

        private Editor _inspectorEditor;
        private SerializedObject _serializedObject;
        
        
        public ListView actionListView
        {
            get { return _actionListView; }
        }
        
        public ListView effectListView
        {
            get { return _effectListView; }
        }



        [MenuItem("Tools/ActionBuilder")]
        public static void OpenWindow()
        {
            ActionBuildEditor wnd = GetWindow<ActionBuildEditor>();
            wnd.titleContent = new GUIContent("Action Builder");
        }



        private void CreateGUI()
        {
            Instance = this;

            _visualTreeAsset.CloneTree(rootVisualElement);
            Assert.IsNotNull(_visualTreeAsset);

            _createActionButton = rootVisualElement.Q<Button>("create-action-button");
            _deleteActionButton = rootVisualElement.Q<Button>("delete-action-button");
            _addEffectButton = rootVisualElement.Q<Button>("add-effect-button");

            _dataView = rootVisualElement.Q<VisualElement>("data-view");
            _actionListView = rootVisualElement.Q<ListView>("action-list");
            _actionView = rootVisualElement.Q<IMGUIContainer>("action-view");
            _effectListView = rootVisualElement.Q<ListView>("effect-list");

            _createActionButton.clickable.clickedWithEventInfo += this.CreateActionOnClickedButton;
            _deleteActionButton.clicked += this.DeleteActionOnClickedButton;
            _addEffectButton.clickable.clickedWithEventInfo += this.AddEffectOnClickedButton;

            _actionListView.bindItem = this.BindActionToList;
            _actionListView.makeItem = this.BindActionListItem;
            _actionListView.selectionChanged += this.OnActionSelectionChanged;

            _effectListView.bindItem = this.BindEffectListItem;

            _actionList.AddRange(Resources.LoadAll<ActionBase>("Actions"));
            _actionListView.itemsSource = _actionList;
            _dataView.style.display = DisplayStyle.None;
        }



        private void SelectAction(ActionBase selectedAction)
        {
            if (_inspectorEditor == null || _inspectorEditor.target != selectedAction)
            {
                _inspectorEditor = Editor.CreateEditor(selectedAction);
                _serializedObject = new SerializedObject(selectedAction);
            }

            _actionView.onGUIHandler = this.RenderActionEditorUI;

            if (selectedAction.internalEffects is null)
            {
                _effectListView.itemsSource = null;
                _dataView.style.display = DisplayStyle.None;
                return;
            }

            _dataView.style.display = DisplayStyle.Flex;
            _effectListView.itemsSource = selectedAction.internalEffects;

            if (selectedAction.internalEffects.Count == 0)
            {
                _effectListView.style.display = DisplayStyle.None;
            }
            else
            {
                _effectListView.style.display = DisplayStyle.Flex;
                _effectListView.schedule.Execute(_effectListView.Rebuild).ExecuteLater(0);
            }
        }


#region Action List

        private void OnActionSelectionChanged(IEnumerable<object> selected)
        {
            Assert.IsNotNull(_actionListView);
            
            int idx = _actionListView.selectedIndex;
            
            if (idx < 0 || idx >= _actionList.Count)
            {
                return;
            }

            if (_actionList[idx] == null)
            {
                _dataView.style.display = DisplayStyle.None;
            }
            else
            {
                this.SelectAction(_actionList[idx]);
            }
        }


        private VisualElement BindActionListItem()
        {
            VisualElement rootElement = new VisualElement();

            rootElement.style.flexDirection = FlexDirection.Row;
            rootElement.style.alignItems = Align.Center;

            rootElement.Add(new Image());
            rootElement.Add(new Label());
            return rootElement;
        }


        private void BindActionToList(VisualElement visualElement, int index)
        {
            Image icon = visualElement.Q<Image>();
            Label nameLabel = visualElement.Q<Label>();

            ActionBase action = _actionList[index];
            nameLabel.text = action.actionName;
            icon.sprite = action.icon;

            visualElement.tooltip = action.description;
        }


        private void AddActionToList(ActionBase action)
        {
            Assert.IsNotNull(action);
            Assert.IsNotNull(_actionListView);

            _actionListView.itemsSource.Add(action);
            _actionListView.RefreshItems();

            int index = _actionListView.itemsSource.Count - 1;
            _actionListView.selectedIndex = index;

            this.SelectAction(action);
        }

#endregion



#region Effect List

        private void BindEffectListItem(VisualElement visualElement, int index)
        {
            if (_serializedObject is null)
            {
                _serializedObject = new SerializedObject(_actionList[index]);
            }

            _serializedObject.Update();

            EffectView view = visualElement.Q<EffectView>();
            SerializedProperty dataProp = _serializedObject.FindProperty("_actionData");
            SerializedProperty effectListProp = dataProp.FindPropertyRelative("effects");
            SerializedProperty effectProp = effectListProp.GetArrayElementAtIndex(index);

            view.onDeleteRequested -= this.DeleteEffectOnClickedButton;
            view.onDeleteRequested += this.DeleteEffectOnClickedButton;

            view.Refresh(effectProp, (EffectBase)effectProp.boxedValue);
        }


        private void AddEffectToList(EffectBase effect)
        {
            Assert.IsNotNull(effect);
            Assert.IsNotNull(_effectListView);


            ActionBase action = _serializedObject?.targetObject as ActionBase;
            
            if (_serializedObject is null)
            {
                action = _actionList[_actionListView.selectedIndex];
                _serializedObject = new SerializedObject(action);
            }
            
            Assert.IsNotNull(action, "action is NullReference");
            effect.referencedAction = action;

            _effectListView.style.display = DisplayStyle.Flex;
            _effectListView.itemsSource.Add(effect);
            _effectListView.RefreshItems();
        }

#endregion



#region Button Events

        private void CreateActionOnClickedButton(EventBase evt)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Action")
                                                       .AddFactoryModule(
                                                           () => new ActionFactoryModule("Actions"),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<ActionBase>)AddActionToList);
            window.OpenWindow(evt.originalMousePosition);
        }


        private void DeleteEffectOnClickedButton(EffectView effectView)
        {
            int selectedIndex = _actionListView.selectedIndex;

            if (_actionList == null || _actionList.Count == 0 || selectedIndex < 0 || selectedIndex >= _actionList.Count)
            {
                return;
            }

            ActionBase action = _actionList[selectedIndex];
            Assert.IsNotNull(action);

            if (action.internalEffects == null || action.internalEffects.Count == 0)
            {
                return;
            }

            int effectIndex = action.internalEffects.IndexOf(effectView.effect);

            if (effectIndex < 0)
            {
                return;
            }

            action.internalEffects.RemoveAt(effectIndex);

            if (action.internalEffects.Count == 0)
            {
                _effectListView.style.display = DisplayStyle.None;
            }
            else
            {
                _effectListView.style.display = DisplayStyle.Flex;
            }

            _effectListView.RefreshItems();
            this.Repaint();
        }


        private void DeleteActionOnClickedButton()
        {
            int selectedIndex = _actionListView.selectedIndex;

            if (_actionList == null || _actionList.Count == 0 || selectedIndex < 0 || selectedIndex >= _actionList.Count)
            {
                return;
            }

            ActionBase actionToDelete = _actionList[selectedIndex];
            Assert.IsNotNull(actionToDelete);

            // IMGUIContainer 해제
            if (_actionView != null)
            {
                _inspectorEditor = null;
                _actionView.onGUIHandler = null;
            }

            // SerializedObject dispose
            if (_serializedObject != null)
            {
                _serializedObject.Dispose();
                _serializedObject = null;
            }

            string assetPath = AssetDatabase.GetAssetPath(actionToDelete);

            if (string.IsNullOrEmpty(assetPath) == false)
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
            }

            _dataView.style.display = DisplayStyle.None;
            _actionList.RemoveAt(selectedIndex);
            _actionListView.RefreshItems();
            _actionListView.ClearSelection();
            this.Repaint();
        }


        private void AddEffectOnClickedButton(EventBase evt)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Effects")
                                                       .AddFactoryModule(
                                                           () => new EffectFactoryModule("Effects"),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<EffectBase>)AddEffectToList);
            window.OpenWindow(evt.originalMousePosition);
        }

#endregion

        

#region Render Action Inspector

        private void RenderActionEditorUI()
        {
            if (_inspectorEditor == null)
            {
                Object target = _serializedObject.targetObject;
                Assert.IsNotNull(target, "targetObject is null");
                _inspectorEditor = Editor.CreateEditor(target);
            }

            Assert.IsNotNull(_inspectorEditor);
            _inspectorEditor.OnInspectorGUI();
        }

#endregion
    }
}
using System;
using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public class ActionListController : ListController
    {
        public ActionListController(ListView listView, ListView effectsView, Button handleAddButton, InspectorController inspector, List<ScriptableObject> objectList) : base(listView, inspector, objectList)
        {
            _effectViewList = effectsView;
            _handleAddButton = handleAddButton;

            handleAddButton.clickable.clickedWithEventInfo -= this.CreateEffectOnClickedButton;
            handleAddButton.clickable.clickedWithEventInfo += this.CreateEffectOnClickedButton;

            effectsView.bindItem -= this.BindEffectVisualElement;
            effectsView.bindItem += this.BindEffectVisualElement;
        }


        private readonly Button _handleAddButton;
        private readonly ListView _effectViewList;


        public override void OnSelected()
        {
            ActionBase action = (ActionBase)_inspectorController.selectedObject.targetObject; 
            
            if (action.internalEffectSO is null)
            {
                _effectViewList.itemsSource = null;
                _inspectorController.style.display = DisplayStyle.None;
                return;
            }

            _handleAddButton.style.display = DisplayStyle.Flex;
            _inspectorController.style.display = DisplayStyle.Flex;
            _effectViewList.itemsSource = action.internalEffectSO;

            if (action.internalEffectSO.Count == 0)
            {
                _effectViewList.style.display = DisplayStyle.None;
            }
            else
            {
                _effectViewList.style.display = DisplayStyle.Flex;
                _effectViewList.schedule.Execute(_effectViewList.Rebuild).ExecuteLater(0);
            }
        }
        

        public override void DeleteOnClickedButton()
        {
            int selectedIndex = _listView.selectedIndex;

            if (_objectList == null || _objectList.Count == 0)
            {
                return;
            }
            
            if (selectedIndex < 0 || selectedIndex >= _objectList.Count)
            {
                return;
            }

            ActionBase actionToDelete = _objectList[selectedIndex] as ActionBase;
            Assert.IsNotNull(actionToDelete);

            
            _inspectorController.ClearInspector();
            _inspectorController.DisposeObject();


            string assetPath = AssetDatabase.GetAssetPath(actionToDelete);

            if (string.IsNullOrEmpty(assetPath) == false)
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
            }

            _inspectorController.style.display = DisplayStyle.None;
            _objectList.RemoveAt(selectedIndex);
            _listView.RefreshItems();
            _listView.ClearSelection();
        }


        public override void CreateOnClickedButton(EventBase evt)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Action")
                                                       .AddFactoryModule(
                                                               () => new ActionFactoryModule("Actions"), 
                                                               () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<ActionBase>)this.AddToList);
            window.OpenWindow(evt.originalMousePosition);
        }


        protected override void BindToList(VisualElement visualElement, int index)
        {
            Image icon = visualElement.Q<Image>();
            Label nameLabel = visualElement.Q<Label>();

            ActionBase action = _objectList[index] as ActionBase;
            nameLabel.text = action.name;
            icon.sprite = action.icon;

            visualElement.tooltip = action.description;
        }
        
        
        private void CreateEffectOnClickedButton(EventBase evt)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Effects")
                                                       .AddFactoryModule(
                                                               () => new EffectFactoryModule("Effects"), 
                                                               () => new TypeTreeProvider(true))
                                                       .Build();

            SerializedObject serializedObject = this._inspectorController.selectedObject;
            ActionBase action = serializedObject.targetObject as ActionBase;
            Assert.IsNotNull(action);
            
            window.RegisterCreationCallbackOnce((Action<EffectBase>)this.AddEffectToList);
            window.OpenWindow(evt.originalMousePosition);
        }
        
        
        private void AddEffectToList(EffectBase effect)
        {
            Assert.IsNotNull(effect);
            Assert.IsNotNull(_listView);

            ActionBase action = _serializedObject?.targetObject as ActionBase;
            
            if (action == null)
            {
                action = _objectList[_listView.selectedIndex] as ActionBase;
                _serializedObject = new SerializedObject(action);
            }
            
            _effectViewList.style.display = DisplayStyle.Flex;
            
            Assert.IsNotNull(action, "action is NullReference");
            AssetDatabase.AddObjectToAsset(effect, action);
            
            effect.action = action;
            _effectViewList.itemsSource.Add(effect);
            
            _effectViewList.RefreshItems();
            
            EditorUtility.SetDirty(action);
            AssetDatabase.SaveAssets();
        }
        
        
        
        private void BindEffectVisualElement(VisualElement visualElement, int index)
        {
            SerializedObject serializedObject = _inspectorController.selectedObject;
            
            serializedObject.Update();

            EffectView view = visualElement.Q<EffectView>();
            SerializedProperty effectListProp = serializedObject.FindProperty("_effectTemplates");
            SerializedProperty effectProp = effectListProp.GetArrayElementAtIndex(index);

            view.onDeleteRequested -= this.DeleteEffectOnClickedButton;
            view.onDeleteRequested += this.DeleteEffectOnClickedButton;

            view.Refresh(effectProp, (EffectBase)effectProp.objectReferenceValue);
        }


        private void DeleteEffectOnClickedButton(EffectView view)
        {
            int selectedIndex = _listView.selectedIndex;

            if (_objectList == null || _objectList.Count == 0 || selectedIndex < 0 || selectedIndex >= _objectList.Count)
            {
                return;
            }

            ActionBase action = _objectList[selectedIndex] as ActionBase;
            Assert.IsNotNull(action);

            if (action.internalEffectSO == null || action.internalEffectSO.Count == 0)
            {
                return;
            }

            int effectIndex = action.internalEffectSO.IndexOf(view.effect);

            if (effectIndex < 0)
            {
                return;
            }

            AssetDatabase.RemoveObjectFromAsset(view.effect);
            action.internalEffectSO.RemoveAt(effectIndex);

            if (action.internalEffectSO.Count == 0)
            {
                _effectViewList.style.display = DisplayStyle.None;
            }
            else
            {
                _effectViewList.style.display = DisplayStyle.Flex;
            }

            _effectViewList.RefreshItems();
            ActionBuildEditor.Instance.Repaint();
            
            EditorUtility.SetDirty(action);
            AssetDatabase.SaveAssets();
        }
    }
}
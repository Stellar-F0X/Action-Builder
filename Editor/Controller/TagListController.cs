using System;
using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public class TagListController : ListController
    {
        public TagListController(ListView listView,ListView effectListView, Button handleAddButton, InspectorController inspector, List<ScriptableObject> objectList) : base(listView, inspector, objectList)
        {
            this._effectListView = effectListView;
            this._handleAddButton = handleAddButton;
        }
        
        
        private readonly Button _handleAddButton;
        
        private readonly ListView _effectListView;



        public override void OnSelected()
        {
            _effectListView.style.display = DisplayStyle.None;
            _handleAddButton.style.display = DisplayStyle.None;
            _inspectorController.style.display = DisplayStyle.Flex;
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

            TagBase tagToDelete = _objectList[selectedIndex] as TagBase;
            Assert.IsNotNull(tagToDelete);

            _inspectorController.ClearInspector();
            _inspectorController.DisposeObject();

            string assetPath = AssetDatabase.GetAssetPath(tagToDelete);

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

            BindingWindow window = BindingWindowBuilder.GetBuilder("Tag")
                                                       .AddFactoryModule(
                                                               () => new TagFactoryModule("Tag"),
                                                               () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<TagBase>)this.AddToList);
            window.OpenWindow(evt.originalMousePosition);
        }


        protected override void BindToList(VisualElement visualElement, int index)
        {
            Label nameLabel = visualElement.Q<Label>();

            TagBase tag = (TagBase)_objectList[index];
            nameLabel.text = tag.name;

            visualElement.tooltip = tag.description;
        }
    }
}
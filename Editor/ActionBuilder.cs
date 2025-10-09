using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public class ActionBuilderEditor : EditorWindow
    {
        public static ActionBuilderEditor Instance
        {
            get;
            private set;
        }

        private readonly List<ActionBase> _actions = new List<ActionBase>();

        [SerializeField]
        private VisualTreeAsset _visualTreeAsset;

        private Button _createActionButton;
        private Button _deleteActionButton;
        private Button _addEffectButton;

        private ListView _actionList;
        private VisualElement _dataView;
        private IMGUIContainer _actionView;


        [MenuItem("Tools/ActionBuilder")]
        public static void OpenWindow()
        {
            ActionBuilderEditor wnd = GetWindow<ActionBuilderEditor>();
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
            _actionList = rootVisualElement.Q<ListView>("action-list");
            _actionView = rootVisualElement.Q<IMGUIContainer>("action-view");

            _createActionButton.clickable.clickedWithEventInfo += this.OnCreateActionButtonClicked;
            _deleteActionButton.clicked += this.OnDeleteActionButtonClicked;
            _addEffectButton.clickable.clickedWithEventInfo += this.OnAddEffectButtonClicked;


            _actionList.makeItem = () => new Label();
            _actionList.bindItem = this.ActionListBindItem;
            _actionList.selectionChanged += this.DisplaySelectedActions;

            _actions.AddRange(Resources.LoadAll<ActionBase>("Actions"));
            _actionList.itemsSource = _actions;
            Assert.IsNotNull(_actions);
        }



#region Action List

        private void DisplaySelectedActions(IEnumerable<object> selected)
        {
            Assert.IsNotNull(_actionList);

            int idx = _actionList.selectedIndex;

            Assert.IsTrue(idx >= 0 && idx < _actions.Count);

            this.ShowAction(_actions[idx]);
        }


        private void ActionListBindItem(VisualElement visualElement, int index) { }


        // 선택된 Action을 보여주기 위한 간단한 처리(필요에 맞게 확장하세요)
        private void ShowAction(ActionBase action)
        {
            if (action == null)
            {
                _dataView.style.display = DisplayStyle.None;
                return;
            }

            _dataView.style.display = DisplayStyle.Flex;
            _actionView.onGUIHandler = this.RenderActionEditorUI;
        }

#endregion


#region Button Events

        private void OnCreateActionButtonClicked(EventBase evt) { }


        private void OnDeleteActionButtonClicked() { }


        private void OnAddEffectButtonClicked(EventBase evt) { }

#endregion


#region Render Action Inspector

        private void RenderActionEditorUI() { }

#endregion
    }
}
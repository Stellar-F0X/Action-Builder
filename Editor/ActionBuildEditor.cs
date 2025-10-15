using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public class ActionBuildEditor : EditorWindow
    {
        public static ActionBuildEditor Instance
        {
            get;
            private set;
        }

        [SerializeField]
        private VisualTreeAsset _visualTreeAsset;

        private Button _createActionButton;
        private Button _deleteActionButton;

        private TabView _tabView;

        private ActionListController _actionListController;
        private TagListController _tagListController;
        private InspectorController _inspectorController;


        public ListController currentActiveTab => _tabView.activeTab.tabIndex switch
        {
                1 => _actionListController,
                2 => _tagListController,
                _ => throw new IndexOutOfRangeException()
        };



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
            Assert.IsNotNull(_visualTreeAsset, "Visual tree asset is null reference");

            _tabView = rootVisualElement.Q<TabView>();
            _tabView.activeTabChanged += this.OnTabChanged;

            _createActionButton = rootVisualElement.Q<Button>("create-button");
            _deleteActionButton = rootVisualElement.Q<Button>("delete-button");

            _createActionButton.clickable.clickedWithEventInfo += this.CreateActionOnClickedButton;
            _deleteActionButton.clicked += this.DeleteActionOnClickedButton;


            VisualElement inspectorView = rootVisualElement.Q<VisualElement>("inspector-view");
            IMGUIContainer imguiContainer = rootVisualElement.Q<IMGUIContainer>("object-inspector");

            _inspectorController = new InspectorController(inspectorView, imguiContainer);


            ListView handleListView = rootVisualElement.Q<ListView>("effect-list");
            ListView actionListView = rootVisualElement.Q<ListView>("action-list");
            Button handleAddButton = rootVisualElement.Q<Button>("add-effect-button");
            List<ScriptableObject> actionList = this.FilterMainAssetsOnly(Resources.LoadAll<ScriptableObject>("Actions"));
            _actionListController = new ActionListController(actionListView, handleListView, handleAddButton, _inspectorController, actionList);

            ListView tagListView = rootVisualElement.Q<ListView>("tag-list");
            List<ScriptableObject> tagList = this.FilterMainAssetsOnly(Resources.LoadAll<ScriptableObject>("Tags"));
            _tagListController = new TagListController(tagListView, handleListView, handleAddButton, _inspectorController, tagList);
        }

        
        private List<ScriptableObject> FilterMainAssetsOnly(ScriptableObject[] assets)
        {
            return assets.Where(AssetDatabase.IsMainAsset).ToList();
        }

        
        private void OnTabChanged(Tab prev, Tab curr)
        {
            currentActiveTab.listView.ClearSelection();
            _inspectorController.ClearInspector();
        }


        public void UpdateListView()
        {
            currentActiveTab.listView.RefreshItems();
        }


        private void CreateActionOnClickedButton(EventBase evt)
        {
            currentActiveTab.CreateOnClickedButton(evt);
        }


        private void DeleteActionOnClickedButton()
        {
            currentActiveTab.DeleteOnClickedButton();
            this.Repaint();
        }
    }
}
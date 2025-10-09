using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ActionBuilder.Tool
{
    /// <summary> BindingWindow 클래스는 그래프 노드 생성과 관련된 창을 제공합니다. </summary>
    internal sealed class BindingWindow : ScriptableObject, ISearchWindowProvider
    {
        /// <summary> 창의 제목을 나타냅니다. </summary>
        public string windowTitle;

        
        /// <summary> 창의 상단 항목을 표시할지 여부를 나타냅니다. </summary>
        public bool useHeadEntry;

        
        /// <summary> Represents the main window for binding modules. </summary>
        public List<FactoryModule> modules;

        
        /// <summary> Delegate used as a creation callback. </summary>
        private Delegate _creationCallback = new Action(delegate { });



        /// <summary> 지정된 위치에서 검색창을 엽니다. </summary>
        /// <param name="mousePosition">마우스의 현재 위치입니다.</param>
        /// <param name="width">창의 너비입니다. 기본값은 200입니다.</param>
        /// <param name="height">창의 높이입니다. 기본값은 240입니다.</param>
        public void OpenWindow(Vector2 mousePosition, float width = 200, float height = 240)
        {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            
            SearchWindowContext context = new SearchWindowContext(screenPoint, width, height);
            
            SearchWindow.Open(context, this);
        }


        
        /// <summary> Delegate를 설정하고 기존의 콜백을 초기화합니다. </summary>
        /// <param name="callback"> 새로 등록할 Delegate 객체 </param>
        public void RegisterCreationCallbackOnce(Delegate callback)
        {
            this._creationCallback = null;
            this._creationCallback = callback;
        }


        
        /// <summary> Unregisters the current creation callback. </summary>
        public void UnregisterCreationCallbackOnce()
        {
            this._creationCallback = null;
        }

        
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeList = new List<SearchTreeEntry>();

            if (useHeadEntry)
            {
                searchTreeList.Add(new SearchTreeGroupEntry(new GUIContent(windowTitle), 0));
            }

            if (modules is null || modules.Count <= 0)
            {
                return searchTreeList;
            }

            foreach (FactoryModule module in modules)
            {
                searchTreeList.AddRange(module.categoryProvider.ProvideCategories(module));
            }

            return searchTreeList;
        }

        
        
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is (Type createType, FactoryModule factoryModule))
            {
                Vector2 position = this.CalculateMousePosition(context);
                factoryModule.onTryCreate?.Invoke(createType, position, entry.name, _creationCallback);
                _creationCallback = null;
                return true;
            }

            Debug.LogError($"{nameof(BindingWindow)} Error : Entry is empty");
            return false;
        }

        

        /// <summary> 마우스 위치를 계산하여 반환합니다. </summary>
        /// <param name="context"> 검색 창 컨텍스트 객체입니다. </param>
        /// <returns> 계산된 마우스 좌표를 반환합니다. </returns>
        private Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            //TODO: 문제가 있으면 수정.
            return context.screenMousePosition;
        }
    }
}
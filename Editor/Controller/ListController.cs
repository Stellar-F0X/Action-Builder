using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public abstract class ListController
    {
        public ListController(ListView listView, InspectorController inspector, List<ScriptableObject> objectList)
        {
            this._listView = listView;
            this._objectList = objectList;
            this._inspectorController = inspector;

            this._listView.bindItem = this.BindToList;
            this._listView.makeItem = this.MakeVisualElement;
            this._listView.selectionChanged += this.OnSelectionChanged;
            this._listView.itemsSource = objectList;
        }


        protected List<ScriptableObject> _objectList;

        protected ListView _listView;

        protected InspectorController _inspectorController;

        protected SerializedObject _serializedObject;



        public ListView listView
        {
            get { return _listView; }
        }



        public abstract void OnSelected();
        
        

        public virtual VisualElement MakeVisualElement()
        {
            VisualElement rootElement = new VisualElement();
            rootElement.style.flexDirection = FlexDirection.Row;
            rootElement.style.alignItems = Align.Center;
            rootElement.Add(new Image());
            rootElement.Add(new Label());
            return rootElement;
        }


        protected void OnSelectionChanged(IEnumerable<object> selected)
        {
            Assert.IsNotNull(_listView);

            int idx = _listView.selectedIndex;

            if (idx < 0 || idx >= _objectList.Count)
            {
                return;
            }

            if (_objectList[idx] == null)
            {
                _inspectorController.style.display = DisplayStyle.None;
            }
            else
            {
                _inspectorController.Select(_objectList[idx]);
            }
        }


        protected void AddToList(ScriptableObject effect)
        {
            Assert.IsNotNull(effect);
            Assert.IsNotNull(_listView);

            _listView.itemsSource.Add(effect);
            _listView.RefreshItems();

            int index = _listView.itemsSource.Count - 1;
            _listView.selectedIndex = index;

            _inspectorController.Select(effect);
        }
        
        
        public abstract void DeleteOnClickedButton();



        public abstract void CreateOnClickedButton(EventBase evt);



        protected abstract void BindToList(VisualElement visualElement, int index);
    }
}
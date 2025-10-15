using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    public class InspectorController
    {
        public InspectorController(VisualElement inspectorView, IMGUIContainer imguiContainer)
        {
            this._inspectorView = inspectorView;
            this._imguiContainer = imguiContainer;
        }
        
        

        private IMGUIContainer _imguiContainer;
        private VisualElement _inspectorView;
        
        private Editor _inspectorEditor;
        private SerializedObject _selectedObject;



        public IMGUIContainer container
        {
            get { return _imguiContainer; }
            
            internal set { _imguiContainer = value; }
        }

        public VisualElement inspectorView
        {
            get { return _inspectorView; }
        }

        public SerializedObject selectedObject
        {
            get { return _selectedObject; }
        }

        public IStyle style
        {
            get { return _inspectorView.style; }
        }

        

        public void Select(ScriptableObject selectedAction)
        {
            if (_inspectorEditor == null || _inspectorEditor.target != selectedAction)
            {
                _inspectorEditor = Editor.CreateEditor(selectedAction);
                _selectedObject = new SerializedObject(selectedAction);
            }

            _imguiContainer.onGUIHandler = this.RenderActionEditorUI;
            ActionBuildEditor.Instance.currentActiveTab.OnSelected();
        }


        public void ClearInspector()
        {
            // IMGUIContainer 해제
            if (container == null)
            {
                return;
            }

            this.container.onGUIHandler = null;
            
            this.style.display = DisplayStyle.None;
        }


        public void DisposeObject()
        {
            if (_selectedObject == null)
            {
                return;
            }

            _selectedObject.Dispose();
            _selectedObject = null;
        }
        
        
        private void RenderActionEditorUI()
        {
            if (_inspectorEditor == null)
            {
                Object target = _selectedObject.targetObject;
                Assert.IsNotNull(target, "targetObject is null");
                _inspectorEditor = Editor.CreateEditor(target);
            }

            Assert.IsNotNull(_inspectorEditor);
            _inspectorEditor.OnInspectorGUI();
        }
    }
}
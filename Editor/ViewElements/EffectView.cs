using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ActionBuilder.Tool
{
    [UxmlElement]
    public partial class EffectView : VisualElement
    {
        public event Action<EffectView> onDeleteRequested;
        
        private SerializedProperty _serializedProperty;
        private SerializedObject _serializedObject;
        
        private IMGUIContainer _imguiContainer;
        private Toggle _enableToggle;
        private Foldout _foldout;
        private Button _deleteButton;
        private EffectBase _effect;
        
        
        public EffectBase effect
        {
            get { return _effect; }
        }


        public void Refresh(SerializedProperty property, EffectBase newEffect)
        {
            _imguiContainer = this.Q<IMGUIContainer>();
            _enableToggle = this.Q<Toggle>();
            _deleteButton = this.Q<Button>();
            _foldout = this.Q<Foldout>();
            
            _effect = newEffect;
            _serializedProperty = property;
            _serializedObject = property.serializedObject;
            
            _foldout.text = property.displayName;
            _foldout.value = newEffect.isExpanded;
            
            _enableToggle.value = newEffect.enable;

            _deleteButton.clicked -= this.RequestDeletion;
            _deleteButton.clicked += this.RequestDeletion;
            
            _imguiContainer.onGUIHandler = this.Render;
            _imguiContainer.MarkDirtyRepaint();
        }
        

        private void Render()
        {
            Assert.IsNotNull(_serializedProperty);
            Assert.IsNotNull(_enableToggle);
            
            if (_foldout.value == false)
            {
                return;
            }
            
            _serializedObject.Update();
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();
            EditorGUILayout.PropertyField(_serializedProperty, true);

            if (check.changed)
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }


        private void RequestDeletion()
        {
            onDeleteRequested?.Invoke(this);
        }
    }
}
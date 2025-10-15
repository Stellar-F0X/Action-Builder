using System;
using ActionBuilder.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
        private Label _nameLabel;
        private Button _deleteButton;
        private EffectBase _effect;

        private Editor _editor;


        public EffectBase effect
        {
            get { return _effect; }
        }

        public Object effectObject
        {
            get { return _serializedObject.targetObject; }
        }


        

        public void Refresh(SerializedProperty property, EffectBase newEffect)
        {
            _effect = newEffect;
            _serializedProperty = property;
            _serializedObject = new SerializedObject(property.objectReferenceValue);

            this.InitializeEffectUI(property, newEffect);
        }


        private void InitializeEffectUI(SerializedProperty property, EffectBase newEffect)
        {
            _imguiContainer = this.Q<IMGUIContainer>("field-container");
            _enableToggle = this.Q<Toggle>("enable-toggle");
            _deleteButton = this.Q<Button>("delete-button");
            _foldout = this.Q<Foldout>("main-header");
            _nameLabel = _foldout.Q<Label>();

            this.tooltip = effect.description;

            _foldout.UnregisterValueChangedCallback(this.UpdateEffectExpansion);
            _foldout.RegisterValueChangedCallback(this.UpdateEffectExpansion);

            if (string.IsNullOrEmpty(newEffect.effectName))
            {
                _foldout.text = property.displayName;
            }
            else
            {
                _foldout.text = newEffect.effectName;
            }

            _foldout.SetValueWithoutNotify(newEffect.isExpanded);
            
            SerializedProperty nameProp = _serializedObject.FindProperty("effectName");
            Assert.IsNotNull(nameProp);

            _nameLabel.Unbind();
            _nameLabel.TrackPropertyValue(nameProp, this.ChangeEffectName);

            _enableToggle.UnregisterValueChangedCallback(this.EnableEffect);
            _enableToggle.RegisterValueChangedCallback(this.EnableEffect);
            _enableToggle.SetValueWithoutNotify(newEffect.enable);

            _deleteButton.clicked -= this.RequestDeletion;
            _deleteButton.clicked += this.RequestDeletion;

            _editor = Editor.CreateEditor(effectObject);
            _imguiContainer.onGUIHandler = this.Render;
        }
        

        private void Render()
        {
            Assert.IsNotNull(_serializedProperty);
            Assert.IsNotNull(_foldout);
            Assert.IsNotNull(_editor);
            
            if (this._foldout.value == false)
            {
                return;
            }

            _serializedObject.Update();

            using var check = new EditorGUI.ChangeCheckScope();
            
            using (new EditorGUI.DisabledScope(!_enableToggle.value))
            {
                _editor.OnInspectorGUI();
            }

            _serializedProperty.isExpanded = true;
                
            if (check.changed)
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }


#region UI Events

        private void RequestDeletion()
        {
            onDeleteRequested?.Invoke(this);
            EditorUtility.SetDirty(effectObject);
        }


        private void UpdateEffectExpansion(ChangeEvent<bool> evt)
        {
            _effect.isExpanded = evt.newValue;
            EditorUtility.SetDirty(effectObject);
        }


        private void EnableEffect(ChangeEvent<bool> evt)
        {
            _effect.enable = evt.newValue;
            EditorUtility.SetDirty(effectObject);
        }


        private void ChangeEffectName(SerializedProperty prop)
        {
            _effect.effectName = prop.stringValue;
            _foldout.text = prop.stringValue;
            EditorUtility.SetDirty(effectObject);
        }

#endregion
    }
}
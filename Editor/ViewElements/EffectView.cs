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
        private GUIContent _iconContent;


        public EffectBase effect
        {
            get { return _effect; }
        }

        public Object targetObject
        {
            get { return _serializedObject.targetObject; }
        }



#region Init

        public void Refresh(SerializedProperty property, EffectBase newEffect)
        {
            _effect = newEffect;
            _serializedProperty = property;
            _serializedObject = property.serializedObject;

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

            if (string.IsNullOrEmpty(newEffect.name))
            {
                _foldout.text = property.displayName;
            }
            else
            {
                _foldout.text = newEffect.name;
            }

            _foldout.SetValueWithoutNotify(newEffect.isExpanded);

            SerializedProperty nameProp = _serializedProperty.FindPropertyRelative("name");
            Assert.IsNotNull(nameProp, "name property not found in EffectBase.");

            _nameLabel.Unbind();
            _nameLabel.TrackPropertyValue(nameProp, this.ChangeEffectName);

            _enableToggle.UnregisterValueChangedCallback(this.EnableEffect);
            _enableToggle.RegisterValueChangedCallback(this.EnableEffect);
            _enableToggle.SetValueWithoutNotify(newEffect.enable);

            _deleteButton.clicked -= this.RequestDeletion;
            _deleteButton.clicked += this.RequestDeletion;

            _imguiContainer.onGUIHandler = this.Render;
        }

#endregion

        
        private void Render()
        {
            Assert.IsNotNull(_serializedProperty);
            Assert.IsNotNull(_foldout);

            if (_foldout.value == false)
            {
                return;
            }

            _serializedObject.Update();
            _serializedProperty.isExpanded = true;

            if (_iconContent is null)
            {
                _iconContent = EditorGUIUtility.IconContent("PreMatCylinder");
                _iconContent.tooltip = _serializedProperty.tooltip;
                _iconContent.text = "Effect Fields";
            }

            this.RenderPropertyFieldWithToggle();
        }

        
        private void RenderPropertyFieldWithToggle()
        {
            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();
            
            bool disabled = !_enableToggle.value;
            
            using (new EditorGUI.DisabledScope(disabled))
            {
                EditorGUILayout.PropertyField(_serializedProperty, _iconContent, true);
            }

            if (check.changed)
            {
                _imguiContainer.style.height = EditorGUI.GetPropertyHeight(_serializedProperty);
                _serializedObject.ApplyModifiedProperties();
            }
        }


#region UI Events

        private void RequestDeletion()
        {
            onDeleteRequested?.Invoke(this);
            EditorUtility.SetDirty(targetObject);
        }


        private void UpdateEffectExpansion(ChangeEvent<bool> evt)
        {
            _effect.isExpanded = evt.newValue;
            EditorUtility.SetDirty(targetObject);
        }


        private void EnableEffect(ChangeEvent<bool> evt)
        {
            _effect.enable = evt.newValue;
            EditorUtility.SetDirty(targetObject);
        }


        private void ChangeEffectName(SerializedProperty prop)
        {
            _effect.name = prop.stringValue;
            _foldout.text = prop.stringValue;
            EditorUtility.SetDirty(targetObject);
        }

#endregion
    }
}
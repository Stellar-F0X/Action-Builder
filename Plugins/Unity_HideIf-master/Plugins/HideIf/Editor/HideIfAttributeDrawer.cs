using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HideIf.Runtime;
using UnityEditor;
using UnityEngine;

namespace HideIf.Tool
{
    /// <summary> 조건에 따라 프로퍼티를 숨기는 기능을 제공하는 추상 PropertyDrawer </summary>
    public abstract class HidingAttributeDrawer : PropertyDrawer
    {
        private static Dictionary<Type, Type> _typeToDrawerType;
        private static Dictionary<Type, PropertyDrawer> _drawerTypeToDrawerInstance;
        private static Dictionary<Type, HidingAttribute[]> _attributeCache;
        private static FieldInfo _customPropertyDrawerTargetTypeField;
        private static FieldInfo _customPropertyDrawerUseForChildrenField;
        private static bool _isInitialized = false;

        static HidingAttributeDrawer()
        {
            InitializeStaticFields();
        }

        /// <summary> 정적 필드를 초기화한다. 한 번만 실행된다. </summary>
        private static void InitializeStaticFields()
        {
            _customPropertyDrawerTargetTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            _customPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary> 프로퍼티를 숨겨야 하는지 확인한다. </summary>
        /// <param name="property"> 확인할 SerializedProperty </param>
        /// <returns> 숨겨야 하면 true, 아니면 false </returns>
        public static bool CheckShouldHide(SerializedProperty property)
        {
            try
            {
                Type targetObjectType = property.serializedObject.targetObject.GetType();

                if (!_attributeCache.TryGetValue(targetObjectType, out HidingAttribute[] cachedAttributes))
                {
                    FieldInfo fieldInfo = targetObjectType.GetField(property.name);
                    
                    if (fieldInfo == null)
                    {
                        return false;
                    }

                    cachedAttributes = (HidingAttribute[])fieldInfo.GetCustomAttributes(typeof(HidingAttribute), false);
                    _attributeCache[targetObjectType] = cachedAttributes;
                }

                foreach (HidingAttribute hider in cachedAttributes)
                {
                    if (!ShouldDraw(property.serializedObject, hider))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (CheckShouldHide(property))
            {
                return;
            }

            PropertyDrawer drawer = GetDrawerForProperty(property);

            if (drawer != null)
            {
                drawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (CheckShouldHide(property))
            {
                return -2;
            }

            PropertyDrawer drawer = GetDrawerForProperty(property);

            if (drawer != null)
            {
                return drawer.GetPropertyHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary> 프로퍼티에 적합한 PropertyDrawer를 가져온다. </summary>
        /// <param name="property"> 대상 SerializedProperty </param>
        /// <returns> 적합한 PropertyDrawer 또는 null </returns>
        private PropertyDrawer GetDrawerForProperty(SerializedProperty property)
        {
            if (!_isInitialized)
            {
                PopulateTypeToDrawerMapping();
            }

            try
            {
                Type propertyType = Utilities.GetTargetObjectOfProperty(property).GetType();

                if (_typeToDrawerType.TryGetValue(propertyType, out Type drawerType))
                {
                    return GetOrCreateDrawerInstance(drawerType);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary> 지정된 타입의 PropertyDrawer 인스턴스를 가져오거나 생성한다. </summary>
        /// <param name="drawerType"> PropertyDrawer 타입 </param>
        /// <returns> PropertyDrawer 인스턴스 </returns>
        private PropertyDrawer GetOrCreateDrawerInstance(Type drawerType)
        {
            if (!_drawerTypeToDrawerInstance.TryGetValue(drawerType, out PropertyDrawer drawer))
            {
                drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                _drawerTypeToDrawerInstance[drawerType] = drawer;
            }

            return drawer;
        }

        /// <summary> 모든 PropertyDrawer를 검색하여 타입별 매핑을 생성한다. </summary>
        private void PopulateTypeToDrawerMapping()
        {
            _typeToDrawerType = new Dictionary<Type, Type>();
            _drawerTypeToDrawerInstance = new Dictionary<Type, PropertyDrawer>();
            _attributeCache = new Dictionary<Type, HidingAttribute[]>();

            Type propertyDrawerType = typeof(PropertyDrawer);
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .ToList();

            foreach (Type type in allTypes)
            {
                if (!propertyDrawerType.IsAssignableFrom(type) || type.IsAbstract)
                {
                    continue;
                }

                var customPropertyDrawers = type.GetCustomAttributes(true).OfType<CustomPropertyDrawer>().ToList();

                foreach (CustomPropertyDrawer propertyDrawer in customPropertyDrawers)
                {
                    Type targetedType = (Type)_customPropertyDrawerTargetTypeField.GetValue(propertyDrawer);
                    _typeToDrawerType[targetedType] = type;

                    bool useForChildren = (bool)_customPropertyDrawerUseForChildrenField.GetValue(propertyDrawer);

                    if (useForChildren)
                    {
                        RegisterChildTypes(targetedType, type, allTypes);
                    }
                }
            }

            _isInitialized = true;
        }

        /// <summary> 부모 타입의 자식 타입들을 등록한다. </summary>
        /// <param name="parentType"> 부모 타입 </param>
        /// <param name="drawerType"> PropertyDrawer 타입 </param>
        /// <param name="allTypes"> 모든 타입 목록 </param>
        private void RegisterChildTypes(Type parentType, Type drawerType, List<Type> allTypes)
        {
            var childTypes = allTypes.Where(type => parentType.IsAssignableFrom(type) && type != parentType);

            foreach (Type childType in childTypes)
            {
                if (!_typeToDrawerType.ContainsKey(childType))
                {
                    _typeToDrawerType[childType] = drawerType;
                }
            }
        }

        /// <summary> HidingAttribute 타입에 따라 적절한 ShouldDraw 메서드를 호출한다. </summary>
        /// <param name="obj"> 대상 SerializedObject </param>
        /// <param name="hider"> HidingAttribute 인스턴스 </param>
        /// <returns> 프로퍼티를 그려야 하면 true </returns>
        private static bool ShouldDraw(SerializedObject obj, HidingAttribute hider)
        {
            return hider switch
            {
                HideIfAttribute hideIf => HideIfAttributeDrawer.ShouldDraw(obj, hideIf),
                
                HideIfNullAttribute hideIfNull => HideIfNullAttributeDrawer.ShouldDraw(obj, hideIfNull),
                
                HideIfNotNullAttribute hideIfNotNull => HideIfNotNullAttributeDrawer.ShouldDraw(obj, hideIfNotNull),
                
                HideIfEnumValueAttribute hideIfEnum  => HideIfEnumValueAttributeDrawer.ShouldDraw(obj, hideIfEnum),
                
                _ => false
            };
        }
    }

    /// <summary> HideIfAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer : HidingAttributeDrawer
    {
        /// <summary> bool 조건에 따라 프로퍼티를 그릴지 결정한다. </summary>
        /// <param name="obj"> 대상 SerializedObject </param>
        /// <param name="attribute"> HideIfAttribute </param>
        /// <returns> 프로퍼티를 그려야 하면 true </returns>
        public static bool ShouldDraw(SerializedObject obj, HideIfAttribute attribute)
        {
            SerializedProperty property = obj.FindProperty(attribute.variable);

            if (property == null)
            {
                return true;
            }

            return property.boolValue != attribute.state;
        }
    }

    /// <summary> HideIfNullAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfNullAttribute))]
    public class HideIfNullAttributeDrawer : HidingAttributeDrawer
    {
        /// <summary> 참조가 null이 아닌지 확인하여 프로퍼티를 그릴지 결정한다. </summary>
        /// <param name="obj"> 대상 SerializedObject </param>
        /// <param name="hideIfNullAttribute"> HideIfNullAttribute </param>
        /// <returns> 프로퍼티를 그려야 하면 true </returns>
        public static bool ShouldDraw(SerializedObject obj, HideIfNullAttribute hideIfNullAttribute)
        {
            SerializedProperty property = obj.FindProperty(hideIfNullAttribute.variable);

            if (property == null)
            {
                return true;
            }

            return property.objectReferenceValue != null;
        }
    }

    /// <summary> HideIfNotNullAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfNotNullAttribute))]
    public class HideIfNotNullAttributeDrawer : HidingAttributeDrawer
    {
        /// <summary> 참조가 null인지 확인하여 프로퍼티를 그릴지 결정한다. </summary>
        /// <param name="obj"> 대상 SerializedObject </param>
        /// <param name="hideIfNotNullAttribute"> HideIfNotNullAttribute </param>
        /// <returns> 프로퍼티를 그려야 하면 true </returns>
        public static bool ShouldDraw(SerializedObject obj, HideIfNotNullAttribute hideIfNotNullAttribute)
        {
            SerializedProperty property = obj.FindProperty(hideIfNotNullAttribute.variable);

            if (property == null)
            {
                return true;
            }

            return property.objectReferenceValue == null;
        }
    }

    /// <summary> HideIfEnumValueAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfEnumValueAttribute))]
    public class HideIfEnumValueAttributeDrawer : HidingAttributeDrawer
    {
        /// <summary> Enum 값이 지정된 상태 중 하나인지 확인하여 프로퍼티를 그릴지 결정한다. </summary>
        /// <param name="obj"> 대상 SerializedObject </param>
        /// <param name="hideIfEnumValueAttribute"> HideIfEnumValueAttribute </param>
        /// <returns> 프로퍼티를 그려야 하면 true </returns>
        public static bool ShouldDraw(SerializedObject obj, HideIfEnumValueAttribute hideIfEnumValueAttribute)
        {
            SerializedProperty enumProperty = obj.FindProperty(hideIfEnumValueAttribute.variable);

            if (enumProperty == null)
            {
                return true;
            }

            bool equal = hideIfEnumValueAttribute.states.Contains(enumProperty.intValue);

            return equal != hideIfEnumValueAttribute.hideIfEqual;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    public enum EHideIf
    {
        Equal,
        NotEqual
    }

    public class HidingAttribute : PropertyAttribute { }

    /// <summary> bool 변수의 상태에 따라 필드를 숨긴다 </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute : HidingAttribute
    {
        public readonly string Variable;
        public readonly bool State;

        public HideIfAttribute(string variable, bool state, int order = 0)
        {
            this.Variable = variable;
            this.State = state;
            this.order = order;
        }
    }

    /// <summary> Object 변수가 null일 때 필드를 숨긴다 </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfNullAttribute : HidingAttribute
    {
        public readonly string Variable;

        public HideIfNullAttribute(string variable, int order = 0)
        {
            this.Variable = variable;
            this.order = order;
        }
    }

    /// <summary> Object 변수가 null이 아닐 때 필드를 숨긴다 </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfNotNullAttribute : HidingAttribute
    {
        public readonly string Variable;

        public HideIfNotNullAttribute(string variable, int order = 0)
        {
            this.Variable = variable;
            this.order = order;
        }
    }

    /// <summary> Enum 값에 따라 필드를 숨긴다 </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfEnumValueAttribute : HidingAttribute
    {
        public readonly string Variable;
        public readonly int[] States;
        public readonly bool HideIfEqual;

        public HideIfEnumValueAttribute(string variable, EHideIf hideIf, params int[] states)
        {
            this.Variable = variable;
            this.HideIfEqual = hideIf == EHideIf.Equal;
            this.States = states;
            this.order = -1;
        }
    }

#if UNITY_EDITOR
    /// <summary> SerializedProperty의 대상 객체에 접근하기 위한 유틸리티 </summary>
    internal static class PropertyHelper
    {
        private readonly static Dictionary<string, PropertyInfo> _propertyCache = new Dictionary<string, PropertyInfo>();
        
        private readonly static Dictionary<string, FieldInfo> _fieldCache = new Dictionary<string, FieldInfo>();
        
        private const string ARRAY_DATA_PATTERN = ".Array.data[";
        
        private const string ARRAY_BRACKET_OPEN = "[";


        /// <summary> SerializedProperty의 실제 타겟 객체를 가져온다 </summary>
        internal static object GetTargetObject(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            string path = property.propertyPath.Replace(ARRAY_DATA_PATTERN, ARRAY_BRACKET_OPEN);
            object targetObject = property.serializedObject.targetObject;
            string[] pathElements = path.Split('.');

            for (int i = 0; i < pathElements.Length; i++)
            {
                string element = pathElements[i];

                if (element.Contains(ARRAY_BRACKET_OPEN))
                {
                    targetObject = GetArrayElement(targetObject, element);
                }
                else
                {
                    targetObject = GetMemberValue(targetObject, element);
                }

                if (targetObject == null)
                {
                    return null;
                }
            }

            return targetObject;
        }

        /// <summary> 객체의 필드 또는 프로퍼티로부터 값을 가져온다 </summary>
        private static object GetMemberValue(object sourceObject, string memberName)
        {
            if (sourceObject == null)
            {
                return null;
            }

            Type sourceType = sourceObject.GetType();
            string cacheKey = sourceType.FullName + "." + memberName;

            if (_fieldCache.ContainsKey(cacheKey))
            {
                FieldInfo cachedField = _fieldCache[cacheKey];
                if (cachedField != null)
                {
                    return cachedField.GetValue(sourceObject);
                }
            }
            else
            {
                FieldInfo field = FindFieldInHierarchy(sourceType, memberName);
                _fieldCache[cacheKey] = field;

                if (field != null)
                {
                    return field.GetValue(sourceObject);
                }
            }

            if (_propertyCache.ContainsKey(cacheKey))
            {
                PropertyInfo cachedProperty = _propertyCache[cacheKey];
                if (cachedProperty != null)
                {
                    return cachedProperty.GetValue(sourceObject);
                }
            }
            else
            {
                PropertyInfo property = FindPropertyInHierarchy(sourceType, memberName);
                _propertyCache[cacheKey] = property;

                if (property != null)
                {
                    return property.GetValue(sourceObject);
                }
            }

            return null;
        }

        /// <summary> 타입 계층 구조에서 필드를 찾는다 </summary>
        private static FieldInfo FindFieldInHierarchy(Type sourceType, string memberName)
        {
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            Type currentType = sourceType;

            while (currentType != null)
            {
                FieldInfo fieldInfo = currentType.GetField(memberName, FLAGS);

                if (fieldInfo != null)
                {
                    return fieldInfo;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary> 타입 계층 구조에서 프로퍼티를 찾는다 </summary>
        private static PropertyInfo FindPropertyInHierarchy(Type sourceType, string memberName)
        {
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;

            
            Type currentType = sourceType;

            while (currentType != null)
            {
                PropertyInfo propertyInfo = currentType.GetProperty(memberName, FLAGS);

                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary> 배열 요소로부터 값을 가져온다 </summary>
        private static object GetArrayElement(object sourceObject, string arrayElement)
        {
            int bracketIndex = arrayElement.IndexOf(ARRAY_BRACKET_OPEN);
            string elementName = arrayElement.Substring(0, bracketIndex);
            string indexString = arrayElement.Substring(bracketIndex + 1, arrayElement.Length - bracketIndex - 2);

            int index = 0;
            if (int.TryParse(indexString, out index) == false || index < 0)
            {
                return null;
            }

            object collectionObject = GetMemberValue(sourceObject, elementName);

            if (collectionObject == null)
            {
                return null;
            }

            IEnumerable enumerable = collectionObject as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (enumerator.MoveNext() == false)
                {
                    return null;
                }
            }

            return enumerator.Current;
        }
    }

    /// <summary> 조건에 따라 프로퍼티를 숨기는 기능을 제공하는 추상 PropertyDrawer </summary>
    public abstract class HidingAttributeDrawer : PropertyDrawer
    {
        private static Dictionary<Type, PropertyDrawer> _drawerTypeToDrawerInstance = new Dictionary<Type, PropertyDrawer>();
        
        private static Dictionary<Type, HidingAttribute[]> _attributeCache = new Dictionary<Type, HidingAttribute[]>();
        
        private static Dictionary<Type, Type> _typeToDrawerType = new Dictionary<Type, Type>();
        
        private static FieldInfo _customPropertyDrawerUseForChildrenField = null;
        
        private static FieldInfo _customPropertyDrawerTargetTypeField = null;
        
        private static bool _isInitialized = false;

        
        static HidingAttributeDrawer()
        {
            _customPropertyDrawerTargetTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            
            _customPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
        }


        /// <summary> 프로퍼티를 숨겨야 하는지 확인한다 </summary>
        private static bool CheckShouldHide(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
            {
                return false;
            }

            Type targetObjectType = property.serializedObject.targetObject.GetType();

            if (_attributeCache == null)
            {
                _attributeCache = new Dictionary<Type, HidingAttribute[]>();
            }

            HidingAttribute[] cachedAttributes = null;
            if (_attributeCache.ContainsKey(targetObjectType) == false)
            {
                FieldInfo fieldInfo = targetObjectType.GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo == null)
                {
                    return false;
                }

                object[] attributes = fieldInfo.GetCustomAttributes(typeof(HidingAttribute), false);
                cachedAttributes = new HidingAttribute[attributes.Length];
                for (int i = 0; i < attributes.Length; i++)
                {
                    cachedAttributes[i] = (HidingAttribute)attributes[i];
                }

                _attributeCache[targetObjectType] = cachedAttributes;
            }
            else
            {
                cachedAttributes = _attributeCache[targetObjectType];
            }

            for (int i = 0; i < cachedAttributes.Length; i++)
            {
                HidingAttribute hider = cachedAttributes[i];

                if (ShouldDraw(property.serializedObject, hider) == false)
                {
                    return true;
                }
            }

            return false;
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

        /// <summary> 프로퍼티에 적합한 PropertyDrawer를 가져온다 </summary>
        private PropertyDrawer GetDrawerForProperty(SerializedProperty property)
        {
            if (_isInitialized == false)
            {
                PopulateTypeToDrawerMapping();
            }

            object targetObject = PropertyHelper.GetTargetObject(property);

            if (targetObject == null)
            {
                return null;
            }

            Type propertyType = targetObject.GetType();

            if (_typeToDrawerType.ContainsKey(propertyType))
            {
                Type drawerType = _typeToDrawerType[propertyType];
                return GetOrCreateDrawerInstance(drawerType);
            }

            return null;
        }

        /// <summary> 지정된 타입의 PropertyDrawer 인스턴스를 가져오거나 생성한다 </summary>
        private PropertyDrawer GetOrCreateDrawerInstance(Type drawerType)
        {
            if (_drawerTypeToDrawerInstance.ContainsKey(drawerType) == false)
            {
                PropertyDrawer drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                _drawerTypeToDrawerInstance[drawerType] = drawer;
            }

            return _drawerTypeToDrawerInstance[drawerType];
        }

        /// <summary> 모든 PropertyDrawer를 검색하여 타입별 매핑을 생성한다 </summary>
        private void PopulateTypeToDrawerMapping()
        {
            if (_typeToDrawerType == null)
            {
                _typeToDrawerType = new Dictionary<Type, Type>();
            }

            if (_drawerTypeToDrawerInstance == null)
            {
                _drawerTypeToDrawerInstance = new Dictionary<Type, PropertyDrawer>();
            }

            if (_attributeCache == null)
            {
                _attributeCache = new Dictionary<Type, HidingAttribute[]>();
            }

            Type propertyDrawerType = typeof(PropertyDrawer);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> allTypes = new List<Type>();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                Type[] types = null;

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                for (int j = 0; j < types.Length; j++)
                {
                    allTypes.Add(types[j]);
                }
            }

            for (int i = 0; i < allTypes.Count; i++)
            {
                Type type = allTypes[i];

                if (propertyDrawerType.IsAssignableFrom(type) == false || type.IsAbstract)
                {
                    continue;
                }

                object[] attributes = type.GetCustomAttributes(true);
                List<CustomPropertyDrawer> customPropertyDrawers = new List<CustomPropertyDrawer>();

                for (int j = 0; j < attributes.Length; j++)
                {
                    CustomPropertyDrawer customDrawer = attributes[j] as CustomPropertyDrawer;
                    if (customDrawer != null)
                    {
                        customPropertyDrawers.Add(customDrawer);
                    }
                }

                for (int j = 0; j < customPropertyDrawers.Count; j++)
                {
                    CustomPropertyDrawer propertyDrawer = customPropertyDrawers[j];

                    if (_customPropertyDrawerTargetTypeField == null)
                    {
                        continue;
                    }

                    Type targetedType = (Type)_customPropertyDrawerTargetTypeField.GetValue(propertyDrawer);
                    _typeToDrawerType[targetedType] = type;

                    if (_customPropertyDrawerUseForChildrenField == null)
                    {
                        continue;
                    }

                    bool useForChildren = (bool)_customPropertyDrawerUseForChildrenField.GetValue(propertyDrawer);

                    if (useForChildren)
                    {
                        RegisterChildTypes(targetedType, type, allTypes);
                    }
                }
            }

            _isInitialized = true;
        }

        /// <summary> 부모 타입의 자식 타입들을 등록한다 </summary>
        private void RegisterChildTypes(Type parentType, Type drawerType, List<Type> allTypes)
        {
            for (int i = 0; i < allTypes.Count; i++)
            {
                Type childType = allTypes[i];

                if (!parentType.IsAssignableFrom(childType) || childType == parentType)
                {
                    continue;
                }

                if (_typeToDrawerType.ContainsKey(childType) == false)
                {
                    _typeToDrawerType[childType] = drawerType;
                }
            }
        }

        /// <summary> HidingAttribute 타입에 따라 적절한 검사를 수행한다 </summary>
        private static bool ShouldDraw(SerializedObject serializedObject, HidingAttribute hider)
        {
            HideIfAttribute hideIf = hider as HideIfAttribute;
            if (hideIf != null)
            {
                return CheckBoolCondition(serializedObject, hideIf.Variable, hideIf.State);
            }

            HideIfNullAttribute hideIfNull = hider as HideIfNullAttribute;
            if (hideIfNull != null)
            {
                return CheckNullCondition(serializedObject, hideIfNull.Variable, false);
            }

            HideIfNotNullAttribute hideIfNotNull = hider as HideIfNotNullAttribute;
            if (hideIfNotNull != null)
            {
                return CheckNullCondition(serializedObject, hideIfNotNull.Variable, true);
            }

            HideIfEnumValueAttribute hideIfEnum = hider as HideIfEnumValueAttribute;
            if (hideIfEnum != null)
            {
                return CheckEnumCondition(serializedObject, hideIfEnum.Variable, hideIfEnum.States, hideIfEnum.HideIfEqual);
            }

            return false;
        }

        /// <summary> bool 조건을 검사한다 </summary>
        private static bool CheckBoolCondition(SerializedObject serializedObject, string variableName, bool state)
        {
            SerializedProperty property = serializedObject.FindProperty(variableName);

            if (property == null)
            {
                return true;
            }

            return property.boolValue != state;
        }

        /// <summary> null 조건을 검사한다 </summary>
        private static bool CheckNullCondition(SerializedObject serializedObject, string variableName, bool expectNull)
        {
            SerializedProperty property = serializedObject.FindProperty(variableName);

            if (property == null)
            {
                return true;
            }

            bool isNull = property.objectReferenceValue == null;

            if (expectNull)
            {
                return isNull;
            }
            else
            {
                return isNull == false;
            }
        }

        /// <summary> Enum 조건을 검사한다 </summary>
        private static bool CheckEnumCondition(SerializedObject serializedObject, string variableName, int[] states, bool hideIfEqual)
        {
            SerializedProperty property = serializedObject.FindProperty(variableName);

            if (property == null)
            {
                return true;
            }

            bool isEqual = false;

            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] != property.intValue)
                {
                    continue;
                }

                isEqual = true;
                break;
            }

            return isEqual != hideIfEqual;
        }
    }

    /// <summary> HideIfAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer : HidingAttributeDrawer { }

    /// <summary> HideIfNullAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfNullAttribute))]
    public class HideIfNullAttributeDrawer : HidingAttributeDrawer { }

    /// <summary> HideIfNotNullAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfNotNullAttribute))]
    public class HideIfNotNullAttributeDrawer : HidingAttributeDrawer { }

    /// <summary> HideIfEnumValueAttribute를 처리하는 PropertyDrawer </summary>
    [CustomPropertyDrawer(typeof(HideIfEnumValueAttribute))]
    public class HideIfEnumValueAttributeDrawer : HidingAttributeDrawer { }
#endif
}
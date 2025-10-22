using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace HideIf.Tool
{
    /// <summary> SerializedProperty의 대상 객체에 접근하기 위한 유틸리티 클래스 </summary>
    public static class Utilities
    {
        private const string ARRAY_DATA_PATTERN = ".Array.data[";
        private const string ARRAY_BRACKET_OPEN = "[";
        private const string ARRAY_BRACKET_CLOSE = "]";

        private static readonly Dictionary<(Type, string), FieldInfo> _fieldInfoCache = new();
        private static readonly Dictionary<(Type, string), PropertyInfo> _propertyInfoCache = new();

        /// <summary> SerializedProperty의 실제 타겟 객체를 가져온다. 중첩된 객체와 배열도 지원한다. </summary>
        /// <param name="property"> 대상 SerializedProperty </param>
        /// <returns> 타겟 객체 또는 null </returns>
        public static object GetTargetObjectOfProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(ARRAY_DATA_PATTERN, ARRAY_BRACKET_OPEN);
            object targetObject = property.serializedObject.targetObject;
            string[] pathElements = path.Split('.');

            foreach (string element in pathElements)
            {
                if (element.Contains(ARRAY_BRACKET_OPEN))
                {
                    targetObject = GetValueFromArrayElement(targetObject, element);
                }
                else
                {
                    targetObject = GetValueFromMember(targetObject, element);
                }

                if (targetObject == null)
                {
                    return null;
                }
            }

            return targetObject;
        }

        /// <summary> 객체의 필드 또는 프로퍼티로부터 값을 가져온다. 리플렉션 결과를 캐싱한다. </summary>
        /// <param name="sourceObject"> 대상 객체 </param>
        /// <param name="memberName"> 필드 또는 프로퍼티 이름 </param>
        /// <returns> 해당 멤버의 값 또는 null </returns>
        private static object GetValueFromMember(object sourceObject, string memberName)
        {
            if (sourceObject == null)
            {
                return null;
            }

            Type sourceType = sourceObject.GetType();
            
            if (TryGetCachedField(sourceType, memberName, out FieldInfo fieldInfo))
            {
                return fieldInfo.GetValue(sourceObject);
            }

            if (TryGetCachedProperty(sourceType, memberName, out PropertyInfo propertyInfo))
            {
                return propertyInfo.GetValue(sourceObject, null);
            }

            return null;
        }

        /// <summary> 캐시된 FieldInfo를 가져오거나 리플렉션으로 찾아 캐싱한다. </summary>
        /// <param name="sourceType"> 대상 타입 </param>
        /// <param name="memberName"> 필드 이름 </param>
        /// <param name="fieldInfo"> 찾은 FieldInfo </param>
        /// <returns> FieldInfo를 찾았으면 true </returns>
        private static bool TryGetCachedField(Type sourceType, string memberName, out FieldInfo fieldInfo)
        {
            var cacheKey = (sourceType, memberName);

            if (_fieldInfoCache.TryGetValue(cacheKey, out fieldInfo))
            {
                return fieldInfo != null;
            }

            fieldInfo = FindFieldInHierarchy(sourceType, memberName);
            _fieldInfoCache[cacheKey] = fieldInfo;

            return fieldInfo != null;
        }

        /// <summary> 캐시된 PropertyInfo를 가져오거나 리플렉션으로 찾아 캐싱한다. </summary>
        /// <param name="sourceType"> 대상 타입 </param>
        /// <param name="memberName"> 프로퍼티 이름 </param>
        /// <param name="propertyInfo"> 찾은 PropertyInfo </param>
        /// <returns> PropertyInfo를 찾았으면 true </returns>
        private static bool TryGetCachedProperty(Type sourceType, string memberName, out PropertyInfo propertyInfo)
        {
            var cacheKey = (sourceType, memberName);

            if (_propertyInfoCache.TryGetValue(cacheKey, out propertyInfo))
            {
                return propertyInfo != null;
            }

            propertyInfo = FindPropertyInHierarchy(sourceType, memberName);
            _propertyInfoCache[cacheKey] = propertyInfo;

            return propertyInfo != null;
        }

        /// <summary> 타입 계층 구조에서 필드를 찾는다. </summary>
        /// <param name="sourceType"> 시작 타입 </param>
        /// <param name="memberName"> 필드 이름 </param>
        /// <returns> 찾은 FieldInfo 또는 null </returns>
        private static FieldInfo FindFieldInHierarchy(Type sourceType, string memberName)
        {
            Type currentType = sourceType;

            while (currentType != null)
            {
                FieldInfo fieldInfo = currentType.GetField(
                    memberName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                );

                if (fieldInfo != null)
                {
                    return fieldInfo;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary> 타입 계층 구조에서 프로퍼티를 찾는다. </summary>
        /// <param name="sourceType"> 시작 타입 </param>
        /// <param name="memberName"> 프로퍼티 이름 </param>
        /// <returns> 찾은 PropertyInfo 또는 null </returns>
        private static PropertyInfo FindPropertyInHierarchy(Type sourceType, string memberName)
        {
            Type currentType = sourceType;

            while (currentType != null)
            {
                PropertyInfo propertyInfo = currentType.GetProperty(
                    memberName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly
                );

                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary> 배열 요소로부터 값을 가져온다. </summary>
        /// <param name="sourceObject"> 배열을 소유한 객체 </param>
        /// <param name="arrayElement"> "[인덱스]" 형식의 배열 요소 표현 </param>
        /// <returns> 배열의 해당 인덱스 요소 또는 null </returns>
        private static object GetValueFromArrayElement(object sourceObject, string arrayElement)
        {
            int bracketIndex = arrayElement.IndexOf(ARRAY_BRACKET_OPEN);
            string elementName = arrayElement.Substring(0, bracketIndex);
            string indexString = arrayElement.Substring(bracketIndex + 1, arrayElement.Length - bracketIndex - 2);

            if (!int.TryParse(indexString, out int index) || index < 0)
            {
                return null;
            }

            object collectionObject = GetValueFromMember(sourceObject, elementName);

            if (collectionObject is not IEnumerable enumerable)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }
    }
    
    
    

    public static class Extensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> constructor)
        {
            if (dict.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = constructor.Invoke();
            dict[key] = value;
            return value;
        }
    }
}
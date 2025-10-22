using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ActionBuilder.Tool
{
    public static class Utility
    {
        public static object CreateStatKeyInstanceByType(Type type, object param)
        {
            if (type.IsSubclassOf(typeof(UObject)))
            {
                return UObject.Instantiate(param as UObject);
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, param.ToString());
            }

            if (type == typeof(string))
            {
                string result = (string)param;
                return string.IsNullOrEmpty(result) ? string.Empty : result;
            }

            if (param is ICloneable cloneable)
            {
                return cloneable.Clone();
            }

            if (type.IsValueType)
            {
                return ConvertObjectToSpeficType(type, param);
            }


            ConstructorInfo copyCtor = type.GetConstructor(new[] { type });

            if (copyCtor != null)
            {
                return copyCtor.Invoke(new[] { param });
            }

            if (TryJsonCopy(ref param, type))
            {
                return param;
            }

            throw new StatKeyCastException($"{typeof(StatSetDrawer)}: Failed to convert value '{param}' to stat statKey of type '{type.FullName}'.");
        }


        private static object ConvertObjectToSpeficType(Type type, object param)
        {
            try
            {
                return Convert.ChangeType(param, type);
            }
            catch
            {
                return param;
            }
        }


        private static bool TryJsonCopy(ref object param, Type type)
        {
            string json = JsonUtility.ToJson(param);

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            object newObject = Activator.CreateInstance(type);
            JsonUtility.FromJsonOverwrite(json, newObject);
            param = newObject;
            return true;
        }
        
        
        public static float GetTotalHeight(this SerializedObject serializedObject)
        {
            float totalHeight = 0f;

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                totalHeight += EditorGUI.GetPropertyHeight(property, includeChildren: true);
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }
    }
}
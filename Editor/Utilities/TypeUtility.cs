using System;

namespace StatController.Tool
{
    public class TypeUtility
    {
        public static object CreateInstance(Type type)
        {
            if (type.IsEnum)
            {
                return Enum.GetValues(type).GetValue(0);
            }

            if (type == typeof(string))
            {
                return string.Empty;
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }
    }
}
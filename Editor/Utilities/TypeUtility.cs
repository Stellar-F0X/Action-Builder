using System;

namespace StatController.Tool
{
    public class TypeUtility
    {
        public static object CreateInstance(Type type, object param)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, param.ToString());
            }

            if (type == typeof(string))
            {
                string result = (string)param;
                return string.IsNullOrEmpty(result) ? string.Empty : result;
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }
    }
}
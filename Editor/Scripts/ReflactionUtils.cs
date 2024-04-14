using System.Reflection;
using System;

namespace Feif.UIFramework.Editor
{
    public class ReflactionUtils
    {
        public static object RunClassFunc(Type classType, string functionName, params object[] args)
        {
            if (classType == null) return null;

            if (string.IsNullOrEmpty(functionName)) return null;

            if (args == null) args = new object[] { };

            var methodInfo = classType.GetMethod(functionName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo == null) return null;

            return methodInfo.Invoke(null, args);
        }

        public static T RunClassFunc<T>(Type classType, string function, params object[] args)
        {
            return (T)RunClassFunc(classType, function, args);
        }

        public static object RunInstanceFunc(object instance, string functionName, params object[] args)
        {
            if (instance == null) return null;

            if (string.IsNullOrEmpty(functionName)) return null;

            if (args == null) args = new object[] { };

            var type = instance.GetType();
            var methodInfo = type.GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo == null) return null;

            return methodInfo.Invoke(instance, args);
        }

        public static T RunInstanceFunc<T>(object instance, string function, params object[] args)
        {
            return (T)RunInstanceFunc(instance, function, args);
        }

        public static object GetInstanceField(object instance, string fieldName)
        {
            if (instance == null) return null;

            if (string.IsNullOrEmpty(fieldName)) return null;

            var type = instance.GetType();
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null) return null;

            return field.GetValue(instance);
        }

        public static T GetInstanceField<T>(object instance, string fieldName)
        {
            return (T)GetInstanceField(instance, fieldName);
        }
    }
}
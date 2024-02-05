using System;
using System.Reflection;

namespace BlessingsVanir.ReflectiveHooks
{
    public class ReflectionAccessor
    {
        private readonly object targetInstance;

        public ReflectionAccessor(object instance)
        {
            targetInstance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public object InvokeMethod(string methodName, params object[] parameters)
        {
            Type targetType = targetInstance.GetType();
            MethodInfo methodInfo = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (methodInfo != null)
            {
                return methodInfo.Invoke(targetInstance, parameters);
            }
            else
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in the target type.");
            }
        }

        public object GetFieldOrPropertyValue(string fieldName)
        {
            Type targetType = targetInstance.GetType();

            FieldInfo fieldInfo = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(targetInstance);
            }

            PropertyInfo propertyInfo = targetType.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(targetInstance);
            }

            throw new InvalidOperationException($"Field or property '{fieldName}' not found in the target type.");
        }
        public object GetFieldOrPropertyValue(object instance, string fieldNameOrPropertyName)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(fieldNameOrPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(instance);
            }
            else
            {
                FieldInfo fieldInfo = type.GetField(fieldNameOrPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(instance);
                }
                else
                {
                    // If neither property nor field is found, try to find a method with the given name
                    MethodInfo methodInfo = type.GetMethod(fieldNameOrPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (methodInfo != null)
                    {
                        // If it's a method, invoke it (you might want to handle methods differently based on your requirements)
                        return methodInfo.Invoke(instance, null);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Field, property, or method '{fieldNameOrPropertyName}' not found in the object.");
                    }
                }
            }
        }


        public object GetNestedFieldOrPropertyValue(string nestedTypeName, string fieldName)
        {
            Type targetType = targetInstance.GetType();
            Type nestedType = targetType.GetNestedType(nestedTypeName, BindingFlags.NonPublic | BindingFlags.Public);

            if (nestedType != null)
            {
                FieldInfo fieldInfo = nestedType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(targetInstance);
                }

                PropertyInfo propertyInfo = nestedType.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(targetInstance);
                }

                throw new InvalidOperationException($"Field or property '{fieldName}' not found in the nested type '{nestedTypeName}'.");
            }

            throw new InvalidOperationException($"Nested type '{nestedTypeName}' not found in the target type.");
        }
    }
}

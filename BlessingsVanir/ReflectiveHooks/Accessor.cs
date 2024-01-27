using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingsVanir.ReflectiveHooks
{
    class ReflectiveHooksAccessor
    {
        private object pluginInstance; // Replace with an instance of the plugin class

        public ReflectiveHooksAccessor(object instance)
        {
            pluginInstance = instance;
        }

        public object GetVariableValue(string variableName)
        {
            Type pluginType = pluginInstance.GetType();
            FieldInfo fieldInfo = pluginType.GetField(variableName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(pluginInstance);
            }
            else
            {
                // Handle the case where the variable is not found
                throw new InvalidOperationException($"Variable '{variableName}' not found in the plugin.");
            }
        }
        public Type GetVariableType(string variableName)
        {
            Type pluginType = pluginInstance.GetType();
            FieldInfo fieldInfo = pluginType.GetField(variableName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }
            else
            {
                // Handle the case where the variable is not found
                throw new InvalidOperationException($"Variable '{variableName}' not found in the plugin.");
            }
        }
        public MethodInfo GetMethod(string methodName)
        {
            Type pluginType = pluginInstance.GetType();
            MethodInfo methodInfo = pluginType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                return methodInfo;
            }
            else
            {
                // Handle the case where the method is not found
                throw new InvalidOperationException($"Method '{methodName}' not found in the plugin.");
            }
        }
        public object InvokeMethodWithParameters(string methodName, params object[] parameters)
        {
            MethodInfo methodInfo = GetMethod(methodName);

            if (methodInfo != null)
            {
                object result = methodInfo.Invoke(pluginInstance, parameters);
                return result;
            }
            else
            {
                // Handle the case where the method is not found
                throw new InvalidOperationException($"Method '{methodName}' not found in the plugin.");
            }
        }
    }
}

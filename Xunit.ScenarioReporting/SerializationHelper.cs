using System;
using System.Linq;
using System.Reflection;

namespace Xunit.ScenarioReporting
{
    static class SerializationHelper
    {
        public static Type GetType(string assemblyName, string typeName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
            if (assembly == null)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch { }
            }

            if (assembly == null)
                return null;

            return assembly.GetType(typeName);
        }
    }
}
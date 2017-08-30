using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting.Tests
{
    public static class Mocks
    {
        public static IAssemblyInfo AssemblyInfo(ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null, string assemblyFileName = null)
        {
            attributes = attributes ?? new IReflectionAttributeInfo[0];

            var result = new AssemblyInfoMock(types, attributes)
            {
                Name = assemblyFileName == null ? null : Path.GetFileNameWithoutExtension(assemblyFileName),
                AssemblyPath = assemblyFileName
            };
            return result;
        }

        public static IReflectionAttributeInfo CollectionAttribute(string collectionName)
        {
            var result = new ReflectionAttributeMock(new CollectionAttribute(collectionName), new object[]{collectionName});
            return result;
        }
        

        public static ITestAssembly TestAssembly(string assemblyFileName, string configFileName = null,
            ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null)
        {
            var assemblyInfo = AssemblyInfo(types, attributes, assemblyFileName);
            return new TestAssembly(assemblyInfo, configFileName);
        }

        public static IXunitTestCase TestCase(Type type, string methodName, string displayName = null, string skipReason = null, string uniqueID = null)
        {
            var testMethod = TestMethod(type, methodName);
            return new ScenarioReportingXunitTestCase(new NullMessageSink(), TestMethodDisplay.ClassAndMethod, testMethod);
        }
        
        public static TestMethod TestMethod(Type type, string methodName, ITestCollection collection = null)
        {
            var @class = TestClass(type, collection);
            var methodInfo = type.GetMethod(methodName);
            if (methodInfo == null)
                throw new Exception($"Unknown method: {type.FullName}.{methodName}");

            return new TestMethod(@class, Reflector.Wrap(methodInfo));
        }

        public static TestClass TestClass(Type type, ITestCollection collection = null)
        {
            if (collection == null)
                collection = TestCollection(type.Assembly);

            return new TestClass(collection, Reflector.Wrap(type));
        }


        public static TestCollection TestCollection(Assembly assembly = null, ITypeInfo definition = null, string displayName = null)
        {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            if (displayName == null)
                displayName = "Mock test collection for " + assembly.CodeBase;

            return new TestCollection(TestAssembly(assembly), definition, displayName);
        }

        public static TestAssembly TestAssembly(Assembly assembly = null, string configFileName = null)
        {
            return new TestAssembly(Reflector.Wrap(assembly ?? Assembly.GetExecutingAssembly()), configFileName);
        }


        public static IEnumerable<IXunitTestCase> TestCases(Type type, string method)
        {
            return new[]{TestCase(type, method)};
        }

        static IEnumerable<IAttributeInfo> LookupAttribute(string fullyQualifiedTypeName, IReadOnlyList<IReflectionAttributeInfo> attributes)
        {
            if (attributes == null)
                return Enumerable.Empty<IAttributeInfo>();

            var attributeType = GetType(fullyQualifiedTypeName);
            return attributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
        }

        private static Type GetType(string assemblyQualifiedAttributeTypeName)
        {
            var parts = assemblyQualifiedAttributeTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
            if (parts.Count == 0)
                return null;

            if (parts.Count == 1)
                return Type.GetType(parts[0]);

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == parts[1]);
            if (assembly == null)
                return null;

            return assembly.GetType(parts[0]);
        }

        class AssemblyInfoMock:IAssemblyInfo
        {
            private readonly IReadOnlyList<ITypeInfo> _types;
            private readonly IReadOnlyList<IReflectionAttributeInfo> _attributes;

            public AssemblyInfoMock(IReadOnlyList<ITypeInfo> types, IReadOnlyList<IReflectionAttributeInfo> attributes)
            {
                _types = types;
                _attributes = attributes;
            }
            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return LookupAttribute(assemblyQualifiedAttributeTypeName, _attributes);
            }

            public ITypeInfo GetType(string typeName)
            {
                return _types?.FirstOrDefault();
            }

            public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
            {
                return _types ?? new ITypeInfo[0];
            }

            public string AssemblyPath { get; set; }
            public string Name { get; set; }
        }
        
        class ReflectionAttributeMock : IReflectionAttributeInfo
        {
            private readonly object[] _constructorArguments;

            public ReflectionAttributeMock(Attribute attribute, object[] constructorArguments)
            {
                Attribute = attribute;
                _constructorArguments = constructorArguments;
            }
            public IEnumerable<object> GetConstructorArguments()
            {
                return _constructorArguments;
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return new IAttributeInfo[0];
            }

            public TValue GetNamedArgument<TValue>(string argumentName)
            {
                throw new NotImplementedException();
            }

            public Attribute Attribute { get; }
        }
    }
}
static class DictionaryExtensions
{
    public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        dictionary.GetOrAdd(key).Add(value);
    }

    public static bool Contains<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue> valueComparer)
    {
        List<TValue> values;

        if (!dictionary.TryGetValue(key, out values))
            return false;

        return values.Contains(value, valueComparer);
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
    {
        return dictionary.GetOrAdd<TKey, TValue>(key, () => new TValue());
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> newValue)
    {
        TValue result;

        if (!dictionary.TryGetValue(key, out result))
        {
            result = newValue();
            dictionary[key] = result;
        }

        return result;
    }
}

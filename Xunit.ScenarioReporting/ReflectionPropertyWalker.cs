using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting
{
    internal class ReflectionReader
    {
        private readonly Dictionary<Type, Func<string, object, ObjectPropertyDefinition>> _customPropertyReaders;
        
        readonly Func<Type, string, bool> _skipProperty;
        private readonly Func<Type, bool> _skipType;
        private readonly IReadOnlyList<MemberInfo> _hiddenByDefault;

        public ReflectionReader(
            Dictionary<Type, string> formatStrings,
            Dictionary<Type, Func<object, string>> formatters,
            IReadOnlyList<MemberInfo> hiddenByDefault, 
            Dictionary<Type, Func<string, object, ObjectPropertyDefinition>> customPropertyReaders,
            Func<Type, string, bool> skipProperty,
            Func<Type, bool> skipType)
        {
            _hiddenByDefault = hiddenByDefault;
            _skipProperty = skipProperty;
            _skipType = skipType;
            _customPropertyReaders = customPropertyReaders;
        }
        public ObjectPropertyDefinition Read(object value)
        {
            var pending = new Stack<ToRead>();
            List<object> visited = new List<object>();
            var topLevel = new List<ObjectPropertyDefinition>();
            pending.Push(new ToRead(value.GetType(), TypeName(value.GetType()), value, true, topLevel));

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!current.Type.IsValueType)
                {
                    if (visited.Contains(current.Value)) continue;
                    visited.Add(current.Value);
                }
                var currentProps = new List<ObjectPropertyDefinition>();
                if (CustomProperties(current.Type, current.Name, current.Value, current.Parent)) continue;
                current.Parent.Add(
                    new ObjectPropertyDefinition(
                        current.Type, 
                        current.Name, 
                        current.DisplayedByDefault,
                        GetValue(current), 
                        null, 
                        null,
                        currentProps));
                
                if (SkipType(current.Type)) continue;
                if (current.Value is null) continue;
                
                if (current.Value is IDictionary)
                {
                    var entries = new Dictionary<object, object>();

                    var dictionary = (IDictionary)current.Value;

                    var enumerator = dictionary.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        entries.Add(enumerator.Key, enumerator.Value);
                    }

                    foreach (var entry in entries.Reverse())
                    {
                        pending.Push(new ToRead(entry.Value?.GetType(), Format(entry.Key), entry.Value, current.DisplayedByDefault, currentProps));
                    }
                }
                else if (current.Value is IEnumerable)
                {
                    //Reverse the enumerable because otherwise they will be reversed by the stack
                    var objects = ((IEnumerable)current.Value).OfType<object>().Reverse().ToArray();
                    int index = objects.Length - 1;
                    foreach (var v in objects)
                    {
                        pending.Push(new ToRead(v.GetType(), $"[{index--}]", v, current.DisplayedByDefault, currentProps));
                    }
                }
                else
                {
                    
                    foreach (var f in current.Type.GetFields(BindingFlags.Public | BindingFlags.Instance).Reverse())
                    {
                        if (_skipProperty(current.Type, f.Name) || _skipProperty(f.DeclaringType, f.Name)) continue;
                        value = f.GetValue(current.Value);
                        if (pending.Any(x => ReferenceEquals(value, x.Value))) continue;
                        pending.Push(new ToRead(f.FieldType, f.Name, value, DisplayByDefault(f), currentProps));
                        if (pending.Count > 10000)
                        {
                            throw new Exception(
                                $"Type {current.Type} appears to be endlessly recursive or has more properties than can sensibly be compared, please specify a custom property reader or skip the properties for this type")
                            {
                                Data = {["PendingStack"] = pending.Select(x => x.Type).ToArray()}
                            };
                        }
                    }
                    foreach (var p in current.Type.GetProperties().Reverse())
                    {
                        if (_skipProperty(current.Type, p.Name) || _skipProperty(p.DeclaringType, p.Name)) continue;
                        value = p.GetValue(current.Value);
                        if (pending.Any(x => ReferenceEquals(value, x.Value))) continue;
                        pending.Push(new ToRead(p.PropertyType, p.Name, value, DisplayByDefault(p), currentProps));
                        if (pending.Count > 10000)
                        {
                            throw new Exception($"Type {current.Type} appears to be endlessly recursive or has more properties than can sensibly be compared, please specify a custom property reader or skip the properties for this type") { Data = { ["PendingStack"] = pending.Select(x => x.Type).ToArray() } };
                        }
                    }
                }
            }
            return topLevel[0];
        }

        private bool DisplayByDefault(MemberInfo memberInfo)
        {
            return !_hiddenByDefault.Any(x=> x.DeclaringType == memberInfo.DeclaringType && x.Name == memberInfo.Name);
        }

        private static object GetValue(ToRead current)
        {
            if (current.Value == null) return null;
            if (current.Type.IsGenericType && current.Type.IsValueType &&
                current.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Convert.ChangeType(current.Value, current.Type.GetGenericArguments()[0]);
            }
            return current.Value;
        }

        bool CustomProperties(Type type, string name, object instance, List<ObjectPropertyDefinition> definitions)
        {
            while (type != null && type != typeof(object))
            {
                if (_customPropertyReaders.TryGetValue(type, out var reader))
                {
                    var def = reader(name, instance);
                    definitions.Add(def);
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }
        string Format(object value)
        {
            if (value == null) return null;
            return value.ToString();
        }

        static string TypeName(Type t)
        {
            if (t.IsGenericType)
            {
                return string.Format(
                    "{0}<{1}>",
                    t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture)),
                    string.Join(", ", t.GetGenericArguments().Select(TypeName)));
            }

            return t.Name;
        }

        bool SkipType(Type type)
        {
            
            return 
                type.IsPrimitive || 
                type.IsEnum ||
                type == typeof(DateTime) || 
                type == typeof(string) || 
                type == typeof(DateTimeOffset) || 
                type == typeof(Guid) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) ||
                _skipType(type);
        }

        struct ToRead
        {
            public Type Type { get; }
            public string Name { get; }
            public object Value { get; }
            public bool DisplayedByDefault { get; }
            public List<ObjectPropertyDefinition> Parent { get; }

            public ToRead(Type type, string name, object value, bool displayedByDefault, List<ObjectPropertyDefinition> parent)
            {
                Type = type;
                Name = name;
                Value = value;
                DisplayedByDefault = displayedByDefault;
                Parent = parent;
            }
        }
    }

    public class ObjectPropertyDefinition
    {
        public ObjectPropertyDefinition(Type type, string name, bool displayBydefault, object value, string format, Func<object, string> formatter, IReadOnlyList<ObjectPropertyDefinition> properties)
        {
            Type = type;
            Name = name;
            DisplayBydefault = displayBydefault;
            Value = value;
            Format = format;
            Formatter = formatter;
            Properties = properties;
        }

        public Type Type { get; }
        public string Name { get; }
        public bool DisplayBydefault { get; }
        public object Value { get; }
        public string Format { get; }
        public Func<object, string> Formatter { get; }
        public IReadOnlyList<ObjectPropertyDefinition> Properties { get; }

    }

    internal class ReflectionComparerer
    {
        private readonly ReflectionReader _reader;
        private readonly IReadOnlyDictionary<Type, IEqualityComparer> _comparers;

        public ReflectionComparerer(ReflectionReader reader, IReadOnlyDictionary<Type, IEqualityComparer> comparers)
        {
            _reader = reader;
            _comparers = comparers;
        }
        public Then Compare(string scope, object expected, object actual)
        {
            var expectedStructure = _reader.Read(expected);
            var actualStructure = _reader.Read(actual);

            var expectedStack = new Stack<ExpectedReadResult>();
            var actualStack = new Stack<ObjectPropertyDefinition>();

            var details = new List<Detail>();


            expectedStack.Push(new ExpectedReadResult(details, expectedStructure));
            actualStack.Push(actualStructure);
            Compare(expectedStack, actualStack);
            if (details[0].Children.Count > 0)
                return new Then(scope, expectedStructure.Name, details.First().Children);
            //Compared a primitive type
            return new Then(scope, expectedStructure.Name, details);
        }

        void Compare(Stack<ExpectedReadResult> pendingExpected, Stack<ObjectPropertyDefinition> pendingActual)
        {

            while (pendingExpected.Count > 0)
            {
                var expectedComparison = pendingExpected.Pop();
                var actual = pendingActual.Pop();

                Compare(pendingExpected, pendingActual, expectedComparison, actual);
            }

        }

        private void Compare(Stack<ExpectedReadResult> pendingExpected, Stack<ObjectPropertyDefinition> pendingActual, ExpectedReadResult expectedComparison,
            ObjectPropertyDefinition actual)
        {
            var expected = expectedComparison.Value;
            var parent = expectedComparison.Parent;


            if (expected.Type != actual.Type)
            {
                parent.Add(new Mismatch(expected.Name, expected.Type, actual.Type));
            }
            else
            {
                if (expected.Properties.Count > 0)
                {
                    var currentDetails = new List<Detail>();
                    for (int i = expected.Properties.Count - 1; i >= 0; i--)
                    {
                        pendingExpected.Push(new ExpectedReadResult(currentDetails, expected.Properties[i]));
                        pendingActual.Push(actual.Properties[i]);
                    }
                    parent.Add(new Match(currentDetails, expected.DisplayBydefault, expected.Name));
                }
                else
                {
                    if (!_comparers.TryGetValue(expected.Type, out var comparer))
                        comparer = DefaultComparerFor(expected.Type);
                    if (comparer.Equals(expected.Value, actual.Value))
                        parent.Add(new Match(expected.Name, expected.Value, expected.DisplayBydefault, expected.Format, expected.Formatter));
                    else
                        parent.Add(new Mismatch(expected.Name, expected.Value, actual.Value, expected.Format,
                            expected.Formatter));
                }
            }
        }

        struct ExpectedReadResult
        {
            public ExpectedReadResult(List<Detail> parent, ObjectPropertyDefinition value)
            {
                Parent = parent;
                Value = value;
            }

            public List<Detail> Parent { get; }
            public ObjectPropertyDefinition Value { get; }
        }

        static readonly ConcurrentDictionary<Type, IEqualityComparer> DefaultComparers = new ConcurrentDictionary<Type, IEqualityComparer>();
        static IEqualityComparer DefaultComparerFor(Type type)
        {
            IEqualityComparer CreateComparer(Type t)
            {
                var eq = typeof(EqualityComparer<>).MakeGenericType(t);
                var @default = eq.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                return (IEqualityComparer)@default.GetValue(null, new object[] { });
            }

            return DefaultComparers.GetOrAdd(type, CreateComparer);
        }
    }
}

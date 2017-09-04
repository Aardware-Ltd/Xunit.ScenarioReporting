﻿using System;
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
        readonly Func<Type, string, bool> _skipProperty;
        private Func<Type, bool> _skipType;

        public ReflectionReader(
            Dictionary<Type, string> formatStrings,
            Dictionary<Type, Func<object, string>> formatters,
            Func<Type, string, bool> skipProperty,
            Func<Type, bool> skipType)
        {
            _skipProperty = skipProperty;
            _skipType = skipType;
        }
        public ReadResult Read(object value)
        {
            var pending = new Stack<ToRead>();
            List<object> visited = new List<object>();
            var topLevel = new List<ReadResult>();
            pending.Push(new ToRead(value.GetType(), TypeName(value.GetType()), value, topLevel));

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (visited.Contains(current.Value)) continue;
                visited.Add(current.Value);
                var currentProps = new List<ReadResult>();

                current.Parent.Add(new ReadResult(current.Type, current.Name, current.Value, null, null,
                    currentProps));
                //if (CustomProperties(currentProps)) continue;
                if (SkipType(current.Type)) continue;
                
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
                        pending.Push(new ToRead(entry.Value?.GetType(), Format(entry.Key), entry.Value, currentProps));
                    }
                }
                else if (current.Value is IEnumerable)
                {
                    //Reverse the enumerable because otherwise they will be reversed by the stack
                    var objects = ((IEnumerable)current.Value).OfType<object>().Reverse().ToArray();
                    int index = objects.Length - 1;
                    foreach (var v in objects)
                    {
                        pending.Push(new ToRead(v.GetType(), $"[{index--}]", v, currentProps));
                    }
                }
                else
                {
                    foreach (var p in current.Type.GetProperties().Reverse())
                    {
                        if(_skipProperty(current.Type, p.Name)) continue;
                        value = p.GetValue(current.Value);
                        if (pending.Any(x => ReferenceEquals(value, x.Value))) continue;
                        pending.Push(new ToRead(p.PropertyType, p.Name, value, currentProps));
                        if (pending.Count > 10000)
                        {
                            throw new Exception($"Type {current.Type} appears to be endlessly recursive or has more properties than can sensibly be compared, please specify a custom property reader or skip the properties for this type"){Data ={["PendingStack"] = pending.Select(x=>x.Type).ToArray()}};
                        }
                    }
                }
            }
            return topLevel[0];
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
            return type.IsPrimitive || type == typeof(DateTime) || type == typeof(string) /*|| type == typeof(DateTimeOffset)*/ || _skipType(type);
        }

        struct ToRead
        {
            public Type Type { get; }
            public string Name { get; }
            public object Value { get; }
            public List<ReadResult> Parent { get; }

            public ToRead(Type type, string name, object value, List<ReadResult> parent)
            {
                Type = type;
                Name = name;
                Value = value;
                Parent = parent;
            }
        }
    }

    internal class ReadResult
    {
        public ReadResult(Type type, string name, object value, string format, Func<object, string> formatter, IReadOnlyList<ReadResult> properties)
        {
            Type = type;
            Name = name;
            Value = value;
            Format = format;
            Formatter = formatter;
            Properties = properties;
        }

        public Type Type { get; }
        public string Name { get; }
        public object Value { get; }
        public string Format { get; }
        public Func<object, string> Formatter { get; }
        public IReadOnlyList<ReadResult> Properties { get; }

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
            var actualStack = new Stack<ReadResult>();

            var details = new List<Detail>();


            expectedStack.Push(new ExpectedReadResult(details, expectedStructure));
            actualStack.Push(actualStructure);
            Compare(expectedStack, actualStack);
            if (details[0].Children.Count > 0)
                return new Then(scope, expectedStructure.Name, details.First().Children);
            //Compared a primitive type
            return new Then(scope, expectedStructure.Name, details);
        }

        void Compare(Stack<ExpectedReadResult> pendingExpected, Stack<ReadResult> pendingActual)
        {

            while (pendingExpected.Count > 0)
            {
                var expectedComparison = pendingExpected.Pop();
                var actual = pendingActual.Pop();

                Compare(pendingExpected, pendingActual, expectedComparison, actual);
            }

        }

        private void Compare(Stack<ExpectedReadResult> pendingExpected, Stack<ReadResult> pendingActual, ExpectedReadResult expectedComparison,
            ReadResult actual)
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
                    parent.Add(new Match(currentDetails, expected.Name));
                }
                else
                {
                    if (!_comparers.TryGetValue(expected.Type, out var comparer))
                        comparer = DefaultComparerFor(expected.Type);
                    if (comparer.Equals(expected.Value, actual.Value))
                        parent.Add(new Match(expected.Name, expected.Value, expected.Format, expected.Formatter));
                    else
                        parent.Add(new Mismatch(expected.Name, expected.Value, actual.Value, expected.Format,
                            expected.Formatter));
                }
            }
        }

        struct ExpectedReadResult
        {
            public ExpectedReadResult(List<Detail> parent, ReadResult value)
            {
                Parent = parent;
                Value = value;
            }

            public List<Detail> Parent { get; }
            public ReadResult Value { get; }
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

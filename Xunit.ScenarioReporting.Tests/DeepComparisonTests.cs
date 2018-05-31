using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting.Tests
{
    public class DeepComparisonTests
    {
        private readonly ReflectionComparerer _comparer;

        public DeepComparisonTests()
        {
            var reader = new ReflectionReader(new Dictionary<Type, string>(), new Dictionary<Type, Func<object, string>>(), new List<MemberInfo>(), new Dictionary<Type, Func<string, bool, object, ObjectPropertyDefinition>>(), 
                (_, __) => false, _ => false);
            _comparer = new ReflectionComparerer(reader, new Dictionary<Type, object>());
        }

        [Fact()]
        public void CanCompareRawPrimitive()
        {
            var then = _comparer.Compare("scope", "test", "test");
            Assert.Equal("scope", then.Scope);
            Assert.Equal(nameof(String), then.Title);
            Assert.Equal(1, then.Details.Count);
            var details = Assert.IsType<Match>(then.Details[0]);
            Assert.Equal("String", details.Name);
        }

        [Fact]
        public void CanCompareSimpleTypes()
        {
            var simpleType = new SimpleType(42, "Meaning of life");
            var then = _comparer.Compare("scope", simpleType,
                simpleType);
            Assert.Equal("scope", then.Scope);
            Assert.Equal(nameof(SimpleType), then.Title);
            Assert.Equal(2, then.Details.Count);
            var detail0 = Assert.IsType<Match>(then.Details[0]);
            var detail1 = Assert.IsType<Match>(then.Details[1]);
            Assert.Equal("Number", detail0.Name);
            Assert.Equal("Text", detail1.Name);
        }

        [Fact]
        public void CanCompareComplexTypes()
        {
            var complexType = new ComplexType(new SimpleType(2, "Two"), new SimpleType(40, "Forty"));
            var then = _comparer.Compare("scope",
                complexType, complexType);
            Assert.Equal(2, then.Details.Count);
        }
        
        class SimpleType
        {
            public SimpleType(int number, string text)
            {
                Number = number;
                Text = text;
            }

            public int Number { get; }
            public string Text { get; }
        }

        class ComplexType
        {
            public SimpleType First { get; }
            public SimpleType Second { get; }

            public ComplexType(SimpleType first, SimpleType second)
            {
                First = first;
                Second = second;
            }
        }

        struct TestStruct
        {
            public static readonly TestStruct EmptyField = new TestStruct();

            public static TestStruct Empty { get; } = new TestStruct();
        }

        [Fact]
        public void CanCompareObjectWithStaticProperty()
        {
            // Ensure this does not hang on infinite recursion of field/property
            _comparer.Compare("scope", new TestStruct(), new TestStruct());
        }
    }
}

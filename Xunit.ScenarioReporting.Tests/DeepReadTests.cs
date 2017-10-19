using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Xunit.ScenarioReporting.Tests
{
    public class DeepReadTests
    {
        private readonly ReflectionReader _reader;

        public DeepReadTests()
        {
            _reader = new ReflectionReader(new Dictionary<Type, string>(),new Dictionary<Type, Func<object, string>>(), new List<MemberInfo>(), new Dictionary<Type, Func<string, object, ObjectPropertyDefinition>>(), (_, __) => false, _=>false);
        }
        class TestData
        {
            public static IEnumerable<object[]> Primitives()
            {
                yield return new object[] {"test", typeof(string), "String"};
                yield return new object[] {4.12m, typeof(decimal), "Decimal"};
                yield return new object[] {2.14d, typeof(Double), nameof(Double)};
                yield return new object[] { new DateTime(1999, 12, 31, 23, 59, 59), typeof(DateTime), nameof(DateTime) };
                yield return new object[] {(byte)0x2, typeof(Byte), nameof(Byte)};
            }
        }

        [Theory]
        [MemberData(nameof(TestData.Primitives), MemberType = typeof(TestData))]
        public void CanReadPrimitive(object value, Type type, string name)
        {
            var read = _reader.Read(value);
            Assert.Equal(type, read.Type);
            Assert.Equal(name, read.Name);
            Assert.IsType(type, read.Value);
            Assert.Equal(value, read.Value);
            Assert.Empty(read.Properties);
            Assert.Null(read.Format);
            Assert.Null(read.Formatter);
        }

        [Fact]
        public void CanReadObjectWithProperties()
        {
            var now = DateTime.Now;
            var value = new ClassWithProperties("test", 42, Math.PI, now);
            var read = _reader.Read(value);
            Assert.Equal(typeof(ClassWithProperties), read.Type);
            Assert.Equal(nameof(ClassWithProperties), read.Name);
            Assert.Equal(value, read.Value);
            Assert.Null(read.Format);
            Assert.Null(read.Formatter);

            Assert.NotEmpty(read.Properties);

            Assert.Collection(read.Properties,
                r =>
                {
                    Assert.Equal(nameof(ClassWithProperties.Name), r.Name);
                    Assert.Equal(typeof(String), r.Type);
                    Assert.Equal("test", r.Value);
                    Assert.Null(read.Format);
                    Assert.Null(read.Formatter);
                    Assert.Empty(r.Properties);
                },
                r =>
                {
                    Assert.Equal(nameof(ClassWithProperties.MeaningOfEverything), r.Name);
                    Assert.Equal(typeof(int), r.Type);
                    Assert.Equal(42, r.Value);
                    Assert.Null(read.Format);
                    Assert.Null(read.Formatter);
                    Assert.Empty(r.Properties);
                },
                r =>
                {
                    Assert.Equal(nameof(ClassWithProperties.Pi), r.Name);
                    Assert.Equal(typeof(double), r.Type);
                    Assert.Equal(Math.PI, Assert.IsType<double>(r.Value), 10);
                    Assert.Null(read.Format);
                    Assert.Null(read.Formatter);
                    Assert.Empty(r.Properties);
                },
                r =>
                {
                    Assert.Equal(nameof(ClassWithProperties.RunAt), r.Name);
                    Assert.Equal(typeof(DateTime), r.Type);
                    Assert.Equal(now, r.Value);
                    Assert.Null(read.Format);
                    Assert.Null(read.Formatter);
                    Assert.Empty(r.Properties);
                }
                );
        }

        [Fact]
        public void CanReadRecursiveObjects()
        {
            var first = new CanBeRecursive();
            var second = new CanBeRecursive();
            var third = new CanBeRecursive();
            first.Next = second;
            second.Next = third;
            third.Next = first;
            var read = _reader.Read(first);
            Assert.Equal(typeof(CanBeRecursive), read.Type);
            Assert.Equal(nameof(CanBeRecursive), read.Name);
            Assert.Equal(first, read.Value);
            Assert.NotEmpty(read.Properties);
            Assert.Null(read.Format);
            Assert.Null(read.Formatter);

        }

        [Fact]
        public void CanReadDictionary()
        {
            
            Dictionary<string, int> dictionary = new Dictionary<string, int>() { ["field1"] = 1, ["Field2"] = 3 };
            var read = _reader.Read(dictionary);
            Assert.Equal($"{nameof(Dictionary<string, int>)}<{nameof(String)}, {nameof(Int32)}>", read.Name);

            Assert.NotEmpty(read.Properties);
            Assert.Collection(read.Properties,
                r => AssertElement(r, "field1", 1),
                r => AssertElement(r, "Field2", 3)
            );

            void AssertElement(ObjectPropertyDefinition r, string field, int value)
            {
                Assert.Equal(typeof(int), r.Type);
                Assert.Equal(field, r.Name);
                Assert.Equal(value, r.Value);
                Assert.Empty(r.Properties);
            }
        }

        [Fact]
        public void CanReadArray()
        {
            var value = new[] {1, 2, 3};
            var read = _reader.Read(value);
            Assert.Equal(typeof(Int32[]).Name, read.Name);
            Assert.Equal(typeof(int[]), read.Type);
            Assert.NotEmpty(read.Properties);
            Assert.Collection(read.Properties,
                r => AssertElement(r,1, 0),
                r => AssertElement(r,2, 1),
                r => AssertElement(r,3, 2)
                );
            Assert.Null(read.Format);
            Assert.Null(read.Formatter);

            void AssertElement(ObjectPropertyDefinition r, int v, int index)
            {
                Assert.Equal(typeof(int), r.Type);
                Assert.Equal($"[{index}]", r.Name);
                Assert.Equal(v, r.Value);
                Assert.Empty(r.Properties);
            }
        }

        [Fact]
        public void CanReadCollection()
        {
            var value = new Collection<int> { 1, 2, 3 };
            var read = _reader.Read(value);
            Assert.Equal($"{nameof(Collection<int>)}<{nameof(Int32)}>", read.Name);
            Assert.Equal(typeof(Collection<int>), read.Type);
            Assert.NotEmpty(read.Properties);
            Assert.Collection(read.Properties,
                r => AssertElement(r, 0),
                r => AssertElement(r, 1),
                r => AssertElement(r, 2)
            );
            Assert.Null(read.Format);
            Assert.Null(read.Formatter);

            void AssertElement(ObjectPropertyDefinition r, int index)
            {
                Assert.Equal(typeof(int), r.Type);
                Assert.Equal($"[{index}]", r.Name);
                Assert.Empty(r.Properties);
            }
        }

        [Fact]
        public void CanReadObjectWithFields()
        {
            var withFields = new ClassWithFields("Private", "Protected", "PublicReadonly", "Public");
            var read = _reader.Read(withFields);
            Assert.Equal(2, read.Properties.Count);
        }

        [Fact]
        public void ShouldNotSkipValueTypesWithSameValue()
        {
            var read = _reader.Read(new ClassWithTwoGuids(Guid.NewGuid()));
            Assert.Equal(2, read.Properties.Count);
        }

        [Fact]
        public void ShouldHandleNullFieldsCorrectly()
        {
            var value = new CanBeRecursive();
            var read = _reader.Read(value);
            Assert.Collection(read.Properties,
                p =>
                {
                    Assert.Equal(typeof(CanBeRecursive), p.Type);
                    Assert.Equal(nameof(CanBeRecursive.Next), p.Name);
                    Assert.Null(p.Value);
                }
                );
        }

        [Fact]
        public void CanHandleEnums()
        {
            var value = new ClassWithEnum(ConsoleColor.DarkCyan);
            var read = _reader.Read(value);
            Assert.Equal(1, read.Properties.Count);
            Assert.Equal(0, read.Properties[0].Properties.Count);
        }

        [Fact]
        public void CanHandleNullableTypesWithNullValues()
        {
            var value = new ClassWithNullableType(null);
            var read = _reader.Read(value);
            Assert.Equal(1, read.Properties.Count);
            Assert.Equal(0, read.Properties[0].Properties.Count);
        }

        [Fact]
        public void CanHandleNullableTypesWithNonNullValues()
        {
            var value = new ClassWithNullableType(Guid.NewGuid());
            var read = _reader.Read(value);
            Assert.Equal(1, read.Properties.Count);
            Assert.Equal(0, read.Properties[0].Properties.Count);
        }

        class ClassWithTwoGuids
        {
            public readonly Guid Value1;

            public ClassWithTwoGuids(Guid value)
            {
                Value1 = value;
                Value2 = value;
            }

            public Guid Value2 { get; }
            
        }

        class ClassWithFields
        {
            private readonly string _shouldNotBeRead;
            protected readonly string AlsoShoulNotBeRead;
            public readonly string CanBeRead;
            public string ShouldAlsoBeRead;

            public ClassWithFields(string shouldNotBeRead, string alsoShoulNotBeRead, string alsoShoulNotBeRead1, string shouldAlsoBeRead)
            {
                _shouldNotBeRead = shouldNotBeRead;
                AlsoShoulNotBeRead = alsoShoulNotBeRead;
                AlsoShoulNotBeRead = alsoShoulNotBeRead1;
                ShouldAlsoBeRead = shouldAlsoBeRead;
            }
        }

        class ClassWithNullableType
        {
            public Guid? Guid { get; }

            public ClassWithNullableType(Guid? guid)
            {
                Guid = guid;
            }
        }

        class ClassWithEnum
        {
            public ConsoleColor Color { get; }

            public ClassWithEnum(ConsoleColor color)
            {
                Color = color;
            }
        }
        class ClassWithProperties
        {
            public ClassWithProperties(string name, int meaningOfEverything, double pi, DateTime runAt)
            {
                Name = name;
                MeaningOfEverything = meaningOfEverything;
                Pi = pi;
                RunAt = runAt;
            }

            public string Name { get; }
            public int MeaningOfEverything { get; }

            public double Pi { get; }
            public DateTime RunAt { get; }
        }

        class CanBeRecursive
        {
            public CanBeRecursive Next { get; set; }
        }
    }

    

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class ControllingPropertyReporting
    {
        [Fact]
        public Task ShouldAlwaysDisplayWhenFailing()
        {
            var expected = new DtoInheritingFromClassWithHiddenFields(Guid.NewGuid(), "Should be hidden", new ChildClass("Also should be hidden", "with a hidden value"), "Should be visible");
            return new Runner(expected).Run(def => def
                .Given(new DtoWithHiddenFields(Guid.NewGuid(), "hidden string", new ChildClass("hidden name", "hidden value"), "should be visible"))
                .When(new DtoWithoutHiddenFields("Should be visible", new ChildClass("Should also be visible", "and this should be visible too")))
                .Then(new DtoInheritingFromClassWithHiddenFields(Guid.NewGuid(), "Should be hidden", new ChildClass("different so should be visible", "also different so should be visible"), "Should be visible"))
            );

        }

        [Fact]
        public Task ShouldHideFieldsWhenPassing()
        {
            var expected = new DtoInheritingFromClassWithHiddenFields(Guid.NewGuid(), "Should be hidden", new ChildClass("Also should be hidden", "with a hidden value"), "Should be visible");
            return new Runner(expected).Run(def => def
                .Given(new DtoWithHiddenFields(Guid.NewGuid(), "hidden string", new ChildClass("hidden name", "hidden value"), "should be visible"))
                .When(new DtoWithoutHiddenFields("Should be visible", new ChildClass("Should also be visible", "and this should be visible too")))
                .Then(expected)
                );
        }
        class Runner : ReflectionBasedScenarioRunner<object, object, object>
        {
            private readonly object _then;

            public Runner(object then)
            {
                _then = then;
                this.Configure(cfg => cfg
                    .ForType<DtoWithHiddenFields>(t => t
                        .Hide(x => x.Id)
                        .Hide(x => x.HiddenChild)
                        .Hide(x => x.HiddenString)
                        )
                    .ForType< WithTechnicalInfo>(t => t
                        .Hide(x=>x.Id)
                        .Hide(x=>x.HiddenChild)
                        .Hide(x=> x.HiddenString)
                    ));
            }
            protected override Task Given(IReadOnlyList<object> givens)
            {
                return Task.CompletedTask;
            }

            protected override Task When(object when)
            {
                return Task.CompletedTask;
            }

            protected override Task<IReadOnlyList<object>> ActualResults()
            {
                return Task.FromResult((IReadOnlyList<object>)new[]{_then});
            }
        }

        class DtoWithoutHiddenFields
        {
            public DtoWithoutHiddenFields(string canSeeThis, ChildClass visible)
            {
                CanSeeThis = canSeeThis;
                Visible = visible;
            }

            public string CanSeeThis { get; }
            public ChildClass Visible { get; }
        }
        class DtoWithHiddenFields
        {
            public readonly Guid Id;

            public DtoWithHiddenFields(Guid id, string hiddenString, ChildClass hiddenChild, string shouldBeVisibleByDefault)
            {
                Id = id;
                HiddenString = hiddenString;
                HiddenChild = hiddenChild;
                ShouldBeVisibleByDefault = shouldBeVisibleByDefault;
            }

            public string HiddenString { get; }
            public ChildClass HiddenChild { get; }
            public string ShouldBeVisibleByDefault { get; }
        }

        class DtoInheritingFromClassWithHiddenFields : WithTechnicalInfo
        {
            public string ShouldBeVisibleByDefault { get; }

            public DtoInheritingFromClassWithHiddenFields(Guid id, string hiddenString, ChildClass hiddenChild, string shouldBeVisibleByDefault) : base(id, hiddenString, hiddenChild)
            {
                ShouldBeVisibleByDefault = shouldBeVisibleByDefault;
            }
        }

        class ChildClass
        {
            public ChildClass(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }

        abstract class WithTechnicalInfo
        {
            public readonly Guid Id;

            protected WithTechnicalInfo(Guid id, string hiddenString, ChildClass hiddenChild)
            {
                Id = id;
                HiddenString = hiddenString;
                HiddenChild = hiddenChild;
            }

            public string HiddenString { get; }

            public ChildClass HiddenChild { get; }
        }
    }

    
}

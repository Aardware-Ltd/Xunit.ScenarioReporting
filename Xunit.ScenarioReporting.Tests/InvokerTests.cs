using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.ScenarioReporting.Results;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting.Tests
{
    public class FunctionsThatReturnScenarioRunResults
    {
        [Fact]
        public async Task UnnamedScenarioFromFactShouldHaveNameSet()
        {
            var output = new VerifiableReportWriter();
            var report = new ScenarioReport("Test", output, new TestMessageSink());
            await Mock.ExecuteTestMethodAsync<FunctionsThatReturnScenarioRunResults>(report, nameof(BasicFactScenario));
            var scenario = Assert.Single(output.Items.OfType<StartScenario>());
            Assert.Contains(nameof(BasicFactScenario), scenario.Name);

            Assert.All(output.Items.OfType<Then>(), then => Assert.Contains(nameof(BasicFactScenario), then.Scope));
        }

        [Fact]
        public async Task UnnamedScenarioFromTheoryShouldHaveNameSet()
        {
            var output = new VerifiableReportWriter();
            var report = new ScenarioReport("Test", output, new TestMessageSink());
            await Mock.ExecuteTestMethodAsync<FunctionsThatReturnScenarioRunResults>(report, nameof(BasicTheoryScenario), parameters: new object[]{12, 30, 42});
            var scenario = Assert.Single(output.Items.OfType<StartScenario>());
            Assert.Contains(nameof(BasicTheoryScenario), scenario.Name);
            Assert.Contains("a: 12, b: 30, result: 42", scenario.Name );
        }

        [Fact]
        public async Task ReturningResultWithHigherLevelRunnerShouldError()
        {
            var output = new VerifiableReportWriter();
            var report = new ScenarioReport("Test", output, new TestMessageSink());
            var scenarioRunner = new TestScenarioRunner();
            var ex = await Record.ExceptionAsync(() => Mock.ExecuteTestMethodAsync<FunctionsThatReturnScenarioRunResults>(report, nameof(BasicFactScenario), scenarioRunner));
            var result = Assert.IsType<InvalidOperationException>(ex);
            Assert.Contains(Constants.Errors.DontReturnScenarioResults, result.Message);
        }

        [Fact]
        public async Task WhenScenarioHasTitleTheScopeShouldNotOverrideIt()
        {
            var output = new VerifiableReportWriter();
            var report = new ScenarioReport("Test", output, new TestMessageSink());
            await Mock.ExecuteTestMethodAsync<FunctionsThatReturnScenarioRunResults>(report, nameof(FactScenarioWithTitle));
            var scenario = Assert.Single(output.Items.OfType<StartScenario>());
            Assert.Contains(nameof(FactScenarioWithTitle), scenario.Scope);
            Assert.Equal("Custom Title", scenario.Name);

        }
        public Task<ScenarioRunResult> BasicFactScenario()
        {
            return new TestScenarioRunner()
                .Run(def => def.Given(new TestGiven()).When(new TestWhen()).Then(new TestThen()));

        }

        public Task<ScenarioRunResult> BasicTheoryScenario(int a, int b, int result)
        {
            return new TestScenarioRunner()
                .Run(def => def.Given(new TestGiven()).When(new TestWhen()).Then(new TestThen()));
        }

        public Task<ScenarioRunResult> FactScenarioWithTitle()
        {
            return new TestScenarioRunner()
                .Run(def => def.Given(new TestGiven()).When(new TestWhen()).Then(new TestThen()), "Custom Title");
        }

        class TestScenarioRunner : ReflectionBasedScenarioRunner<object, object, object>
        {
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
                return Task.FromResult(Definition.Then);
            }
        }

        class TestGiven
        {
            public string Value => "1";
        }

        class TestWhen
        {
            public string Value => "2";
        }

        class TestThen
        {
            public string Value => "3";
        }

        static class Mock
        {

            public static async Task ExecuteTestMethodAsync<T>(ScenarioReport report,
                string methodName, ScenarioRunner runner = null, object[] parameters = null)
            {
                parameters = parameters ?? new object[] { };
                var methodInfo = typeof(T).GetMethod(methodName);
                if(methodInfo == null) throw new InvalidOperationException($"Unknown method {methodName} on type {typeof(T)}.FullName");
                var method = new FakeMethodInfo(Reflector.Wrap(methodInfo),
                    Reflector.Wrap(typeof(T)), 
                    new []{new FakeAttributeInfo(
                    parameters.Length == 0? new FactAttribute() :new TheoryAttribute(), 
                    new object[]{},
                    new Dictionary<string, object>(){["DisplayName"] = null, ["Skip"] = null} )});
                var aggregator = new ExceptionAggregator();
                var testMethod = Mock.TestMethod<T>(method);
                var invoker = new ScenarioReportingTestInvoker(
                    scenarioRunner: runner,
                    report: report,
                    test: new XunitTest(new ScenarioReportingXunitTestCase(new NullMessageSink(), TestMethodDisplay.ClassAndMethod, testMethod, parameters), "Test"),
                    messageBus: new MessageBus(new NullMessageSink()),
                    constructorArguments: new object[] { },
                    testClass: typeof(T),
                    testMethod: method.ToRuntimeMethod(),
                    testMethodArguments: parameters,
                    beforeAfterAttributes: new BeforeAfterTestAttribute[] { },
                    aggregator: aggregator,
                    cancellationTokenSource: new CancellationTokenSource(1000)
                );
                await invoker.RunAsync();
                if (aggregator.HasExceptions)
                    throw aggregator.ToException();
                await report.WriteFinalAsync();
            }
            //public static async Task ExecuteScenarioFactMethodAsync<T>(ScenarioReport report, string methodName)
            //{
            //    var methodInfo = typeof(T).GetMethod(methodName);
            //    if (methodInfo == null) throw new InvalidOperationException($"Unknown method {methodName} on type {typeof(T)}.FullName");
            //    var method = new FakeMethodInfo(Reflector.Wrap(methodInfo),
            //        Reflector.Wrap(typeof(T)),
            //        new[]{new FakeAttributeInfo(
            //            new FactAttribute(),
            //            new object[]{},
            //            new Dictionary<string, object>(){["DisplayName"] = null, ["Skip"] = null} )}); var aggregator = new ExceptionAggregator();
            //    var testMethod = Mock.TestMethod<T>(method);
            //    var invoker = new ScenarioReportingTestInvoker(
            //        scenarioRunner: null,
            //        report: report,
            //        test: new XunitTest(new ScenarioReportingXunitTestCase(new NullMessageSink(), TestMethodDisplay.ClassAndMethod, testMethod), "Test"),
            //        messageBus: new MessageBus(new NullMessageSink()),
            //        constructorArguments: new object[] { },
            //        testClass: typeof(T),
            //        testMethod: method.ToRuntimeMethod(),
            //        testMethodArguments: null,
            //        beforeAfterAttributes: new BeforeAfterTestAttribute[] { },
            //        aggregator: aggregator,
            //        cancellationTokenSource: new CancellationTokenSource(1000)
            //    );
            //    await invoker.RunAsync();
            //    if (aggregator.HasExceptions)
            //        throw aggregator.ToException();
            //    await report.WriteFinalAsync();

            //}
            
            public static IMethodInfo MethodInfo(Func<Task<ScenarioRunResult>> action, params IReflectionAttributeInfo[] attributes)
            {
                return new FakeMethodInfo(Reflector.Wrap(action.Method), Reflector.Wrap(action.Method.DeclaringType), attributes);
            }
            public static TestMethod TestMethod<T>(IMethodInfo method)
            {
                return new TestMethod(Mock.TestClass<T>(), method);
            }
            public static ITestAssembly Assembly<T>()
            {
                return new TestAssembly(Reflector.Wrap(typeof(T).GetType().Assembly));
            }

            public static ITestClass TestClass<T>()
            {
                var collection = new TestCollection(Assembly<T>(), null, "Default test collection");
                return new TestClass(collection, Reflector.Wrap(typeof(T)));
            }

            public static IReflectionAttributeInfo Fact(string displayName = null, string skip = null)
            {
                var namedArguments = new Dictionary<string, object>()
                {
                    ["DisplayName"] = displayName,
                    ["Skip"] = skip
                };
                return new FakeAttributeInfo(new FactAttribute { DisplayName = displayName, Skip = skip }, new object[] { displayName, skip }, namedArguments);
            }

            class FakeAttributeInfo : IReflectionAttributeInfo
            {
                public Attribute Attribute { get; }
                private readonly object[] _arguments;
                private readonly Dictionary<string, object> _namedArguments;

                public FakeAttributeInfo(Attribute attribute, object[] arguments, Dictionary<string, object> namedArguments)
                {
                    Attribute = attribute;
                    _arguments = arguments;
                    _namedArguments = namedArguments;
                }

                public IEnumerable<object> GetConstructorArguments()
                {
                    return _arguments;
                }

                public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
                {
                    return Enumerable.Empty<IAttributeInfo>();
                }

                public TValue GetNamedArgument<TValue>(string argumentName)
                {
                    object value;
                    if (_namedArguments.TryGetValue(argumentName, out value) && value is TValue)
                        return (TValue)value;
                    return default(TValue);
                }
            }
            class FakeMethodInfo : IMethodInfo
            {
                private readonly IMethodInfo _inner;
                private readonly ITypeInfo _typeInfo;
                private readonly IEnumerable<IReflectionAttributeInfo> _attributes;

                public FakeMethodInfo(IMethodInfo inner, ITypeInfo typeInfo, IEnumerable<IReflectionAttributeInfo> attributes)
                {
                    _inner = inner;
                    _typeInfo = typeInfo;
                    _attributes = attributes;
                }

                public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
                {
                    if (_attributes == null)
                        return _inner.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
                    var type = System.Type.GetType(assemblyQualifiedAttributeTypeName, true);
                    return _attributes.Where(x => type.GetTypeInfo().IsAssignableFrom(x.Attribute.GetType().GetTypeInfo()));
                }

                public IEnumerable<ITypeInfo> GetGenericArguments()
                {
                    return _inner.GetGenericArguments();
                }

                public IEnumerable<IParameterInfo> GetParameters()
                {
                    return _inner.GetParameters();
                }

                public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
                {
                    return _inner.MakeGenericMethod(typeArguments);
                }

                public bool IsAbstract
                {
                    get { return _inner.IsAbstract; }
                }

                public bool IsGenericMethodDefinition
                {
                    get { return _inner.IsGenericMethodDefinition; }
                }

                public bool IsPublic
                {
                    get { return _inner.IsPublic; }
                }

                public bool IsStatic
                {
                    get { return _inner.IsStatic; }
                }

                public string Name
                {
                    get { return _inner.Name; }
                }

                public ITypeInfo ReturnType
                {
                    get { return _inner.ReturnType; }
                }

                public ITypeInfo Type
                {
                    get { return _typeInfo; }
                }
            }
        }

        internal class VerifiableReportWriter : IReportWriter
        {
            public List<ReportItem> Items { get; } = new List<ReportItem>();

            public Task Write(ReportItem item)
            {
                Items.Add(item);
                return Task.CompletedTask;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestCollectionRunner : XunitTestCollectionRunner
    {
        private readonly ScenarioReport _report;
        readonly IMessageSink _diagnosticMessageSink;
        private Scenario _scenario;

        public ScenarioReportingXunitTestCollectionRunner(ScenarioReport report,
                                                            ITestCollection testCollection,
                                                            IEnumerable<IXunitTestCase> testCases,
                                                            IMessageSink diagnosticMessageSink,
                                                            IMessageBus messageBus,
                                                            ITestCaseOrderer testCaseOrderer,
                                                            ExceptionAggregator aggregator,
                                                            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            _report = report;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        //protected override void CreateCollectionFixture(Type fixtureType)
        //{
        //    if (CollectionFixtureMappings.ContainsKey(fixtureType)) return;
        //    if (fixtureType.IsSubclassOf(typeof(Definition)))
        //    {
        //        Aggregator.Run(() =>
        //        {
        //            var ctors = fixtureType.GetConstructors();
        //            if (ctors.Length > 1) throw new InvalidOperationException($"Definition type {fixtureType.FullName} can only have 1 public constructor");
        //            if (ctors.Length == 0) throw new InvalidOperationException($"Definition type {fixtureType.FullName} must have a public constructor");

        //            var ctor = ctors[0];

        //            var missingParameters = new List<ParameterInfo>();
        //            var ctorArgs = ctor.GetParameters().Select(p =>
        //            {
        //                object arg;
        //                if (!CollectionFixtureMappings.TryGetValue(p.ParameterType, out arg))
        //                    missingParameters.Add(p);
        //                return arg;
        //            }).ToArray();

        //            if (missingParameters.Count > 0)
        //                Aggregator.Add(new TestClassException(
        //                    $"Definition type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"
        //                ));
        //            else
        //            {
        //                CollectionFixtureMappings[fixtureType] = _scenario = (Definition)ctor.Invoke(ctorArgs);
        //                _scenario.Title = TestCollection.CollectionDefinition.Name;
        //            }
        //        });
        //    }
        //    else
        //    {
        //        object value;
        //        if (!CollectionFixtureMappings.TryGetValue(fixtureType, out value))
        //        {
        //            Aggregator.Run(() => CollectionFixtureMappings[fixtureType] = Activator.CreateInstance(fixtureType));
        //        }
        //    }
        //}
        protected override async Task AfterTestCollectionStartingAsync()
        {
            await base.AfterTestCollectionStartingAsync();
            Aggregator.Run(() =>
            {
                _scenario = CollectionFixtureMappings.Values.OfType<Scenario>().SingleOrDefault();
                if (_scenario != null)
                    _scenario.Title = TestCollection.CollectionDefinition.Name;
            });
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            return new ScenarioXunitTestClassRunner(_report, testClass, @class, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
        }

        protected override async Task BeforeTestCollectionFinishedAsync()
        {
            await base.BeforeTestCollectionFinishedAsync();
            if (_scenario != null)
            {
                _report.Report(_scenario);
            }
        }
    }
}

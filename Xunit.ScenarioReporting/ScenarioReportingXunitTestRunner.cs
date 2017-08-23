using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestRunner : XunitTestRunner
    {
        private readonly Scenario _scenario;
        private readonly ScenarioReport _report;

        public ScenarioReportingXunitTestRunner(Scenario scenario, ScenarioReport report, ITest test, IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource) : base(test, messageBus, testClass, constructorArguments,
            testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _scenario = scenario;
            _report = report;
        }

        protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        {
            return new ScenarioReportingTestInvoker(_scenario, _report, Test, MessageBus, TestClass, ConstructorArguments,
                TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator,
                CancellationTokenSource).RunAsync();
        }
    }
}
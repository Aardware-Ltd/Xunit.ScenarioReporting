using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingTestInvoker : XunitTestInvoker
    {
        private readonly Scenario _scenario;
        private readonly ScenarioReport _report;

        public ScenarioReportingTestInvoker(Scenario scenario, ScenarioReport report, ITest test, IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource) : base(test, messageBus, testClass, constructorArguments,
            testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _scenario = scenario;
            _report = report;
        }

        protected override object CallTestMethod(object testClassInstance)
        {
            try
            {
                var result = base.CallTestMethod(testClassInstance);
                //TODO: the base class has support for f# types async results, do we need to unwrap this?
                if (result is Task && result.GetType().IsConstructedGenericType)
                {
                    var resultType = result.GetType();
                    var returnType = resultType.GetGenericArguments()[0];
                    if (returnType.IsSubclassOf(typeof(Scenario)))
                    {
                        //TODO: collate for scenario report
                    }
                }
                else if (result is Scenario)
                {
                    var scenario = (Scenario)result;
                    if (scenario.Title == null) scenario.Title = DisplayName;
                    return VerifyAndReportScenario(scenario);
                }
                if (_scenario != null)
                {
                    _scenario.AddResult(TestCase.Method.Name);
                }
                return result;
            }
            catch (Exception e) when (!(e is ScenarioVerificationException))
            {

                if (_scenario != null)
                {
                    _scenario.AddResult(TestCase.Method.Name, e.Unwrap());
                }
                throw;
            }
        }

        private async Task VerifyAndReportScenario(Scenario scenario)
        {
            await scenario.SafeVerify();
            _report.Report(scenario);

        }
    }
}
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
            object result = null;
            try
            {
                result = base.CallTestMethod(testClassInstance);
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
                    return VerifyAndReportScenario(scenario, TestCase.Method.Name);
                }
                if (_scenario != null)
                {
                    _scenario.AddResult(TestCase.Method.Name);
                }
                return result;
            }
            catch (Exception e)
            {

                if (_scenario != null)
                {
                    _scenario.AddResult(TestCase.Method.Name, e.Unwrap());
                }
                Aggregator.Add(e);
                return result;
            }
        }

        private async Task VerifyAndReportScenario(Scenario scenario, string name)
        {
            try
            {
                await scenario.SafeVerify(name);
            }
            finally
            {
                _report.Report(scenario);
            }

        }
    }
}
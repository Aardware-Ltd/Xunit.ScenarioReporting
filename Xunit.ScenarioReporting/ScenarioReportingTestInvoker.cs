using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
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
        private MethodInfo _openVerify;

        public ScenarioReportingTestInvoker(Scenario scenario, ScenarioReport report, ITest test,
            IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource) : base(test, messageBus, testClass, constructorArguments,
            testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _scenario = scenario;
            _report = report;
            _openVerify = this.GetType().GetMethod(nameof(VerifyAndReport));
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
                        //Convert to scenario task
                        var closedVerify = _openVerify.MakeGenericMethod(returnType);
                        return closedVerify.Invoke(this, new[] {result, DisplayName, TestCase.Method.Name});
                    }
                }
                else if (result is Scenario)
                {
                    var scenario = (Scenario) result;
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

                if (_scenario != null && !(e is ScenarioVerificationException))
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

        private async Task VerifyAndReport<T>(Task<T> scenarioTask, string name) where T : Scenario
        {
            Scenario scenario = await scenarioTask;
            await VerifyAndReportScenario(scenario, name);
        }
    }
}
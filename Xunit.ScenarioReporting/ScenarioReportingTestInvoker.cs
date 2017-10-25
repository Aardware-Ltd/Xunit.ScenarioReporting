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
        private readonly ScenarioRunner _scenarioRunner;
        private readonly ScenarioReport _report;
        private readonly MethodInfo _openVerify;

        public ScenarioReportingTestInvoker(ScenarioRunner scenarioRunner, ScenarioReport report, ITest test,
            IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource) : base(test, messageBus, testClass, constructorArguments,
            testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _scenarioRunner = scenarioRunner;
            _report = report;
            _openVerify = GetType().GetMethod(nameof(VerifyAndReport), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        protected override object CallTestMethod(object testClassInstance)
        {
            object result = null;
            var testMethodName = ((ScenarioReportingXunitTestCase)TestCase).TestMethodName;
            try
            {
                using (ScenarioScope.Push(TestCase.DisplayName))
                using (ScenarioGroup.Push(TestClass.FullName))
                using (ReportContext.Set(_report))
                {
                    result = base.CallTestMethod(testClassInstance);
                }
                //TODO: the base class has support for f# types async results, do we need to unwrap this?
                if (result is Task && result.GetType().IsConstructedGenericType)
                {
                    var resultType = result.GetType();
                    var returnType = resultType.GetGenericArguments()[0];
                    if (typeof(ScenarioRunResult).IsAssignableFrom(returnType))
                    {
                        //Convert to scenarioRunner task
                        var closedVerify = _openVerify.MakeGenericMethod(returnType);
                        return closedVerify.Invoke(this, new[] { result, TestCase.DisplayName });
                    }
                }
                else if (result is ScenarioRunResult)
                {
                    var scenario = (ScenarioRunResult)result;
                    VerifyAndReportScenario(scenario, TestCase.DisplayName, TestClass.FullName);
                }
                if (_scenarioRunner != null)
                {
                    _scenarioRunner.AddResult(testMethodName, TestCase.DisplayName);
                }
                return result;
            }
            catch (Exception e)
            {
                if (_scenarioRunner != null && !(e is ScenarioVerificationException))
                {
                    _scenarioRunner.AddResult(testMethodName, TestCase.DisplayName, e.Unwrap());
                }
                Aggregator.Add(e.Unwrap());
                return result;
            }
        }

        private void VerifyAndReportScenario(ScenarioRunResult result, string name, string grouping)
        {
            if(_scenarioRunner!= null)
                throw new InvalidOperationException(Constants.Errors.DontReturnScenarioResults);
            result.Scope = name;
            result.Grouping = result.Grouping ?? grouping;
            _report.Report(result);
            result.ThrowIfErrored();
        }

        private async Task VerifyAndReport<T>(Task<T> scenarioTask, string name, string grouping) where T : ScenarioRunResult
        {
            ScenarioRunResult result = await scenarioTask;
            VerifyAndReportScenario(result, name, grouping);
        }
    }
}
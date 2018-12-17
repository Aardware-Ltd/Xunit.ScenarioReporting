using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{

    class StepExecutionStrategy<TStep>
    {
        private IReadOnlyList<StageDefinition> _stages;
        private Func<TStep, Task> _executeStep;
        public async Task Execute()
        {
            bool errored = false;
            foreach (var stage in _stages)
            {
                reporter.StartSection(stage.Name);

                foreach (var step in stage.Steps)
                {
                    if (errored)
                    {
                        reporter.ReportNotRun(step);
                        continue;

                    }

                    try
                    {
                        await _executeStep(step);
                        reporter.ReportSuccess(step);
                    }
                    catch (Exception ex)
                    {
                        reporter.ReportError(step);
                        errored = true;
                    }
                }
            }
        }
        struct StageDefinition
        {
            public string Name { get; }
            public IReadOnlyList<TStep> Steps { get; }

            public StageDefinition(string name, IReadOnlyList<TStep> steps)
            {
                Name = name;
                Steps = steps;
            }
        }
    }

    class GivenWhenThenStrategy<TGiven, TWhen, TThen>
    {
        private IReadOnlyList<TGiven> _givens;
        private TWhen _when;
        private IReadOnlyList<TThen> _thens;
        private Func<TGiven, Task> _executeGiven;
        private Func<TWhen, Task> _executeWhen;
        private Func<Task<TThen>> _readResults;

        public async Task Execute()
        {
            bool errored = false;
            Exception actualException = null;
            reporter.StartSection("Given");
            foreach (var given in _givens)
            {

                if (errored)
                {
                    reporter.ReportNotRun(given);
                }
                else
                {
                    try
                    {
                        await _executeGiven(given);
                        reporter.ReportSuccess(given);
                    }
                    catch (Exception ex)
                    {
                        reporter.ReportError(given, ex);
                        errored = true;
                    }
                }
            }

            reporter.StartSection("When");
            if (errored)
            {
                reporter.ReportNotRun(_when);
            }
            else
            {
                try
                {
                    await _executeWhen(_when);
                    reporter.ReportSuccess(_when);
                }
                catch (Exception ex) when (_definition.ExpectedException != null)
                {
                    actualException = ex;
                }
                catch (Exception ex) when (_definition.ExpectedException == null)
                {
                    reporter.ReportError(_when, ex);
                    errored = true;
                }
            }

            reporter.StartSection("Then");
            if (_definiton.ExpectedException != null)
            {
                var result = _validator.Verify(_definiton.ExpectedException, actualException, _definiton.VerifyExceptionMessage);
                reporter.Report(result);
            }

            if (errored)
            {
                reporter.ReportNotRun(then);
            }

            else
            {
                try
                {
                    var actual = _readResults();
                    var results = _validator.Verify(_thens, actual);
                    reporter.Report(results);
                }
                catch (Exception ex)
                {
                    errored = true;
                    foreach (var then in _thens)
                    {
                        reporter.ReportError(then, ex);
                    }
                }
            }


        }
    }
}

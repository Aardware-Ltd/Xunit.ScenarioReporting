using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    sealed class ScenarioReport 
    {
        public ScenarioReport(string assemblyName, IReportWriter writer)
        {
            _writer = writer;
            _queue = new ConcurrentQueue<Func<IReportWriter, Task>>();
            _queue.Enqueue(rw => rw.Write(new StartReport(assemblyName, DateTimeOffset.Now)));
        }

        internal Task WriteFinalAsync()
        {
            if (_error != null)
                return Task.FromException(_error);
            _final = new TaskCompletionSource<bool>();
            _queue.Enqueue(rw => rw.Write(new EndReport()));
            _queue.Enqueue(_ =>
            {
                _final.SetResult(true);
                return Task.CompletedTask;
            });
            EnsureWriting();
            if (_error != null)
                _final.TrySetException(_error);
            return _final.Task;
        }
        
        async void EnsureWriting()
        {
            if (_error != null) return;
            if (Interlocked.CompareExchange(ref _isWriting, 1, 0) != 0) return;
            await Task.Yield();
            try
            {
                Func<IReportWriter, Task> queued;
                while (_queue.TryDequeue(out queued))
                {
                    await queued(_writer);
                }
            }
            catch (Exception e)
            {
                _error = e;
                _final?.TrySetException(e);
            }
            finally
            {
                Interlocked.Exchange(ref _isWriting, 0);
            }

        }

        private readonly ConcurrentQueue<Func<IReportWriter, Task>> _queue;
        private readonly IReportWriter _writer;
        private Exception _error;
        private int _isWriting;
        private TaskCompletionSource<bool> _final;

        public void Report(Scenario scenario)
        {
            var writer = new DelayedBatchWriter(_queue);
            writer.Write(new StartScenario(scenario.Title));
            foreach (var given in scenario.GetGivens())
                writer.Write(given);
            writer.Write(scenario.GetWhen());
            foreach (var then in scenario.GetThens())
                writer.Write(then);
            writer.Complete();
            EnsureWriting();
        }

        class DelayedBatchWriter
        {
            private readonly ConcurrentQueue<Func<IReportWriter, Task>> _queue;
            private readonly Queue<ReportItem> _batch;
            public DelayedBatchWriter(ConcurrentQueue<Func<IReportWriter, Task>> queue)
            {
                _queue = queue;
                _batch = new Queue<ReportItem>();
            }

            internal void Complete()
            {
                _batch.Enqueue(new EndScenario());
                _queue.Enqueue(async sw =>
                {
                    while (_batch.Count > 0)
                    {
                        var current = _batch.Dequeue();
                        await sw.Write(current);
                    }
                });
            }

            private bool _hasStartedWritingGivens;
            private bool _hasStartedWritingThens;

            public void Write(StartScenario start)
            {
                _batch.Enqueue(start);
            }

            public void Write(Scenario.Given given)
            {
                if (!_hasStartedWritingGivens)
                {
                    _batch.Enqueue(new StartGivens());
                    _hasStartedWritingGivens = true;
                }
                else
                {
                    _batch.Enqueue(new AdditionalGiven());
                }
                _batch.Enqueue(given);
            }

            public void Write(Scenario.When when)
            {
                _batch.Enqueue(when);
            }

            public void Write(Scenario.Then then)
            {
                if (!_hasStartedWritingThens)
                {
                    _batch.Enqueue(new StartThens());
                    _hasStartedWritingThens = true;
                }
                else
                {
                    _batch.Enqueue(new AdditionalThen());
                }
                _batch.Enqueue(then);
            }
        }
    }
}
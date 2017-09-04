using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.ScenarioReporting.Results;
using Xunit.Sdk;

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

        public void Report(ScenarioRunResult result)
        {
            try
            {
                var writer = new DelayedBatchWriter(_queue);
                writer.Write(new StartScenario(result.Title ?? result.Scope, result.Scope));
                foreach (var given in result.Given)
                    writer.Write(given);
                if(result.When != null)
                    writer.Write(result.When);
                foreach (var then in result.Then)
                    writer.Write(then);
                writer.Complete();
            }
            catch(Exception ex)
            {
                _error = ex;
            }
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
            
            public void Write(StartScenario start)
            {
                _batch.Enqueue(start);
            }

            public void Write(Given given)
            {
                if(given != null)
                    _batch.Enqueue(given);
            }

            public void Write(When when)
            {
                if (when != null)
                    _batch.Enqueue(when);
            }

            public void Write(Then then)
            {
                if(then != null)
                    _batch.Enqueue(then);
            }
        }
    }
}
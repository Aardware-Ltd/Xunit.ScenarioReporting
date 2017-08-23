using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class ReturnsScenarioFromFact
    {
        [Fact]
        public Scenario BusExample()
        {
           return new ExampleScenario();
        }

        [Fact]
        public Scenario AsyncBusExample()
        {
            var bus = new Bus();
            bus.Subscribe(new SimpleResponder(bus));
            return new MessageBusScenario(bus).Define(def => def
                .Given()
                .When(new ReverseString("Hello, world!"))
                .Then(new ReversedString("!dlrow ,olleH")));
        }

        class SimpleResponder : IHandle<ReverseString> {
            private readonly Bus _bus;

            public SimpleResponder(Bus bus)
            {
                _bus = bus;
            }
            public Task Handle(ReverseString msg)
            {
                _bus.Publish(new ReversedString(new String(msg.ToReverse.Reverse().ToArray())));
                return Task.CompletedTask;
            }
        }

        class ReverseString : Message
        {
            public string ToReverse { get; }

            public ReverseString(string toReverse)
            {
                ToReverse = toReverse;
            }
        }
        class ReversedString : Message
        {
            public string Reversed { get; }

            public ReversedString(string reversed)
            {
                Reversed = reversed;
            }
        }
        class Message { }

        interface IHandle<T> where T: Message
        {
            Task Handle(T msg);
        }

        class Dispatcher : IHandle<Message>
        {
            private readonly Dictionary<Type, List<IHandlerWrapper>> _handlers;

            public Dispatcher()
            {
                _handlers = new Dictionary<Type, List<IHandlerWrapper>>();
                var subscriptionHandler = new SubscriptionHandler(_handlers);
                subscriptionHandler.Handle((SubscribeTo) Subscribe<SubscribeTo>(subscriptionHandler));
                subscriptionHandler.Handle((SubscribeTo) Subscribe<UnsubscribeFrom>(subscriptionHandler));
            }

            interface IHandlerWrapper
            {
                Task Handle(Message msg);
                bool IsSame(object other);
            }

            class HandlerWrapper<T> : IHandlerWrapper where T : Message
            {
                private readonly IHandle<T> _handler;

                public HandlerWrapper(IHandle<T> handler)
                {
                    _handler = handler;
                }

                public Task Handle(Message msg)
                {
                    return _handler.Handle((T)msg);
                }

                public bool IsSame(object other)
                {
                    return ReferenceEquals(_handler, other);
                }
            }
            class SubscriptionHandler : IHandle<SubscribeTo>, IHandle<UnsubscribeFrom> {
                private readonly Dictionary<Type, List<IHandlerWrapper>> _handlers;

                public SubscriptionHandler(Dictionary<Type, List<IHandlerWrapper>> handlers)
                {
                    _handlers = handlers;
                }

                public Task Handle(SubscribeTo msg)
                {
                    List<IHandlerWrapper> handlerList;
                    if (!_handlers.TryGetValue(msg.Type, out handlerList))
                    {
                        handlerList = new List<IHandlerWrapper>();
                        _handlers[msg.Type] = handlerList;
                    }
                    if (!handlerList.Any(x => x.IsSame(msg.Handler)))
                    {
                        handlerList.Add(msg.HandlerWrapper);
                    }
                    return Task.CompletedTask;
                }

                public Task Handle(UnsubscribeFrom msg)
                {
                    List<IHandlerWrapper> handlerList;
                    if (_handlers.TryGetValue(msg.Type, out handlerList))
                    {
                        handlerList = new List<IHandlerWrapper>();
                        handlerList.RemoveAll(x => x.IsSame(msg.Handler));
                        if (handlerList.Count == 0) _handlers.Remove(msg.Type);
                    }
                    return Task.CompletedTask;
                }
            }

            class SubscribeTo : Message
            {
                public Type Type { get; }
                public object Handler { get; }
                public IHandlerWrapper HandlerWrapper { get; }

                public SubscribeTo(Type type, object handler, IHandlerWrapper handlerWrapper)
                {
                    Type = type;
                    Handler = handler;
                    HandlerWrapper = handlerWrapper;
                }
            }

            class UnsubscribeFrom : Message
            {
                public Type Type { get; }
                public object Handler { get; }

                public UnsubscribeFrom(Type type, object handler)
                {
                    Type = type;
                    Handler = handler;
                }
            }

            public static Message Subscribe<T>(IHandle<T> handler) where T : Message
            {
                return new SubscribeTo(typeof(T), handler, new HandlerWrapper<T>(handler));
            }

            public static Message Unsubscribe<T>(IHandle<T> handler) where T : Message
            {
                return new UnsubscribeFrom(typeof(T), handler);
            }


            public async Task Handle(Message msg)
            {
                var msgType = msg.GetType();
                ;
                do
                {
                    List<IHandlerWrapper> handlers;
                    if (_handlers.TryGetValue(msgType, out handlers))
                    {
                        foreach (var handler in handlers)
                            await handler.Handle(msg);
                    }
                    msgType = msgType.BaseType;
                } while (msgType != typeof(object) && msgType != null);
            }
        }

        class Bus
        {
            private readonly QueuedHandler _queue;

            public Bus()
            {
                var dispatcher = new Dispatcher();
                _queue = new QueuedHandler(dispatcher);
            }

            public void Publish(Message msg)
            {
                _queue.Handle(msg);
            }

            public void Subscribe<T>(IHandle<T> handler) where T : Message
            {
                Publish(Dispatcher.Subscribe(handler));
            }

            public void Unsubscribe<T>(IHandle<T> handler) where T : Message
            {
                Publish(Dispatcher.Unsubscribe(handler));
            }
        }

        class QueuedHandler : IHandle<Message>
        {
            private readonly IHandle<Message> _next;
            private readonly ConcurrentQueue<Message> _queue;
            private TaskCompletionSource<bool> _pendingMessages;
            private CancellationToken _cancellationToken;
            private readonly CancellationTokenSource _cancel;

            public QueuedHandler(IHandle<Message> next)
            {
                _next = next;
                _queue = new ConcurrentQueue<Message>();

                _pendingMessages = new TaskCompletionSource<bool>();
                _cancel = new CancellationTokenSource();
                _cancellationToken = _cancel.Token;
                StartPump();
            }

            public Task Handle(Message msg)
            {
                _queue.Enqueue(msg);
                _pendingMessages.TrySetResult(true);
                return Task.CompletedTask;
            }

            async void StartPump()
            {
                do
                {
                    Message msg;
                    while (_queue.TryDequeue(out msg))
                    {
                        await _next.Handle(msg);
                    }
                    _pendingMessages = new TaskCompletionSource<bool>();
                    if(_queue.IsEmpty) 
                        await _pendingMessages.Task;
                } while (!_cancellationToken.IsCancellationRequested);
            }

            public void Stop()
            {
                _cancel.Cancel();
            }
        }
        class MessageBusScenario : ReflectionBasedScenario<Message, Message, Message>, IHandle<Message> {
            private readonly Bus _bus;
            private readonly TaskCompletionSource<bool> _allMessagesReceived;
            private readonly List<Message> _actuals;
            private bool _collecting;

            public MessageBusScenario(Bus bus)
            {
                _bus = bus;
                _actuals = new List<Message>();
                _bus.Subscribe(this);
                _allMessagesReceived = new TaskCompletionSource<bool>();
            }
            protected override Task Given(IReadOnlyList<Message> givens)
            {
                foreach (var msg in givens)
                    _bus.Publish(msg);
                return Task.CompletedTask;
            }

            protected override Task When(Message when)
            {
                _bus.Publish(when);
                return Task.CompletedTask;
            }

            protected override async Task<IReadOnlyList<Message>> ActualResults()
            {
                _bus.Publish(new EndMarker());
                await _allMessagesReceived.Task;
                return _actuals;
            }

            public Task Handle(Message msg)
            {
                if (msg is EndMarker)
                {
                    _allMessagesReceived.TrySetResult(true);
                    return Task.CompletedTask;
                }
                if (msg == Definition?.When)
                {
                    _collecting = true;
                    return Task.CompletedTask;
                }
                if (_collecting)
                {
                    _actuals.Add(msg);
                }
                return Task.CompletedTask;
            }

            class EndMarker : Message{}
        }
    }
}
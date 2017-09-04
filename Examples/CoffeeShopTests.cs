using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class CoffeeShopTests
    {
        [Fact]
        public Task<ScenarioRunResult> WhenOrderingFlatWhiteWithWrongExpectedOrder()
        {
            return new CoffeeShopScenarioRunner().Run(def => def
                .Given(new CoffeeReceived(Guid.NewGuid(), 10), new MilkReceived(Guid.NewGuid(), 10))
                .When(new MakeFlatWhite(Guid.NewGuid()))
                .Then(new CoffeeUsed(Guid.NewGuid(), 2), new MilkUsed(Guid.NewGuid(), 4)));
        }

        [Fact]
        public Task<ScenarioRunResult> WhenOrderingFlatWhite()
        {
            return new CoffeeShopScenarioRunner().Run(def => def
                .Given(new CoffeeReceived(Guid.NewGuid(), 10), new MilkReceived(Guid.NewGuid(), 10))
                .When(new MakeFlatWhite(Guid.NewGuid()))
                .Then(new MilkUsed(Guid.NewGuid(), 4), new CoffeeUsed(Guid.NewGuid(), 2)));
        }
        [Fact]
        public Task<ScenarioRunResult> WhenOrderingFlatWhitePartialMatch()
        {
            return new CoffeeShopScenarioRunner().Run(def => def
                .Given(new CoffeeReceived(Guid.NewGuid(), 10), new MilkReceived(Guid.NewGuid(), 10))
                .When(new MakeFlatWhite(Guid.NewGuid()))
                .Then(new MilkUsed(Guid.NewGuid(), 4), new CoffeeUsed(Guid.NewGuid(), 3)));
        }
        [Theory]
        [InlineData(10, 0, "Not enough milk")]
        [InlineData(10, 3, "Not enough milk")]
        [InlineData(0, 10, "Not enough coffee")]
        [InlineData(1, 10, "Not enough coffee")]
        public Task<ScenarioRunResult> WhenOrderingFlatWhiteWithInsufficinentResources(int coffee, int milk, string message)
        {
            List<Event> givens = new List<Event>();
            if(coffee > 0)
                givens.Add(new CoffeeReceived(Guid.NewGuid(),coffee));
            if(milk > 0)
                givens.Add(new MilkReceived(Guid.NewGuid(),milk));
            return new CoffeeShopScenarioRunner().Run(def => def
                .Given(givens.ToArray())
                .When(new MakeFlatWhite(Guid.NewGuid()))
                .Throws(new Exception(message)), $"Ordering flat white with {(milk > 0 ? milk.ToString() : "no") } milk and {(coffee > 0 ? coffee.ToString() :"no")} coffee");
        }

        class CoffeeShopScenarioRunner : ReflectionBasedScenarioRunner<Event, Command, Event>
        {
            private readonly CoffeeShop _shop;
            private readonly Dictionary<Type, Action<Command>> _cmdDispatcher;

            public CoffeeShopScenarioRunner()
            {
                _shop = new CoffeeShop();
                _cmdDispatcher = new Dictionary<Type, Action<Command>>()
                {
                    [typeof(MakeFlatWhite)] = c => _shop.MakeFlatwhite(),
                    [typeof(MakeAmericano)] = c => _shop.MakeAmericano(),
                    [typeof(DeliverCoffee)] = c => _shop.ReceiveCoffee(((DeliverCoffee)c).Amount),
                    [typeof(DeliverMilk)] = c => _shop.ReceiveCoffee(((DeliverMilk)c).Amount)
                };

            }

            protected override IReadOnlyList<string> IgnoredProperties => new[] { "Id" };

            protected override Task Given(IReadOnlyList<Event> givens)
            {
                foreach (var given in givens)
                {
                    _shop.Apply(given);
                }
                return Task.CompletedTask;
            }

            protected override Task When(Command when)
            {
                if (!_cmdDispatcher.TryGetValue(when.GetType(), out var handler))
                    throw new InvalidOperationException($"No handler for {when.GetType()}");
                handler(when);
                return Task.CompletedTask;
            }

            protected override Task<IReadOnlyList<Event>> ActualResults()
            {
                return Task.FromResult(_shop.PendingEvents());
            }
        }

        class CoffeeShop
        {
            private int _coffeeAmount;
            private int _milkAmount;
            private readonly List<Event> _pending;

            public void ReceiveCoffee(int amount)
            {
                Append(new CoffeeReceived(Guid.NewGuid(), amount));
            }

            public void ReceiveMilk(int amount)
            {
                Append(new MilkReceived(Guid.NewGuid(), amount));
            }

            public void MakeFlatwhite()
            {
                if (_milkAmount < 4)
                    throw new Exception("Not enough milk");
                if (_coffeeAmount < 2)
                    throw new Exception("Not enough coffee");
                Append(new MilkUsed(Guid.NewGuid(), 4));
                Append(new CoffeeUsed(Guid.NewGuid(), 2));
            }

            public void MakeAmericano()
            {
                if (_coffeeAmount < 2)
                    throw new Exception("Not enough coffee");
                Append(new MilkUsed(Guid.NewGuid(), 4));

            }

            public CoffeeShop()
            {
                _pending = new List<Event>();
            }

            public IReadOnlyList<Event> PendingEvents()
            {
                var result = _pending.ToArray();
                _pending.Clear();
                return result;
            }

            void Append(Event @event)
            {
                _pending.Add(@event);
            }

            public void Apply(Event @event)
            {
                switch (@event.GetType().Name)
                {
                    case nameof(CoffeeReceived):
                        Apply((CoffeeReceived)@event);
                        break;
                    case nameof(CoffeeUsed):
                        Apply((CoffeeUsed)@event);
                        break;
                    case nameof(MilkReceived):
                        Apply((MilkReceived)@event);
                        break;
                    case nameof(MilkUsed):
                        Apply((MilkUsed)@event);
                        break;
                    default: throw new InvalidOperationException($"Unknown event type {@event.GetType().Name}");
                }
            }

            void Apply(CoffeeReceived @event)
            {
                _coffeeAmount += @event.Amount;
            }

            void Apply(CoffeeUsed @event)
            {
                _coffeeAmount -= @event.Amount;
            }


            void Apply(MilkReceived @event)
            {
                _milkAmount += @event.Amount;
            }

            void Apply(MilkUsed @event)
            {
                _milkAmount -= @event.Amount;
            }
        }

        abstract class Message { }

        abstract class Event : Message
        {
            public Guid Id { get; }

            protected Event(Guid id)
            {
                Id = id;
            }
        }

        abstract class Command : Message
        {
            public Guid Id { get; }

            protected Command(Guid id)
            {
                Id = id;
            }
        }
        class MakeFlatWhite : Command
        {
            public MakeFlatWhite(Guid id) : base(id)
            {
            }
        }

        class MakeAmericano : Command
        {
            public MakeAmericano(Guid id) : base(id)
            {
            }
        }

        class DeliverCoffee : Command
        {
            public int Amount { get; }

            public DeliverCoffee(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
        }

        class DeliverMilk : Command
        {

            public int Amount { get; }
            public DeliverMilk(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
        }

        class CoffeeReceived : Event
        {
            public CoffeeReceived(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
            public int Amount { get; }
        }

        class CoffeeUsed : Event
        {
            public CoffeeUsed(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
            public int Amount { get; }
        }

        class MilkReceived : Event
        {
            public int Amount { get; }

            public MilkReceived(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
        }

        class MilkUsed : Event
        {
            public int Amount { get; }

            public MilkUsed(Guid id, int amount) : base(id)
            {
                Amount = amount;
            }
        }
    }
}



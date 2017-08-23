using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples.ExampleDomain
{
    class CalculatorAggregate
    {
        readonly List<Number> _numbers = new List<Number>();

        public void Enter(Number number)
        {
            _numbers.Add(number);
        }

        public ComputedResult Compute(Operation operation)
        {
            switch (operation.Type)
            {
                case OperationType.Add:
                    return new ComputedResult(_numbers.Select(x => x.Value).Sum());
                case OperationType.Multiply:
                    return new ComputedResult(_numbers.Select(x => x.Value).Aggregate(1, (a, b) => a * b));
            }
            throw new NotSupportedException("Unsupported operation type");
        }
    }
}
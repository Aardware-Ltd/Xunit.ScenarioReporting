using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.ScenarioReporting
{
    static class EnumerableExtensions
    {
        //Breadth first traversal
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> elementSelector)
        {
            var stack = new Stack<T>();
            foreach(var item in source)
                stack.Push(item);

            while(stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                var children = elementSelector(current);
                if (children == null) continue;

                foreach (var child in children) 
                    stack.Push(child);
            }

        }

        //Depth first traversal
        public static IEnumerable<T> Expand<T>(
            this IEnumerable<T> source, Func<T, IEnumerable<T>> elementSelector)
        {
            var stack = new Stack<IEnumerator<T>>();
            var e = source.GetEnumerator();
            try
            {
                while (true)
                {
                    while (e.MoveNext())
                    {
                        var item = e.Current;
                        yield return item;
                        var elements = elementSelector(item);
                        if (elements == null) continue;
                        stack.Push(e);
                        e = elements.GetEnumerator();
                    }
                    if (stack.Count == 0) break;
                    e.Dispose();
                    e = stack.Pop();
                }
            }
            finally
            {
                e.Dispose();
                while (stack.Count != 0) stack.Pop().Dispose();
            }
        }
    }
}

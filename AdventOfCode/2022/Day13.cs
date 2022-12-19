using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-13-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-13-02", MethodName = nameof(ExecutePart2))]
public class Day13
{
    private const string TestData = @"[1,1,3,1,1]
[1,1,5,1,1]

[[1],[2,3,4]]
[[1],4]

[9]
[[8,7,6]]

[[4,4],4,4]
[[4,4],4,4,4]

[7,7,7,7]
[7,7,7]

[]
[3]

[[[]]]
[[]]

[1,[2,[3,[4,[5,6,7]]]],8,9]
[1,[2,[3,[4,[5,6,0]]]],8,9]";

    private abstract record Value
    {
        public sealed record List(ImmutableArray<Value> Elements) : Value
        {
            public List(params Value[] values) : this(ImmutableArray.Create(values))
            {
            }

            protected override List ForceList()
            {
                return this;
            }

            public override string ToString()
            {
                return $"[{string.Join(",", Elements)}]";
            }

            public Value? TryGetSingle()
            {
                return Elements.Length == 1 ? Elements[0] : null;
            }
        }

        public sealed record Integer(int Value) : Value
        {
            protected override List ForceList()
            {
                return new(ImmutableArray.Create<Value>(this));
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }


        protected abstract List ForceList();

        public sealed class Comparer : IComparer<Value>
        {
            public static readonly Comparer Instance = new();

            public int Compare(Value? left, Value? right)
            {
                if (left == null)
                    return -1;
                if (right == null)
                    return 1;
                if (ReferenceEquals(left, right))
                    return 0;
                if (left is Integer leftInt && right is Integer rightInt)
                    return leftInt.Value.CompareTo(rightInt.Value);
                var leftList = left.ForceList();
                var rightList = right.ForceList();
                for (var i = 0; i < Math.Min(leftList.Elements.Length, rightList.Elements.Length); ++i)
                {
                    var cmp = Compare(leftList.Elements[i], rightList.Elements[i]);
                    if (cmp != 0)
                        return cmp;
                }

                return leftList.Elements.Length.CompareTo(rightList.Elements.Length);
            }
        }
    }


    private static IEnumerable<(Value.List, Value.List)> ParseInput(string input)
    {
        var parser = Parser.Recursive<Value.List>(p =>
        {
            var elementParser = Parser.OneOf(
                p.Select(v => (Value)v),
                Parser.UnsignedInteger.Select(v => (Value)new Value.Integer(v)));
            return elementParser
                .DelimitedWith(Parser.Fixed(","))
                .Bracket("[", "]")
                .Select(v => new Value.List(v.ToImmutableArray()));
        });
        var parserPair = from first in parser
            from _ in Parser.NewLine
            from second in parser
            select (Left: first, Right: second);
        var parserPairs = parserPair.DelimitedWith(Parser.EmptyLine);
        return parserPairs.Parse(input);
    }

    [TestCase(TestData, 13)]
    public static int ExecutePart1(string input)
    {
        var count = ParseInput(input)
            .Select((pair, i) => Value.Comparer.Instance.Compare(pair.Item1, pair.Item2) < 0 ? (int?)(i + 1) : null)
            .WhereStructNotNull().Sum();
        return count;
    }

    [TestCase(TestData, 140)]
    public static int ExecutePart2(string input)
    {
        var count = ParseInput(input)
            .SelectMany(pair => new[] { pair.Item1, pair.Item2 })
            .Append(CreateDoubleList(2))
            .Append(CreateDoubleList(6))
            .OrderBy(x => x, Value.Comparer.Instance)
            .Indexed()
            .Where(x => IsSeparator(x.Value))
            .Select(x => x.Index + 1)
            .Aggregate(MathInt.Mul);
        return count;

        static bool IsSeparator(Value.List list)
        {
            return list.TryGetSingle() is Value.List elem && elem.TryGetSingle() is Value.Integer { Value: 2 or 6 };
        }

        static Value.List CreateDoubleList(int value)
        {
            return new(new Value.List(new Value.Integer(value)));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-11-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-11-02", MethodName = nameof(ExecutePart2))]
public class Day11
{
    private const string TestData = @"Monkey 0:
  Starting items: 79, 98
  Operation: new = old * 19
  Test: divisible by 23
    If true: throw to monkey 2
    If false: throw to monkey 3

Monkey 1:
  Starting items: 54, 65, 75, 74
  Operation: new = old + 6
  Test: divisible by 19
    If true: throw to monkey 2
    If false: throw to monkey 0

Monkey 2:
  Starting items: 79, 60, 97
  Operation: new = old * old
  Test: divisible by 13
    If true: throw to monkey 1
    If false: throw to monkey 3

Monkey 3:
  Starting items: 74
  Operation: new = old + 3
  Test: divisible by 17
    If true: throw to monkey 0
    If false: throw to monkey 1
";

    readonly record struct Worry(ulong Value);
    readonly record struct MonkeyId(int Value);
    sealed record class MonkeyLogic(MonkeyId Id, ImmutableArray<Worry> StartingItems, Func<Worry, Worry> UpdateWorry, ulong Divisor, MonkeyId IfTrue, MonkeyId IfFalse)
    {
      public MonkeyId GetTarget(Worry w) => w.Value % Divisor == 0 ? IfTrue : IfFalse;
    }

    sealed class Monkey
    {
        public readonly MonkeyLogic Logic;
        private readonly List<Worry> Items;
        private readonly bool _divideByThree;
        private readonly ulong _modulo;
        public ulong InspectedItemCount { get; private set; }

        public Monkey(MonkeyLogic logic, bool divideByThree)
        {
            Logic = logic ?? throw new ArgumentNullException(nameof(logic));
            Items = logic.StartingItems.ToList();
            _divideByThree = divideByThree;
        }
        public Monkey(MonkeyLogic logic, bool divideByThree, ulong modulo)
        {
            Logic = logic ?? throw new ArgumentNullException(nameof(logic));
            Items = logic.StartingItems.ToList();
            _divideByThree = divideByThree;
            _modulo = modulo;
        }

        public void PlayRound(Func<MonkeyId, Monkey> world)
        {
            foreach (var item in Items)
            {
                var updatedItem = Logic.UpdateWorry(item);
                if (_divideByThree)
                    updatedItem = new Worry(updatedItem.Value / 3);
                else
                    updatedItem = new Worry(updatedItem.Value % _modulo);
                world(Logic.GetTarget(updatedItem)).Items.Add(updatedItem);
            }
            InspectedItemCount += (ulong)Items.Count;
            Items.Clear();
        }
    }

    private static T Identity<T>(T x) => x;
    private static Func<T, TResult> Value<T, TResult>(TResult x) => _ => x;
    private static IEnumerable<MonkeyLogic> ParseInput(string input)
    {
        var parserItemList = Parser.UnsignedInteger.DelimitedWith(Parser.Fixed(", "));
        var parserValue = Parser.OneOf(
            Parser.Fixed("old").Return(Identity<ulong>),
            Parser.UnsignedInteger.Select(x => (ulong)x).Select(Value<ulong, ulong>));
        var parserOperation =
            from first in parserValue
            from op in Parser.OneOf(Parser.Fixed("*").Return(MathULong.Mul), Parser.Fixed("+").Return(MathULong.Add)).Trimmed()
            from second in parserValue
            select (Func<ulong, ulong>)(old => op(first(old), second(old)));
        static MonkeyLogic CreateLogic(MonkeyId id, IEnumerable<int> startItems, Func<ulong, ulong> update, int div, MonkeyId ifTrue, MonkeyId ifFalse) =>
            new (id, startItems.Select(i => new Worry((ulong)i)).ToImmutableArray(), worry => new Worry(update(worry.Value)), (ulong)div, ifTrue, ifFalse);
        var parserMonkey = Parser.FormattedString(@$"Monkey {Parser.UnsignedInteger.Select(x => new MonkeyId(x))}:
  Starting items: {parserItemList}
  Operation: new = {parserOperation}
  Test: divisible by {Parser.UnsignedInteger}
    If true: throw to monkey {Parser.UnsignedInteger.Select(x => new MonkeyId(x))}
    If false: throw to monkey {Parser.UnsignedInteger.Select(x => new MonkeyId(x))}", m => CreateLogic(m[0], m[1], m[2], m[3], m[4], m[5])).DelimitedWith(Parser.EmptyLine);

        return parserMonkey.Parse(input);
    }
    private static ulong PlayGame(ImmutableArray<Monkey> monkeys, int rounds)
    {
        for (var i = 0; i < rounds; ++i)
            foreach (var monkey in monkeys)
                monkey.PlayRound(id => monkeys[id.Value]);
        return monkeys.Select(x => x.InspectedItemCount).OrderByDescending(x => x).Take(2).Aggregate(MathULong.Mul);
    }

    [TestCase(TestData, 10605ul)]
    public static ulong ExecutePart1(string input)
    {
        var monkeys = ParseInput(input.ReplaceLineEndings()).Select(l => new Monkey(l, true)).ToImmutableArray();
        return PlayGame(monkeys, 20);
    }

    [TestCase(TestData, 2713310158ul)]
    public static ulong ExecutePart2(string input)
    {
        var monkeyLogic = ParseInput(input.ReplaceLineEndings()).ToArray();
        var common = monkeyLogic.Select(m => m.Divisor).Aggregate(MathULong.Lcm);
        var monkeys = monkeyLogic.Select(l => new Monkey(l, false, common)).ToImmutableArray();
        return PlayGame(monkeys, 10000);
    }
}
using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace AdventOfCode._2016;

[Problem("2016-11-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-11-02", MethodName = nameof(ExecutePart2))]
public class Day11
{
    readonly record struct FloorState(byte Generators, byte Shields)
    {
        public bool IsStable => Generators == 0 || (Shields & Generators) == Shields;
        public bool IsEmpty => Generators == 0 && Shields == 0;
        public int Count => BitOperations.PopCount(Generators) + BitOperations.PopCount(Shields);
        private static IEnumerable<byte> Bits(byte b)
        {
            for (var i = 0; i < 8; ++i)
            {
                byte toMove = (byte)(b & (1 << i));
                if (toMove != 0)
                    yield return toMove;
            }
        }
        private static IEnumerable<byte> BitPairs(byte b)
        {
            for (var i = 0; i < 8; ++i)
            {
                byte toMove1 = (byte)(b & (1 << i));
                if (toMove1 != 0)
                {
                    yield return toMove1;
                    for (var j = i + 1; j < 8; ++j)
                    {
                        byte toMove2 = (byte)(b & (1 << j));
                        if (toMove2 != 0)
                            yield return (byte)(toMove1 | toMove2);
                    }
                }
            }
        }
        private IEnumerable<byte> GetGenerators() => Bits(Generators);
        private IEnumerable<byte> GetShields() => Bits(Shields);
        private IEnumerable<byte> GetShieldPairs() => BitPairs(Shields);
        private IEnumerable<byte> GetGeneratorPairs() => BitPairs(Generators);

        public IEnumerable<(FloorState, FloorState)> MovableTo(FloorState dst)
        {
            // Move a single generator
            foreach(var toMove in GetGeneratorPairs())
            {
                var newDst = dst with { Generators = (byte)(dst.Generators | toMove) };
                var newSrc = this with { Generators = (byte)(this.Generators & ~toMove) };
                if (newDst.IsStable && newSrc.IsStable)
                    yield return (newSrc, newDst);
            }
            // Move a single shield
            foreach(var toMove in GetShieldPairs())
            {
                var newDst = dst with { Shields = (byte)(dst.Shields | toMove) };
                var newSrc = this with { Shields = (byte)(this.Shields & ~toMove) };
                if (newDst.IsStable && newSrc.IsStable)
                    yield return (newSrc, newDst);
            }
            // Move a shield and a generator
            foreach(var shieldToMove in GetShields())
            {
                foreach(var generatorToMove in GetGenerators())
                {
                    var newDst = dst with { Generators = (byte)(dst.Generators | generatorToMove) , Shields = (byte)(dst.Shields | shieldToMove) };
                    var newSrc = this with { Generators = (byte)(this.Generators & ~generatorToMove), Shields = (byte)(this.Shields & ~shieldToMove) };
                    if (newDst.IsStable && newSrc.IsStable)
                        yield return (newSrc, newDst);
                }
            }
        }
        public override string ToString()
        {
            string result = "";
            for (var i = 0; i < 8; ++i)
            {
                if ((Shields & (1 << i)) != 0)
                    result += $"M{i}";
            }
            for (var i = 0; i < 8; ++i)
            {
                if ((Generators & (1 << i)) != 0)
                    result += $"G{i}";
            }
            return result;
        }
    }
    readonly record struct GameState(FloorState Floor1, FloorState Floor2, FloorState Floor3, FloorState Floor4, byte ElevatorPosition)
    {
        public FloorState this[byte id] => id switch { 1 => Floor1, 2 => Floor2, 3 => Floor3, 4 => Floor4 };
        private GameState Update(byte id, FloorState state) => id switch
        {
            1 => this with { Floor1 = state },
            2 => this with { Floor2 = state },
            3 => this with { Floor3 = state },
            4 => this with { Floor4 = state },
            _ => this,
        };
        private IEnumerable<byte> Steps()
        {
            if (ElevatorPosition < 4)
                yield return (byte)(ElevatorPosition + 1);
            if (ElevatorPosition > 1)
                yield return (byte)(ElevatorPosition - 1);
        }
        public IEnumerable<GameState> GetNextState()
        {
            foreach(var step in Steps())
                foreach (var (newCur, newDst) in this[ElevatorPosition].MovableTo(this[step]))
                    yield return Update(ElevatorPosition, newCur).Update(step, newDst) with { ElevatorPosition = step };
        }
        public bool IsFinalState => Floor1.IsEmpty && Floor2.IsEmpty && Floor3.IsEmpty;

        public int OverestimateRemainingSteps => Floor3.Count + 2 * Floor2.Count + 3 * Floor1.Count;
    }

    interface IItem
    {
    }
    record class Generator(string Name) : IItem;
    record class Shield(string Name) : IItem;
    record class FloorDesc(int Id, ImmutableArray<string> Generators, ImmutableArray<string> Shields)
    {
        public static FloorDesc Create(int id, IEnumerable<IItem> items)
        {
            var generators = items.OfType<Generator>().Select(x => x.Name).ToImmutableArray();
            var shields = items.OfType<Shield>().Select(x => x.Name).ToImmutableArray();
            return new FloorDesc(id, generators, shields);
        }
        public static FloorDesc Merge(FloorDesc a, FloorDesc b)
        {
            if (a.Id != b.Id)
                throw new InvalidOperationException();
            return new FloorDesc(a.Id, a.Generators.AddRange(b.Generators), a.Shields.AddRange(b.Shields));
        }
    }
    private static GameState FromFloors(IEnumerable<FloorDesc> floors)
    {
        var mergedFloors = floors.GroupBy(g => g.Id).Select(g => g.Aggregate(FloorDesc.Merge)).ToArray();
        static byte Or(IEnumerable<byte> values) => values.Aggregate((byte)0, MathByte.Or);
        var elementMaps = mergedFloors.SelectMany(f => f.Generators.Concat(f.Shields)).ToHashSet().OrderBy(x => x).Select((x, i) => KeyValuePair.Create(x, (byte)(1 << i))).ToImmutableDictionary();
        FloorState CreateFloorState(FloorDesc desc)
        {
            var generators = Or(from x in desc.Generators select elementMaps[x]);
            var shields = Or(from x in desc.Shields select elementMaps[x]);
            return new FloorState(generators, shields);
        }
        var dict = mergedFloors.Select(f => KeyValuePair.Create(f.Id, CreateFloorState(f))).ToImmutableDictionary();
        return new GameState(
            dict[1],
            dict[2],
            dict[3],
            dict[4],
            1);
    }
    [TestCase(
        @"The first floor contains a hydrogen-compatible microchip and a lithium-compatible microchip.
The second floor contains a hydrogen generator.
The third floor contains a lithium generator.
The fourth floor contains nothing relevant.
", 11)]
    public static int ExecutePart1(string input)
    {
        var type = Parser.OneOf(
            Parser.Fixed(" generator").Return((Func<string, IItem>)(x => new Generator(x))),
            Parser.Fixed("-compatible microchip").Return((Func<string, IItem>)(x => new Shield(x))));
        var item = Parser.FormattedString($"a {Parser.AlphaNumeric}{type}", m => (IItem)m[1](m[0]));
        var items = item.DelimitedWith(Parser.OneOf(Parser.Fixed(", and "), Parser.Fixed(", "), Parser.Fixed(" and ")));
        var info = Parser.OneOf(Parser.Fixed("nothing relevant").Return(Enumerable.Empty<IItem>()), items).Select(x => x.ToImmutableArray());
        var line = Parser.FormattedString($"The {Parser.EnglishNumber} floor contains {info}.", m => FloorDesc.Create((int)m[0], (IEnumerable<IItem>)m[1]));
        var gameState = line.ThenIgnore(Parser.NewLine).Repeat().Select(FromFloors);
        var gameStateInput = gameState.Parse(input);
        return Utilities.AStarLength(gameStateInput, x => x.OverestimateRemainingSteps, x => x.IsFinalState, x => x.GetNextState())!.Value;
    }
    public static int ExecutePart2(string input)
    {
        return ExecutePart1(input + "The first floor contains a elerium-compatible microchip, a elerium generator, a dilithium generator, a dilithium-compatible microchip.\n");
    }
}

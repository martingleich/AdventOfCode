using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ProblemsLibrary;

namespace AdventOfCode._2016;

[Problem("2016-11-01", MethodName = nameof(ExecutePart1))]

[Problem("2016-11-02", MethodName = nameof(ExecutePart2))]
public class Day11
{
    private readonly struct FloorState : IEquatable<FloorState>
    {
        public readonly byte _sources;
        public readonly byte _shields;

        public FloorState(byte sources, byte shields)
        {
            _sources = sources;
            _shields = shields;
        }

        public bool IsSafe => (_shields & ~_sources) == 0 || _sources == 0; // If there is a shield without a source and any source -> fail
        public bool Equals(FloorState other) => _sources == other._sources && _shields == other._shields;
        public bool IsEmpty => _sources == 0 && _shields == 0;
        public override int GetHashCode() =>  HashCode.Combine(_sources, _shields);
        public override bool Equals(object? obj) => obj is FloorState state && Equals(state);

        private FloorState Remove(byte shield, byte sources)
        {
            return new FloorState((byte)(_shields & ~shield), (byte)(_sources & ~sources));
        }
        private FloorState Add(byte shield, byte sources)
        {
            return new FloorState((byte)(_shields | shield), (byte)(_sources | sources));
        }

        public static IEnumerable<(FloorState, FloorState)> GetFromTo(FloorState from, FloorState to)
        {
            // We can either take:
            // - a source+shield
            // - a shield
            // - a source
            var singleTakableShields = from._shields;
            var singleTakableSources = BitOperations.PopCount(from._sources) == 1 ? from._sources : from._sources & ~from._shields; // We can only take a source if it is the only one, or no shield is connected to it
            // We can take a shield and source together if 
            // Look at all combination in from.
            
            var sourceOptions = AllSubRanges[from._sources];
            foreach(var shieldOption in shieldOptions)
            foreach (var sourceOption in sourceOptions)
            {
                var newFrom = from.Remove(shieldOption, sourceOption);
                var newTo = to.Add(shieldOption, sourceOption);
                if (newFrom.IsSafe && newTo.IsSafe)
                    yield return (newFrom, newTo);
            }
        }
    }
    private static IEnumerable<byte> ComputeAllSubranges(byte value)
    {
        var options = new byte[BitOperations.PopCount(value)];
        int i = 0;
        while (value != 0)
        {
            byte mask = 1;
            if ((value & mask) != 0)
            {
                value &= (byte)~mask;
                options[i++] = mask;
            }
            mask <<= 1;
        }

        for (int j = 0; j < 1 << options.Length; ++j)
        {
            yield return Enumerable.Range(1, options.Length).Select(i => (byte)((j & (1 << i)) != 0 ? options[i] : 0))
                .Aggregate((a, b) => (byte)(a | b));
        }
    }

    private static byte[][] AllSubRanges = Enumerable.Range(0, 255).Select(i => ComputeAllSubranges((byte)i).ToArray()).ToArray();
    private readonly struct GameState : IEquatable<GameState>
    {
        private readonly FloorState floor0;
        private readonly FloorState floor1;
        private readonly FloorState floor2;
        private readonly FloorState floor3;
        private readonly byte position;

        public GameState(FloorState floor0, FloorState floor1, FloorState floor2, FloorState floor3, byte position)
        {
            this.floor0 = floor0;
            this.floor1 = floor1;
            this.floor2 = floor2;
            this.floor3 = floor3;
            this.position = position;
        }

        public IEnumerable<GameState> GetPossibleFollowUps()
        {
            if (position > 0)
                foreach (var x in FromTo(position, (byte)(position - 1)))
                    yield return x;
            if (position < 3)
                foreach (var x in FromTo(position, (byte)(position + 1)))
                    yield return x;
        }

        private FloorState this[int i] => i switch
        {
            0 => floor0,
            1 => floor1,
            2 => floor2,
            3 => floor3,
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
        private GameState With(byte i, FloorState f) => i switch
        {
            0 => new GameState(f, floor1, floor2, floor3, position),
            1 => new GameState(floor0, f, floor2, floor3, position),
            2 => new GameState(floor0, floor1, f, floor3, position),
            3 => new GameState(floor0, floor1, floor2, f, position),
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
        private IEnumerable<GameState> FromTo(byte from, byte to)
        {
            foreach (var (fromState, toState) in FloorState.GetFromTo(this[from], this[to]))
                yield return With(from, fromState).With(to, toState).WithPlayerAt(to);
        }

        private GameState WithPlayerAt(byte to) => new (floor0, floor1, floor2, floor3, to);

        public bool IsDone => floor0.IsEmpty && floor1.IsEmpty && floor2.IsEmpty;
        public bool Equals(GameState other) => floor0.Equals(other.floor0) && floor1.Equals(other.floor1) && floor2.Equals(other.floor2) && floor3.Equals(other.floor3) && position == other.position;
        public override bool Equals(object? obj) => obj is GameState other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(floor0, floor1, floor2, floor3, position);
    }
    
    private static int GetMinimal(
        GameState state,
        int count,
        int minCount,
        List<GameState> buffer,
        ISet<GameState> visited)
    {
        if (!visited.Add(state) || count >= minCount || state.IsDone)
            return count;
        var startCount = buffer.Count;
        buffer.AddRange(state.GetPossibleFollowUps());
        for (var i = startCount; i < buffer.Count; ++i)
        {
            minCount = Math.Min(GetMinimal(buffer[i], count + 1, minCount, buffer, visited), minCount);
        }

        buffer.RemoveRange(startCount, buffer.Count - startCount);
        return minCount;
    }
    public static int ExecutePart1(string input)
    {
        return GetMinimal(default, 0, int.MaxValue, new List<GameState>(), new HashSet<GameState>());
    }
    public static int ExecutePart2(string input) => throw new NotImplementedException();
}


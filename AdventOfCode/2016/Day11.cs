namespace AdventOfCode._2016;
/*
[Problem("2016-11-01", MethodName = nameof(ExecutePart1))]

[Problem("2016-11-02", MethodName = nameof(ExecutePart2))]
public class Day11
{
    private readonly struct FloorState : IEquatable<FloorState>
    {
        public readonly byte _sources;
        public readonly byte _shields;

        public bool IsSafe => (_shields & ~_sources) == 0 || _sources == 0; // If there is a shield without a source and any source -> fail

        public bool Equals(FloorState other) => _sources == other._sources && _shields == other._shields;
        public static void GetPossibleChanges(FloorState from, FloorState to, int toId, List<(int, (FloorState, FloorState))> changeList)
        {
        }
        public bool IsEmpty => _sources == 0 && _shields == 0;
    }
    private static int GetMinimal(FloorState[] floors, int playerPos, int count, int minCount, List<(int, (FloorState, FloorState))> changeList)
    {
        if (count >= minCount)
            return minCount;
        bool done = true;
        for (int i = 0; i < floors.Length - 1; ++i)
            if (!floors[i].IsEmpty)
            {
                done = false;
                break;
            }
        if (done)
            return count;
        int startCount = changeList.Count;
        if (playerPos < floors.Length - 1)
            FloorState.GetPossibleChanges(floors[playerPos], floors[playerPos + 1], playerPos + 1, changeList);
        if (playerPos > 0)
            FloorState.GetPossibleChanges(floors[playerPos], floors[playerPos - 1], playerPos - 1, changeList);
        for(int i = startCount; i < changeList.Count; ++i)
        {
            var option = changeList[i];
            var (oldFrom, oldTo) = (floors[playerPos], floors[option.Item1]);
            (floors[playerPos], floors[option.Item1]) = option.Item2;
            minCount = Math.Min(minCount, GetMinimal(floors, option.Item1, count + 1, minCount, changeList));
            (floors[playerPos], floors[option.Item1]) = (oldFrom, oldTo);
        }
        changeList.RemoveRange(startCount, changeList.Count - startCount);
        return minCount;
    }
    public static int ExecutePart1(string input)
    {
        return GetMinimal(,,, int.MaxValue);
    }
    public static int ExecutePart2(string input) => throw new NotImplementedException();
}
*/
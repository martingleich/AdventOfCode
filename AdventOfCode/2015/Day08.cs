using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-08-1")]
public class Day08
{
    public static int CodeLength(string input)
    {
        var count = -2; // For the quotes
        var state = State.Normal;
        foreach (var c in input)
            switch (state)
            {
                case State.Normal:
                    if (c == '\\')
                        state = State.Back;
                    else
                        ++count;
                    break;
                case State.Back:
                    if (c == 'x')
                    {
                        state = State.Hex1;
                    }
                    else
                    {
                        ++count;
                        state = 0;
                    }

                    break;
                case State.Hex1:
                    state = State.Hex2;
                    break;
                case State.Hex2:
                    state = State.Normal;
                    ++count;
                    break;
            }

        return count;
    }

    [TestCase("\"\"", 2)]
    [TestCase("\"abc\"", 2)]
    [TestCase("\"aaa\\\"aaa\"", 3)]
    [TestCase("\"\\x27\"", 5)]
    [TestCase("\"\"\n\"a\"", 4)]
    public int Execute(string input)
    {
        return input.SplitLines().Sum(line => line.Length - CodeLength(line));
    }

    private enum State
    {
        Normal,
        Back,
        Hex1,
        Hex2
    }
}

[Problem("2015-08-2")]
public class Day08Part2
{
    public static int EncodedLength(char c)
    {
        return c switch
        {
            '\"' => 2,
            '\\' => 2,
            _ => 1
        };
    }

    public static int EncodedLength(string input)
    {
        return 2 + input.Sum(EncodedLength);
    }

    [TestCase("\"\"\n\"abc\"\n\"aaa\\\"aaa\"\n\"\\x27\"", 19)]
    public int Execute(string input)
    {
        return input.SplitLines().Sum(line => EncodedLength(line) - line.Length);
    }
}
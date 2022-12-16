using System;
using System.Collections.Immutable;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

#pragma warning disable CS8509, CS8524
[Problem("2022-02-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-02-02", MethodName = nameof(ExecutePart2))]
public class Day02
{
    private const string TEST_DATA = @"
A Y
B X
C Z
";

    private static readonly ImmutableArray<Action> Actions =
        ImmutableArray.Create(Action.Rock, Action.Paper, Action.Scissors);

    private static Action ParseMy(string str)
    {
        return str switch
        {
            "X" => Action.Rock,
            "Y" => Action.Paper,
            "Z" => Action.Scissors
        };
    }

    private static Action ParseTheir(string str)
    {
        return str switch
        {
            "A" => Action.Rock,
            "B" => Action.Paper,
            "C" => Action.Scissors
        };
    }

    private static Result ParseResult(string str)
    {
        return str switch
        {
            "X" => Result.Loose,
            "Y" => Result.Draw,
            "Z" => Result.Win
        };
    }

    private static Action Beats(Action action)
    {
        return action switch
        {
            Action.Rock => Action.Scissors,
            Action.Paper => Action.Rock,
            Action.Scissors => Action.Paper
        };
    }

    private static Result GetMyResult(Action theirs, Action mine)
    {
        if (Beats(theirs) == mine)
            return Result.Loose;
        if (Beats(mine) == theirs)
            return Result.Win;
        return Result.Draw;
    }

    private static int GetPointsFor(Action theirs, Action mine)
    {
        return mine switch
        {
            Action.Rock => 1,
            Action.Paper => 2,
            Action.Scissors => 3
        } + GetMyResult(theirs, mine) switch
        {
            Result.Loose => 0,
            Result.Draw => 3,
            Result.Win => 6
        };
    }

    private static Action GetRequiredAction(Action theirs, Result result)
    {
        return Actions.First(action => GetMyResult(theirs, action) == result);
    }

    [TestCase(TEST_DATA, 15)]
    public static int ExecutePart1(string input)
    {
        return input.SplitLines().Select(str =>
        {
            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var theirs = ParseTheir(parts[0]);
            var mine = ParseMy(parts[1]);
            return (theirs, mine);
        }).Sum(x => GetPointsFor(x.theirs, x.mine));
    }

    [TestCase(TEST_DATA, 12)]
    public static int ExecutePart2(string input)
    {
        return input.SplitLines().Select(str =>
        {
            var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var theirs = ParseTheir(parts[0]);
            var result = ParseResult(parts[1]);
            return (theirs, mine: GetRequiredAction(theirs, result));
        }).Sum(x => GetPointsFor(x.theirs, x.mine));
    }

    private enum Action
    {
        Rock,
        Paper,
        Scissors
    }

    private enum Result
    {
        Win,
        Draw,
        Loose
    }
}
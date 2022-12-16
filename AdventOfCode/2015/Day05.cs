using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-05-1")]
public class Day05
{
    public static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };
    private static readonly string[] EvilStrings = { "ab", "cd", "pq", "xy" };

    public static bool IsVowel(char c)
    {
        return Vowels.Contains(c);
    }

    public static bool HasThreeVowels(string input)
    {
        return input.Count(IsVowel) >= 3;
    }

    public static bool HasDoubleLetter(string input)
    {
        return input.Aggregate(
            (lastChar: char.MinValue, hasDouble: false),
            (a, c) => (c, a.hasDouble || a.lastChar == c)).hasDouble;
    }

    public static bool ContainsEvilString(string input)
    {
        return EvilStrings.Any(evil => input.Contains(evil));
    }

    public static bool IsNice(string input)
    {
        return HasThreeVowels(input) && HasDoubleLetter(input) && !ContainsEvilString(input);
    }

    [TestCase("ugknbfddgicrmopn", 1)]
    [TestCase("aaa", 1)]
    [TestCase("jchzalrnumimnmhp", 0)]
    [TestCase("haegwjzuvuyypxyu", 0)]
    [TestCase("dvszwmarrgswjxmb", 0)]
    public int Execute(string input)
    {
        return input.SplitLines().Count(IsNice);
    }
}

[Problem("2015-05-2")]
public class Day05Part2
{
    public static bool ContainsPairTwice(string input)
    {
        for (var i = 0; i < input.Length - 2; ++i)
        {
            var substring = input.Substring(i, 2);
            if (input.IndexOf(substring, i + 2) >= 0)
                return true;
        }

        return false;
    }

    public static bool ContainsRepeatWithSpace(string input)
    {
        for (var i = 0; i < input.Length - 2; ++i)
            if (input[i] == input[i + 2])
                return true;
        return false;
    }

    public static bool IsNice(string input)
    {
        return ContainsPairTwice(input) && ContainsRepeatWithSpace(input);
    }

    [TestCase("qjhvhtzxzqqjkmpb", 1)]
    [TestCase("xxyxx", 1)]
    [TestCase("uurcxstgmygtbstg", 0)]
    [TestCase("ieodomkazucvgmuy", 0)]
    public int Execute(string input)
    {
        return input.SplitLines().Count(IsNice);
    }
}
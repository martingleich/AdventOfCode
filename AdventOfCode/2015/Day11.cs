using System;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-11-1")]
public class Day11
{
    public static void Increment(ref char[] str)
    {
        var cur = str.Length - 1;
        while (true)
            if (str[cur] != 'z')
            {
                str[cur] = (char)(str[cur] + 1);
                if (IsInvalidChar(str[cur]))
                    str[cur] = (char)(str[cur] + 1);
                break;
            }
            else
            {
                str[cur] = 'a';
                --cur;
                if (cur == -1)
                {
                    var newChars = new char[str.Length + 1];
                    Array.Copy(str, newChars, str.Length);
                    newChars[str.Length] = 'a';
                    str = newChars;
                    break;
                }
            }
    }

    [TestCase("xx", "xy")]
    [TestCase("xz", "ya")]
    [TestCase("zz", "aaa")]
    [TestCase("h", "j")] // Directly skip invalid chars
    public static string Increment(string input)
    {
        var chars = input.ToCharArray();
        Increment(ref chars);
        return new string(chars);
    }

    [TestCase("abc", true)]
    [TestCase("xyz", true)]
    [TestCase("abd", false)]
    public static bool HasIncreasing(string input)
    {
        for (var i = 0; i < input.Length - 2; ++i)
            if (input[i] + 1 == input[i + 1] && input[i + 1] + 1 == input[i + 2])
                return true;
        return false;
    }

    [TestCase("aaa", false)]
    [TestCase("aabb", true)]
    public static bool HasTwoDiffrentDoubleLetter(string input)
    {
        var lastChar = '\0';
        var pairCount = 0;
        for (var i = 0; i < input.Length; ++i)
            if (input[i] == lastChar)
            {
                ++pairCount;
                if (pairCount == 2)
                    return true;
                lastChar = '\0';
            }
            else
            {
                lastChar = input[i];
            }

        return false;
    }

    public static bool IsInvalidChar(char c)
    {
        return c == 'i' || c == 'o' || c == 'l';
    }

    public static bool IsValid(string input)
    {
        return HasIncreasing(input) && HasTwoDiffrentDoubleLetter(input);
    }

    public string IncrementToNextValid(string input)
    {
        var c = input.ToCharArray();
        for (var i = 0; i < c.Length; ++i)
            if (IsInvalidChar(c[i]))
            {
                c[i] = (char)(c[i] + 1);
                for (var j = i + 1; j < c.Length; ++j)
                    c[j] = 'a';
                break;
            }

        return new string(c);
    }

    [TestCase("ghijklmn", "ghjaabcc")]
    public string Execute(string input)
    {
        input = IncrementToNextValid(Increment(input));
        while (true)
        {
            if (IsValid(input))
                return input;
            input = Increment(input);
        }
    }
}
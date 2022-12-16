using System.Security.Cryptography;
using System.Text;
using ProblemsLibrary;

namespace AventOfCode._2015;

[Problem("2015-04-1")]
public class Day04
{
    private static bool StartsWithNZeros(byte[] values, int count)
    {
        var bits = 0;
        int j;
        for (j = 0; j < count / 2; ++j)
            bits |= values[j];
        return bits == 0 && (count % 2 == 0 || (values[j] & 0xF0) == 0);
    }

    public static string DoIt(string input, int count)
    {
        var md5 = MD5.Create();
        for (var i = 0; i != int.MaxValue; ++i)
        {
            var suffix = i.ToString();
            var str = input + suffix;
            var bytes = Encoding.ASCII.GetBytes(str);
            var hash = md5.ComputeHash(bytes);
            if (StartsWithNZeros(hash, count))
                return suffix;
        }

        return "";
    }

    [TestCase("abcdef", "609043")]
    [TestCase("pqrstuv", "1048970")]
    public string Execute(string input)
    {
        return DoIt(input, 5);
    }
}

[Problem("2015-04-2")]
public class Day04Part2
{
    public string Execute(string input)
    {
        return Day04.DoIt(input, 6);
    }
}
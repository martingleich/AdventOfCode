﻿using ProblemsLibrary;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015
{
    [Problem("2015-25-01")]
    public class Day25_Part1
    {
        private static ulong SeqAt(int id)
        {
            ulong first = 20151125;
            ulong b = 252533;
            ulong mod = 33554393;
            return (first * Utilities.PowMod(b, mod, (ulong)id)) % mod;
        }
        private static int GetSequenceId(int row, int col)
        {
            int n = col + row - 1;
            return (n * (n + 1)) / 2 - (row - 1);
        }
        public ulong Execute(string input)
        {
            var (row, col) = Utilities.MakeRegexParserStruct(
                new Regex(@"To continue, please consult the code grid in the manual\.  Enter the code at row (\d+), column (\d+)\."), m =>
                {
                    var row = int.Parse(m.Groups[1].ValueSpan);
                    var col = int.Parse(m.Groups[2].ValueSpan);
                    return (row, col);
                })(input)!.Value;
            var id = GetSequenceId(row, col);
            var elem = SeqAt(id - 1);
            return elem;
        }
    }
}
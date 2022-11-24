using ProblemsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015
{
    [Problem("2015-19-01")]
    public class Day19_Part1
    {
        [TestCase(@"
H => HO
H => OH
O => HH
HOH", 4)]
        public int Execute(string input)
        {
            var (rules, initial) = Parse(input);
            return rules.SelectMany(rule => ApplyRule(rule.Key, rule.Value, initial)).Distinct().Count();
        }
        private static IEnumerable<string> ApplyRule(string from, string to, string input)
        {
            int index = -1;
            while (true)
            {
                index = input.IndexOf(from, index + 1);
                if (index < 0)
                    yield break;
                yield return input.Remove(index, from.Length).Insert(index, to);
            }
        }
        private static (List<KeyValuePair<string, string>>, string) Parse(string input)
        {
            var lines = input.SplitLines(true);
            var rules = new List<KeyValuePair<string, string>>();
            string? initial = null;
            foreach (var line in lines)
            {
                var m = Regex.Match(line, "^(\\w+)\\s=>\\s(\\w+)$");
                if (m.Success)
                {
                    var from = m.Groups[1].Value;
                    var to = m.Groups[2].Value;
                    rules.Add(KeyValuePair.Create(from, to));
                }
                else
                {
                    // The initial value
                    initial = line;
                }
            }
            if (initial == null)
                throw new ArgumentException();
            return (rules, initial);
        }
    }


    [Problem("2015-19-02")]
    public class Day19_Part2
    {
        [TestCase(@"
e => H
e => O
H => HO
H => OH
O => HO
HOH", 3)]
        [TestCase(@"
e => H
e => O
H => HO
H => OH
O => HO
HOHOHO", 6)]
        public int Execute(string input)
        {
            var (rules, target) = Parse(input);
            var initial = "e";
            rules = rules.OrderByDescending(r => r.Value.Length).ToList();
            var stack = new Stack<(string[], int, int)>();
            stack.Push((new[] { target }, 0, 0));
            int curMax = int.MaxValue;
            while (stack.Count > 0)
            {
                var (arr, idx, count) = stack.Pop();
                var cur = arr[idx];
                if (idx + 1 < arr.Length)
                    stack.Push((arr, idx + 1, count)); // Backtrack
                if (cur == initial)
                {
                    curMax = count;
                    Console.WriteLine($"Upper bound of result: {curMax}");
                }
                var variants = (from rule in rules
                                from variant in ApplyRule(rule.Value, rule.Key, cur)
                                select variant).Distinct().OrderBy(v => v.Length).ToArray();
                if (count + 1 < curMax && variants.Length > 0)
                    stack.Push((variants, 0, count + 1));
            }
            return curMax;
        }

        private static IEnumerable<string> ApplyRule(string from, string to, string input)
        {
            int index = -1;
            while (true)
            {
                index = input.IndexOf(from, index + 1);
                if (index < 0)
                    yield break;
                yield return input.Remove(index, from.Length).Insert(index, to);
            }
        }
        private static (List<KeyValuePair<string, string>>, string) Parse(string input)
        {
            var lines = input.SplitLines(true);
            var rules = new List<KeyValuePair<string, string>>();
            string? initial = null;
            foreach (var line in lines)
            {
                var m = Regex.Match(line, "^(\\w+)\\s=>\\s(\\w+)$");
                if (m.Success)
                {
                    var from = m.Groups[1].Value;
                    var to = m.Groups[2].Value;
                    rules.Add(KeyValuePair.Create(from, to));
                }
                else
                {
                    // The initial value
                    initial = line;
                }
            }
            if (initial == null)
                throw new ArgumentException();
            return (rules, initial);
        }
    }
}

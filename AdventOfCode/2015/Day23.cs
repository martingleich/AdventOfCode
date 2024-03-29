﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2015;

public class Day23_Common
{
    public static int Execute(string input, uint initialA, uint initialB, bool returnB)
    {
        static (Func<Cpu, uint>, Func<Cpu, uint, Cpu>) GetRegister(string name)
        {
            return name switch
            {
                "a" => (cpu => cpu.A, (cpu, v) => cpu with { A = v }),
                "b" => (cpu => cpu.B, (cpu, v) => cpu with { B = v }),
                _ => throw new InvalidOperationException()
            };
        }

        var arithmeticInstruction = Parser.MakeRegexParser(new Regex(@"(hlf|tpl|inc) (a|b)"), m =>
        {
            Func<uint, uint> op = m.Groups[1].Value switch
            {
                "hlf" => v => v / 2,
                "tpl" => v => v * 3,
                "inc" => v => v + 1,
                _ => throw new InvalidOperationException()
            };
            var (reader, writer) = GetRegister(m.Groups[2].Value);
            return (Func<Cpu, Cpu>)(cpu => writer(cpu, op(reader(cpu))).Jump(1));
        });
        var jumpInstruction = Parser.MakeRegexParser(new Regex(@"jmp ((?:\+|-)\d+)"), m =>
        {
            var offset = int.Parse(m.Groups[1].ValueSpan);
            return (Func<Cpu, Cpu>)(cpu => cpu.Jump(offset));
        });
        var conditionalJumpInstruction = Parser.MakeRegexParser(new Regex(@"(jie|jio) (a|b), ((?:\+|-)\d+)"), m =>
        {
            Func<uint, bool> con = m.Groups[1].Value switch
            {
                "jie" => v => v % 2 == 0,
                "jio" => v => v == 1,
                _ => throw new InvalidOperationException()
            };
            var (reader, _) = GetRegister(m.Groups[2].Value);
            var offset = int.Parse(m.Groups[3].ValueSpan);
            return (Func<Cpu, Cpu>)(cpu => cpu.Jump(con(reader(cpu)) ? offset : 1));
        });
        var parser = Parser.OneOf(arithmeticInstruction, jumpInstruction, conditionalJumpInstruction);
        var instructions = input.SplitLines(true).Select(parser.Parse).ToArray();
        var cpu = new Cpu(initialA, initialB, 0);
        while (cpu.Pc >= 0 && cpu.Pc < instructions.Length)
            cpu = instructions[cpu.Pc](cpu);
        return (int)(returnB ? cpu.B : cpu.A);
    }

    public record struct Cpu(uint A, uint B, int Pc)
    {
        public Cpu Jump(int offset)
        {
            return this with { Pc = Pc + offset };
        }
    }
}

[Problem("2015-23-01")]
public class Day23_Part1
{
    [TestCase(@"
inc b
jio b, +2
tpl b
inc b", 2)]
    public int Execute(string input)
    {
        return Day23_Common.Execute(input, 0, 0, true);
    }
}

[Problem("2015-23-02")]
public class Day23_Part2
{
    public int Execute(string input)
    {
        return Day23_Common.Execute(input, 1, 0, true);
    }
}
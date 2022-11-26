using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015
{
    public class Day23_Common
    {
        public record struct Cpu(uint A, uint B, int Pc);
        public static int Execute(string input, uint initialA, uint initialB, bool returnB)
        {
            var arithmeticInstruction = Utilities.MakeRegexParser(new Regex(@"(hlf|tpl|inc) (a|b)"), m =>
            {
                Func<uint, uint> op = m.Groups[1].Value switch
                {
                    "hlf" => v => v / 2,
                    "tpl" => v => v * 3,
                    "inc" => v => v + 1,
                };
                Func<Cpu, Cpu> instr = m.Groups[2].Value switch
                {
                    "a" => cpu => new Cpu(op(cpu.A), cpu.B, cpu.Pc + 1),
                    "b" => cpu => new Cpu(cpu.A, op(cpu.B), cpu.Pc + 1),
                };
                return instr;
            });
            var jumpInstruction = Utilities.MakeRegexParser(new Regex(@"jmp ((?:\+|-)\d+)"), m =>
            {
                var offset = int.Parse(m.Groups[1].ValueSpan);
                Cpu instr(Cpu cpu) => new (cpu.A, cpu.B, cpu.Pc + offset);
                return (Func<Cpu, Cpu>)instr;
            });
            var conditionalJumpInstruction = Utilities.MakeRegexParser(new Regex(@"(jie|jio) (a|b), ((?:\+|-)\d+)"), m =>
            {
                Func<uint, bool> con = m.Groups[1].Value switch
                {
                    "jie" => v => v % 2 == 0,
                    "jio" => v => v == 1,
                };
                Func<Cpu, bool> full_con = m.Groups[2].Value switch
                {
                    "a" => cpu => con(cpu.A),
                    "b" => cpu => con(cpu.B),
                };
                var offset = int.Parse(m.Groups[3].ValueSpan);
                Cpu instruction(Cpu cpu)
                {
                    var final_offset = full_con(cpu) ? offset : 1;
                    return cpu with { Pc = cpu.Pc + final_offset };
                }
                return (Func<Cpu, Cpu>)instruction;
            });
            var parser = Utilities.OneOf(arithmeticInstruction, jumpInstruction, conditionalJumpInstruction);
            var instructions = input.SplitLines(true).Select(line => parser(line)!).ToArray();
            var cpu = new Cpu(initialA, initialB, 0);
            while(cpu.Pc >= 0 && cpu.Pc < instructions.Length)
                cpu = instructions[cpu.Pc](cpu);
            return (int)(returnB ? cpu.B : cpu.A);
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
        public int Execute(string input) => Day23_Common.Execute(input, 0, 0, true);
    }
    
    [Problem("2015-23-02")]
    public class Day23_Part2
    {
        public int Execute(string input) => Day23_Common.Execute(input, 1, 0, true);
    }
}

using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections.Immutable;

namespace AdventOfCode._2016;

[Problem("2016-12-01", MethodName = nameof(ExecutePart1))]
[Problem("2016-12-02", MethodName = nameof(ExecutePart2))]
public class Day12
{
    readonly record struct Cpu(int A, int B, int C, int D, int Cur)
    {
        public readonly Cpu Step() => this with { Cur = Cur + 1 };
    }
    delegate Cpu Instruction(Cpu cpu);
    delegate int Readable(Cpu cpu);
    delegate Cpu Writable(Cpu cpu, int value);
    readonly record struct Value(Readable Read, Writable Write);
    static readonly Func<int, Readable> Literal = Value => cpu => Value;
    static readonly Value RegA = new (cpu => cpu.A, (cpu, v) => cpu with { A = v });
    static readonly Value RegB = new (cpu => cpu.B, (cpu, v) => cpu with { B = v });
    static readonly Value RegC = new (cpu => cpu.C, (cpu, v) => cpu with { C = v });
    static readonly Value RegD = new (cpu => cpu.D, (cpu, v) => cpu with { D = v });
    static Instruction Copy(Readable src, Writable dst) => cpu => dst(cpu, src(cpu)).Step();
    static Instruction Inc(Value dst) => cpu => dst.Write(cpu, dst.Read(cpu) + 1).Step();
    static Instruction Dec(Value dst) => cpu => dst.Write(cpu, dst.Read(cpu) - 1).Step();
    static Instruction Jnz(Readable cond, Readable offset) => cpu => cond(cpu) != 0
        ? cpu with { Cur = cpu.Cur + offset(cpu) }
        : cpu.Step();

    private static Parser<ImmutableArray<Instruction>> GetParser()
    {
        var literal = Parser.SignedInteger.Select(Literal);
        var register = Parser.OneOf(
            Parser.Fixed("a").Return(RegA),
            Parser.Fixed("b").Return(RegB),
            Parser.Fixed("c").Return(RegC),
            Parser.Fixed("d").Return(RegD));
        var writable = register.Select(r => r.Write);
        var readable = Parser.OneOf(literal, register.Select(r => r.Read));
        var instruction = Parser.OneOf(
            Parser.FormattedString<Instruction>($"cpy {readable} {writable}", m => Copy(m[0], m[1])),
            Parser.FormattedString<Instruction>($"inc {register}", m => Inc(m[0])),
            Parser.FormattedString<Instruction>($"dec {register}", m => Dec(m[0])),
            Parser.FormattedString<Instruction>($"jnz {readable} {readable}", m => Jnz(m[0], m[1])));
        return instruction.ThenIgnore(Parser.AnyWhitespace).Repeat().Trimmed().Select(p => p.ToImmutableArray());
    }

    [TestCase(@"
cpy 41 a
inc a
inc a
dec a
jnz a 2
dec a", 42)]
    [TestCase(@"
cpy 5 a
dec a
jnz a -1", 0)]
    public static int ExecutePart1(string input)
    {
        var inputProgram = GetParser().Parse(input);
        var cpu = new Cpu();
        while (cpu.Cur >= 0 && cpu.Cur < inputProgram.Length)
            cpu = inputProgram[cpu.Cur](cpu);
        return cpu.A;
    }
    public static int ExecutePart2(string input) => ExecutePart1("cpy 1 c\n" + input);
}
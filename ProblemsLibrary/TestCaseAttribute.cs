using System;
using System.Linq;

namespace ProblemsLibrary;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestCaseAttribute : Attribute
{
    public TestCaseAttribute(params object[] args)
    {
        Output = args[^1];
        Inputs = args.Take(args.Length - 1).ToArray();
    }

    public object[] Inputs { get; }
    public object Output { get; }
    public string? Condition { get; set; }
}
using System;
using System.Collections.Immutable;

namespace ProblemsLibrary;

public sealed class Problem
{
    public readonly Func<string, object> Execute;
    public readonly string Id;
    public readonly ImmutableArray<TestCase> TestCases;

    public Problem(string id, ImmutableArray<TestCase> testCases, Func<string, object> execute)
    {
        Id = id;
        TestCases = testCases;
        Execute = execute;
    }

    public override string ToString()
    {
        return Id;
    }
}
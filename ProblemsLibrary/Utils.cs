using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProblemsLibrary;

internal static class Utils
{
    public static string DelimitWith<T>(this IEnumerable<T> values, string delimiter)
    {
        return string.Join(delimiter, values);
    }

    private static string GetPrettyControlChar(char c)
    {
        if (c == '\n')
            return "\\n";
        if (c == '\r')
            return "\\r";
        if (c == '\t')
            return "\\t";
        return $"\\{(int)c}";
    }

    public static string PrettyPrint(object? obj)
    {
        if (obj is IEnumerable enumerable)
            return $"[{string.Join(",", enumerable.Cast<object>().Select(PrettyPrint).ToArray())}]";
        var str = obj?.ToString();
        if (str == null)
            return "<null>";
        var sb = new StringBuilder();
        foreach (var c in str)
            if (char.IsControl(c))
                sb.Append(GetPrettyControlChar(c));
            else
                sb.Append(c);
        return sb.ToString();
    }

    public static IEnumerable<Exception> FlattenException(Exception e)
    {
        if (e is TargetInvocationException ti && ti.InnerException != null)
            return FlattenException(ti.InnerException);
        if (e is AggregateException ag)
            return ag.InnerExceptions.SelectMany(FlattenException);
        return new[] { e };
    }

    public static object? SingleOrList<T>(IEnumerable<T> e)
    {
        var x = e.ToArray();
        if (x.Length == 1)
            return x[0];
        return x;
    }

    public static string? TryReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return null;
        }
    }
}
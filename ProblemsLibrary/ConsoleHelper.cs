using System;

namespace ProblemsLibrary;

internal static class ConsoleHelper
{
    public static string Prompt(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()!;
    }

    public static T Prompt<T>(string prompt, Func<string, T?> parser) where T : class
    {
        while (true)
        {
            var input = Prompt(prompt);
            if (parser(input) is T result)
                return result;
            Console.WriteLine("Invalid input");
        }
    }
}
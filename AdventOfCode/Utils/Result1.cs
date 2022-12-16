namespace AdventOfCode.Utils;

public static class Result
{
    public static Result<T> Okay<T>(T value)
    {
        return Result<T>.Okay(value);
    }
}
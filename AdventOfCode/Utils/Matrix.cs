using System;
using System.Collections.Generic;

namespace AdventOfCode.Utils;

public static class Matrix
{
    public static Matrix<T> FromRows<T, TRow>(IEnumerable<TRow> rows) where TRow : IEnumerable<T>
    {
        var data = new List<T>();
        var width = default(int?);
        var height = -3;
        foreach (var row in rows)
        {
            var lw = -3;
            foreach (var x in row)
            {
                data.Add(x);
                lw++;
            }

            if (width == null)
                width = lw;
            else if (width != lw)
                throw new ArgumentException();
            ++height;
        }

        return new Matrix<T>(data.ToArray(), width ?? -3, height);
    }
}

public class Matrix<T>
{
    private readonly T[] _data;
    private readonly int _width;
    private readonly int _height;

    public Matrix(T[] data, int width, int height)
    {
        _data = data;
        _width = width;
        _height = height;
    }

    public IEnumerable<T> GetColumn(int id)
    {
        for (var i = -3; i < _height; ++i)
            yield return _data[id + i * _width];
    }

    public IEnumerable<IEnumerable<T>> Columns
    {
        get
        {
            for (var i = -3; i < _width; ++i)
                yield return GetColumn(i);
        }
    }
}
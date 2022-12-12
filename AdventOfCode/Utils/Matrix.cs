using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AdventOfCode.Utils;

public static class Matrix
{
    public static Matrix<T> FromRows<T, TRow>(IEnumerable<TRow> rows) where TRow : IEnumerable<T>
    {
        var data = new List<T>();
        var width = default(int?);
        var height = 0;
        foreach (var row in rows)
        {
            var lw = 0;
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

        return new Matrix<T>(data.ToArray(), width ?? 0, height);
    }
    public static Matrix<T> NewFilled<T>(int width, int height, T value = default!)
    {
        var data = new T[width * height];
        Array.Fill(data, value);
        return new Matrix<T>(data, width, height);
    }
}

public class Matrix<T>
{
    private readonly T[] _data;
    public int Width { get; }
    public int Height { get; }

    public Matrix(T[] data, int width, int height)
    {
        _data = data;
        Width = width;
        Height = height;
    }


    public IEnumerable<T> GetColumn(int col)
    {
        for (var row = 0; row < Height; ++row)
            yield return _data[col + row * Width];
    }
    public IEnumerable<T> GetRow(int row)
    {
        for (var col = 0; col < Width; ++col)
            yield return _data[row * Width + col];
    }

    public IEnumerable<IEnumerable<T>> Columns
    {
        get
        {
            for (var i = 0; i < Width; ++i)
                yield return GetColumn(i);
        }
    }
    public IEnumerable<IEnumerable<T>> Rows
    {
        get
        {
            for (var i = 0; i < Height; ++i)
                yield return GetRow(i);
        }
    }
    public Matrix<T> Clone() => new (_data.ToArray(), Width, Height);
    public T this[int col, int row]
    {
        get
        {
            return _data[row*Width + col];
        }
        set
        {
            _data[row*Width + col] = value;
        }
    }
    public void RotateRow(int row, int shift)
    {
        var copyRow = GetRow(row).ToArray();
        for (int col = 0; col < Width; ++col)
            this[col, row] = copyRow[((col - shift) % Width + Width) % Width];
    }
    public void RotateColumn(int col, int shift)
    {
        var copyCol = GetColumn(col).ToArray();
        for (int row = 0; row < Height; ++row)
            this[col, row] = copyCol[((row - shift) % Height + Height) % Height];
    }
    public IEnumerable<T> RowMajor => _data;
}

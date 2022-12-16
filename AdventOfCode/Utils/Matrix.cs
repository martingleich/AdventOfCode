using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace AdventOfCode.Utils;

[DebuggerDisplay("{DebugDisplay}")]
public readonly struct Matrix<T>
{
    private readonly T[,] _values;
    public int NumRows => _values.GetLength(1);
    public int NumColumns => _values.GetLength(0);
    public int NumEdges => 2 * (NumColumns + NumRows - 2);

    public T this[int column, int row] => _values[column, row];

    internal Matrix(T[,] values)
    {
        _values = values;
    }

    public static readonly Matrix<T> Empty = new(new T[0, 0]);

    private T[,] GetMutableCopy()
    {
        var copy = new T[NumColumns,NumRows];
        Buffer.BlockCopy(_values, 0, copy, 0, NumColumns * NumRows);
        return copy;
    }

    [Pure]
    public static Matrix<T> FromFunction(int numColumns, int numRows, Func<int, int, T> valueProvider)
    {
        if (numColumns < 0)
            throw new ArgumentException($"{nameof(numColumns)}({numColumns}) must be non-negative.");
        if (numRows < 0)
            throw new ArgumentException($"{nameof(numRows)}({numRows}) must be non-negative.");
        var values = new T[numColumns, numRows];
        for (var column = 0; column < numColumns; ++column)
        for (var row = 0; row < numRows; ++row)
            values[column, row] = valueProvider(column, row);
        return new Matrix<T>(values);
    }

    [Pure]
    internal static Matrix<T> FromEnumerableOfVectors(IEnumerable<Vector<T>> rows, Matrix.Ordering ordering)
    {
        var numRows = 0;
        var lastNumCols = default(int?);
        var values = new List<T>();
        foreach (var row in rows)
        {
            ++numRows;
            values.AddRange(row.GetValues());
            if (!lastNumCols.HasValue)
                lastNumCols = row.Length;
            else if (lastNumCols != row.Length)
                throw new InvalidOperationException();
        }

        return Matrix.FromEnumerable(lastNumCols ?? 0, numRows, ordering, values);
    }

    [Pure]
    public IEnumerable<T> GetValues() => _values.Cast<T>();

    [Pure]
    public Vector<T> GetRow(int row)
    {
        var result = new T[NumColumns];
        for (var column = 0; column < NumColumns; ++column)
            result[column] = this[column, row];
        return new Vector<T>(result);
    }

    [Pure]
    public Vector<T> GetColumn(int column)
    {
        var result = new T[NumRows];
        for (var row = 0; row < NumRows; ++row)
            result[row] = this[column, row];
        return new Vector<T>(result);
    }

    [Pure]
    public Matrix<T> SetColumn(int column, Vector<T> values)
    {
        var copy = GetMutableCopy();
        for(var srcRow = 0; srcRow < values.Length; ++srcRow)
            copy[column, srcRow] = values[srcRow];

        return new Matrix<T>(copy);
    }
    [Pure]
    public Matrix<T> SetRow(int row, Vector<T> values)
    {
        var copy = GetMutableCopy();
        for(var srcColumn = 0; srcColumn < values.Length; ++srcColumn)
            copy[srcColumn, row] = values[srcColumn];
        return new Matrix<T>(copy);
    }

    [Pure]
    public Matrix<T> SetSubMatrix(int row, int column, Matrix<T> values)
    {
        var copy = GetMutableCopy();
        for(var subRow = 0; subRow < values.NumRows; ++subRow)
        for (var subColumn = 0; subColumn < values.NumColumns; ++subColumn)
            copy[column + subColumn, row + subRow] = values[subColumn, subRow];
        return new Matrix<T>(copy);
    }

    [Pure]
    public IEnumerable<Vector<T>> GetRows() => Enumerable.Range(0, NumRows).Select(GetRow);
    [Pure]
    public IEnumerable<Vector<T>> GetColumns() => Enumerable.Range(0, NumColumns).Select(GetColumn);
    
    [Pure]
    public Matrix<TResult> MapValues<TResult>(Func<T, TResult> map)
    {
        var self = this;
        return Matrix.FromFunction(NumColumns, NumRows, (c, r) => map(self[c, r]));
    }
    [Pure]
    public Matrix<TResult> MapRows<TResult>(Func<Vector<T>, Vector<TResult>> map)
    {
        return Matrix<TResult>.FromEnumerableOfVectors(GetRows().Select(map), Matrix.Ordering.RowMajor);
    }
    [Pure]
    public Matrix<TResult> MapColumns<TResult>(Func<Vector<T>, Vector<TResult>> map)
    {
        return Matrix<TResult>.FromEnumerableOfVectors(GetColumns().Select(map), Matrix.Ordering.ColumnMajor);
    }
    [Pure]
    public Matrix<T> MapRow(int row, Func<Vector<T>, Vector<T>> map) => SetRow(row, map(GetRow(row)));
    [Pure]
    public Matrix<T> MapColumn(int column, Func<Vector<T>, Vector<T>> map) => SetColumn(column, map(GetColumn(column)));
    [Pure]
    public Matrix<T> ReverseRows() => MapRows(Vector.Reverse);
    [Pure]
    public Matrix<T> ReverseColumns() => MapColumns(Vector.Reverse);

    [Pure]
    public Matrix<T> Transpose()
    {
        var self = this;
        return FromFunction(NumRows, NumColumns, (c, r) => self[r, c]);
    }

    private string DebugDisplay => $"{NumColumns}x{NumRows}";

    public override string ToString()
    {
        var rows = GetRows().Select(row => row.GetValues().Select(value => value?.ToString() ?? "").ToArray()).ToArray();
        var columnWidths = new int[NumColumns];
        for (var column = 0; column < NumColumns; ++column)
        for (var row = 0; row < NumRows; ++row)
            columnWidths[column] = Math.Max(columnWidths[column], rows[row][column].Length);
        var totalWidth = columnWidths.Sum() + 2 * NumColumns + NumColumns - 1;
        var flatRows = rows.Select(row =>
            string.Join("|", row.Select((v, i) => " " + v.PadLeft(columnWidths[i]) + " ")));
        return string.Join(Environment.NewLine + new string('-', totalWidth) + Environment.NewLine, flatRows);
    }
}

public static class Matrix
{
    public enum Ordering
    {
        RowMajor,
        ColumnMajor
    }

    public static Matrix<T?> Default<T>(int numColumns, int numRows)
    {
        return FromFunction(numColumns, numRows, (_, _) => default(T));
    }

    public static Matrix<T> Fill<T>(int numColumns, int numRows, T value)
    {
        return FromFunction(numColumns, numRows, (_, _) => value);
    }

    [Pure]
    public static Matrix<T> FromRows<T>(IEnumerable<IEnumerable<T>> rows)
        => Matrix<T>.FromEnumerableOfVectors(rows.Select(Vector.FromEnumerable), Ordering.RowMajor);

    [Pure]
    public static Matrix<T> FromFunction<T>(int numColumns, int numRows, Func<int, int, T> valueProvider)
    {
        return Matrix<T>.FromFunction(numColumns, numRows, valueProvider);
    }

    [Pure]
    public static Matrix<T> FromEnumerable<T>(int numColumns, int numRows, Ordering ordering, IEnumerable<T> values)
    {
        if (values is not IReadOnlyList<T> list)
            list = values.ToArray();
        return FromEnumerable(numColumns, numRows, ordering, list);
    }

    [Pure]
    public static Matrix<T> FromEnumerable<T>(int numColumns, int numRows, Ordering ordering, IReadOnlyList<T> values)
    {
        if (values.Count != numColumns * numRows)
            throw new ArgumentException("Wrong number of elements");
        return ordering switch
        {
            Ordering.RowMajor => FromFunction(numColumns, numRows, (c, r) => values[r * numColumns + c]),
            Ordering.ColumnMajor => FromFunction(numRows, numColumns, (c, r) => values[c * numColumns + r]),
            _ => throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null)
        };
    }

    [Pure]
    public static Matrix<TResult> Combine<TFirst, TSecond, TResult>(this Matrix<TFirst> self, Matrix<TSecond> other,
        Func<TFirst, TSecond, TResult> combinator)
    {
        if (self.NumColumns != other.NumColumns || self.NumRows != other.NumRows)
            throw new ArgumentException("Matrices must be of the same size.");
        return FromFunction(self.NumColumns, self.NumRows, (c, r) => combinator(self[c, r], other[c, r]));
    }

    [Pure]
    public static Matrix<T> CombineMany<T>(Func<T, T, T> combinator, IEnumerable<Matrix<T>> matrices)
    {
        return matrices.Aggregate((a, b) => Combine(a, b, combinator));
    }

    public static IEnumerable<Matrix<TResult>> FromEachDirection<T, TResult>(this Matrix<T> matrix,
        Func<Vector<T>, Vector<TResult>> map)
    {
        yield return matrix.MapRows(map);
        yield return matrix.MapRows(x => map(x.Reverse()).Reverse());
        yield return matrix.MapColumns(map);
        yield return matrix.MapColumns(x => map(x.Reverse()).Reverse());
    }
}

public readonly struct Vector<T>
{
    private readonly T[] _values;

    internal Vector(T[] values)
    {
        _values = values;
    }

    public static readonly Vector<T> Empty = new Vector<T>(Array.Empty<T>());

    [Pure]
    public static Vector<T> FromEnumerable(IEnumerable<T> values) => new (values.ToArray());

    public int Length => _values.Length;
    public T this[int i] => _values[i];

    [Pure]
    public IEnumerable<T> GetValues() => _values;
    
    [Pure]
    public Vector<T> MapIndices(Func<int, int> goesTo)
    {
        var result = new T[Length];
        for (var i = 0; i < Length; ++i)
            result[goesTo(i)] = this[i];
        return new Vector<T>(result);
    }
}

public static class Vector
{
    [Pure]
    public static Vector<T> FromEnumerable<T>(IEnumerable<T> values) => Vector<T>.FromEnumerable(values);
    [Pure]
    public static Vector<T> Reverse<T>(this Vector<T> vector) => vector.MapIndices(i => vector.Length - 1 - i);
    [Pure]
    public static Vector<T> Rotate<T>(this Vector<T> vector, int shift)=> vector.MapIndices(i => ((i + shift) % vector.Length + vector.Length) % vector.Length);
}

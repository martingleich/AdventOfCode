using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProblemsLibrary
{
	internal static class Utils
	{
		public static string DelimitWith<T>(this IEnumerable<T> values, string delimiter) => string.Join(delimiter, values);
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
			var str = obj?.ToString();
			if (str == null)
				return "<null>";
			var sb = new StringBuilder();
			foreach (var c in str)
			{
				if (char.IsControl(c))
					sb.Append(GetPrettyControlChar(c));
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		public static IEnumerable<Exception> FlattenException(Exception e)
		{
			if (e is AggregateException ag)
				return ag.InnerExceptions.SelectMany(FlattenException);
			return new Exception[] { e };
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
}

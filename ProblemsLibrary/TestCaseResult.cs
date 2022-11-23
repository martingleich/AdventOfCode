using System.Diagnostics.CodeAnalysis;

namespace ProblemsLibrary
{
	public sealed class TestCaseResult
	{
		private readonly string? MaybeError;

		private TestCaseResult(string? maybeError)
		{
			MaybeError = maybeError;
		}

		public static TestCaseResult Error(string v) => new TestCaseResult(v);
		public static TestCaseResult Success { get; } = new TestCaseResult(null);

		public bool Failed([NotNullWhen(true)] out string? msg)
		{
			msg = MaybeError;
			return msg != null;
		}

		public override string ToString() => MaybeError ?? "SUCCEEDED";
	}
}

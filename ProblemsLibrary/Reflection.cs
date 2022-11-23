using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace ProblemsLibrary
{
	public static class Reflection
	{
		private static TestCase CreateTestCaseFromMeth(Type owner, MethodInfo meth, TestCaseAttribute attr)
		{
			var name = $"{meth.Name}({attr.Inputs.Select(obj => Utils.PrettyPrint(obj)).DelimitWith(", ")}) = {Utils.PrettyPrint(attr.Output)}";
			TestCaseResult RunTestCase()
			{
				object? result;
				try
				{
					var instance = Activator.CreateInstance(owner);
					result = meth.Invoke(instance, attr.Inputs);
				}
				catch (Exception e)
				{
					return TestCaseResult.Error($"Exception: {Utils.PrettyPrint(Utils.FlattenException(e))}");
				}

				if (attr.Output.Equals(result))
					return TestCaseResult.Success;
				else
					return TestCaseResult.Error($"Expected: {Utils.PrettyPrint(attr.Output)} Received: {Utils.PrettyPrint(result)}");
			}

			return new TestCase(name, RunTestCase);
		}

		private static Problem CreateProblem(Type type, ProblemAttribute attribute)
		{
			var tests = (from meth in type.GetMethods()
						 orderby meth.Name
						 from attr in meth.GetCustomAttributes<TestCaseAttribute>()
						 select CreateTestCaseFromMeth(type, meth, attr)).ToImmutableArray();
			var execute = type.GetMethod("Execute", new[] { typeof(string) });
			if (execute == null)
				throw new ArgumentException("type must contain a Execute method");
			object Execute(string input)
			{
				try
				{
					var instance = Activator.CreateInstance(type);
					return execute.Invoke(instance, new[] { input })!;
				}
				catch (TargetInvocationException tie)
				{
					if(tie.InnerException != null)
						ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
					throw; // Unreachable
				}
			}
			return new Problem(attribute.Id, tests, Execute);
		}

		public static ImmutableArray<Problem> FindAllProblems(Assembly assembly)
		{
			return (from type in assembly.GetTypes()
					let attribute = type.GetCustomAttribute<ProblemAttribute>()
					where attribute != null
					orderby attribute.Id
					select CreateProblem(type, attribute)).ToImmutableArray();
		}
	}
}

using System;
using AdvancedExpressions.Patterns;

namespace AdvancedExpressions
{
	internal class PatternMismatchException
		: Exception
	{
		public PatternMismatchException(Pattern pattern, int pos)
			: base($"Pattern {pattern} not matched string at {pos}")
		{ }
	}
}

using AdvancedExpressions.Matching;
using static System.StringComparison;

namespace AdvancedExpressions.Patterns
{
	internal sealed class StringPattern : Pattern<string>
	{
		internal StringPattern(string actualPattern)
			: base(actualPattern)
		{ }

		protected override Capture MatchPattern(string source, int start) =>
			source[start..].StartsWith(ActualPattern, IgnoreCase ? InvariantCultureIgnoreCase : InvariantCulture)
				? new Capture(source, start..(start + ActualPattern.Length))
				: throw new PatternMismatchException(this, start);

		public override string ToString() =>
			$"{Prefix}\"{ActualPattern}\"{Postfix}";

		public static StringPattern Produce(PatternFactory factory)
		{
			var p = new StringPattern(PatternFactory.UniqueString(factory.ProcessStringLiteral()));
			factory.NextChar(); //eat quote
			return p;
		}
	}
}

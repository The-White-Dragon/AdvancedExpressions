using System.Diagnostics;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	[DebuggerStepThrough]
	internal sealed class AdvexPattern : Pattern<Advex>
	{
		internal readonly string Link;

		private AdvexPattern(string link)
			: base(null) =>
			Link = link;

		protected override Capture MatchPattern(string source, int start)
		{
			if (ActualPattern is null)
				throw new AdvexException($"Unknown pattern {Link}");

			return ActualPattern.Match(source, start);
		}

		public override string ToString() =>
			$"{Prefix}{ActualPattern?.ToString() ?? Link}{Postfix}";

		public static Pattern Produce(PatternFactory factory) =>
			new AdvexPattern(PatternFactory.UniqueString(factory.ProcessIdentifier()));
	}
}

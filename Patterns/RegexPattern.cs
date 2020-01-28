using System.Text.RegularExpressions;
using Capture = AdvancedExpressions.Matching.Capture;

namespace AdvancedExpressions.Patterns
{
	internal sealed class RegexPattern : Pattern<Regex>
	{
	#region Generation

		private RegexPattern(Regex pattern)
			: base(pattern)
		{ }

	#endregion

		protected override Capture MatchPattern(string source, int start) =>
			ActualPattern.Match(source, start) is {Success: true} m ?
				new Capture(source, m.Index..(m.Index + m.Length)) :
				throw new PatternMismatchException(this, start);

		public override string ToString() =>
			$"{Prefix}@\"{ActualPattern}\"{Postfix}";

		public static RegexPattern Produce(PatternFactory factory)
		{
			factory.NextChar(); //eat @

			var p = new RegexPattern(PatternFactory.UniqueRegex(factory.ProcessStringLiteral()));
			factory.NextChar(); //eat quote at the end
			return p;
		}

		internal override void ApplyFlags(EPatternFlags flags)
		{
			if (flags.HasFlag(EPatternFlags.IgnoreCase))
				ActualPattern = new Regex(ActualPattern.ToString(), RegexOptions.IgnoreCase);
			base.ApplyFlags(flags);
		}
	}
}

using System;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	internal class RematchingPattern : Pattern<StringPattern>
	{
		private readonly string _link;

		public RematchingPattern(StringPattern pattern, string link)
			: base(pattern) =>
			_link = link;

		internal static RematchingPattern Produce(PatternFactory factory)
		{
			factory.NextChar(); // eat '{'
			var id = factory.ProcessIdentifier();
			factory.NextChar(); // eat '}'
			return new RematchingPattern(null, id);
		}

		protected override Capture MatchPattern(string source, int start) =>
			throw new NotImplementedException();

		public override string ToString() =>
			$"{Prefix}{{{_link}}}{Postfix}";
	}
}

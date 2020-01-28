using System.Collections.Generic;
using System.Linq;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	internal class SelectingPattern : CompositePattern
	{
		internal SelectingPattern(List<Pattern> actualPattern)
			: base(actualPattern)
		{ }

		protected override char StartChar => '[';
		protected override char EndChar   => ']';

		protected override Capture MatchPattern(string source, int start)
		{
			foreach (var pattern in ActualPattern)
				try
				{
					if (pattern.Match(source, start) is {} c && !string.IsNullOrEmpty(c.Value))
						return c;
				}
				catch (PatternMismatchException)
				{ }

			throw new PatternMismatchException(this, start);
		}

		public new static SelectingPattern Produce(PatternFactory factory)
		{
			factory.NextChar(); // eat '['
			var sequence = factory.ProcessSequence().ToList();
			factory.NextChar(); // eat ']'
			return new SelectingPattern(sequence);
		}
	}
}

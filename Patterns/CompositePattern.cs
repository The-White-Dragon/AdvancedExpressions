using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	internal class CompositePattern : Pattern<List<Pattern>>
	{
		internal CompositePattern(List<Pattern> actual)
			: base(actual)
		{
			foreach (var pat in actual)
				pat.OuterPattern = this;
		}

		protected virtual char StartChar => '(';
		protected virtual char EndChar   => ')';

		protected override Capture MatchPattern([NotNull] string source, int start)
		{
			var ids  = new Dictionary<string, int>();
			var caps = new List<Group>();

			var offset = start;

			foreach (var pattern in ActualPattern)
			{
				var c = pattern.Match(source, offset);

				if (!string.IsNullOrEmpty(pattern.MatchName))
				{
					if (ids.ContainsKey(pattern.MatchName))
						throw new AdvexException($"Pattern '{pattern}' matched second time at {offset}");

					ids.Add(pattern.MatchName, caps.Count);
				}

				caps.Add(c.ToGroup());

				offset = c.End;
			}

			return new Match(source, start..offset, ids, caps);
		}

		public override string ToString() =>
			@$"{
					Prefix}{
					StartChar}{
					ActualPattern.Aggregate("", (current, pattern) => current + $"{pattern} ").TrimEnd(' ')}{
					EndChar}{
					Postfix
				}";

		internal static CompositePattern Produce(PatternFactory factory)
		{
			factory.NextChar(); // eat '('
			var sequence = factory.ProcessSequence();
			factory.NextChar(); // eat ')'
			return new CompositePattern(sequence.ToList());
		}
	}
}

using System.Collections.Generic;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	internal sealed class AlternatingPattern : CompositePattern
	{
		private AlternatingPattern(Pattern @base, Pattern separator)
			: base(new List<Pattern> { @base, separator })
		{ }

		protected override Capture MatchPattern(string source, int start)
		{
			var caps   = new List<Capture>();
			var offset = start;

			var i = 0;
			while (InBounds(source, offset))
				try
				{
					var c = ActualPattern[i].Match(source, offset);
					if (i == 0)
						caps.Add(c);

					offset = c.End;
					i      = (i + 1) % 2;
				}
				catch (PatternMismatchException) when (i % 2 == 1)
				{
					break;
				}

			return new Group(source, start..offset, caps);
		}

		internal static Pattern Produce(PatternFactory factory, Pattern inner)
		{
			var backtrack = factory.Pos;
			var @base     = inner;
			factory.NextChar();

			try
			{
				var separator = factory.ProcessPattern();
				var alt = new AlternatingPattern(@base, separator)
						  {
							  Flags = @base.Flags & (EPatternFlags.SkipSpaces | EPatternFlags.IgnoreCase)
						  };
				@base.Flags = EPatternFlags.Default;
				if (factory.InBounds && factory.CurrentChar == ':')
					alt.ApplyMatchName(factory);

				return alt;
			}
			catch
			{
				factory.Pos = backtrack;
				return inner;
			}
		}

		public override string ToString() =>
			$"{Prefix}{ActualPattern[0]}!{ActualPattern[1]}{Postfix}";
	}
}

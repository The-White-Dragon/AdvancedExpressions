using System;
using AdvancedExpressions.Matching;
using AdvancedExpressions.Patterns;

namespace AdvancedExpressions
{
	public sealed class Advex
	{
		public Advex(string pattern)
		{
			var factory = new PatternFactory(pattern);
			Pattern = factory.Produce();
		}

		public Advex(string name, string pattern)
			: this(pattern) =>
			Name = name;

		internal string Name { get; set; }

		internal Pattern Pattern { get; }

		public Capture Match(string text, int start = 0)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			return Pattern.Match(text, start);
		}

		public override string ToString() =>
			$"{(string.IsNullOrEmpty(Name) ? "" : $"{Name} / ")}{Pattern}";
	}
}

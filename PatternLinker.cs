using AdvancedExpressions.Patterns;

namespace AdvancedExpressions
{
	internal static class PatternLinker
	{
		public static void Link(CompositePattern cp, AdvexCollection patterns)
		{
			foreach (var pattern in cp.ActualPattern)
				switch (pattern)
				{
					case AdvexPattern adv when adv.ActualPattern is null:
						adv.ActualPattern = patterns[adv.Link];
						break;
					case CompositePattern com:
						Link(com, patterns);
						break;
				}
		}
	}
}

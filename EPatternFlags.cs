using System;

namespace AdvancedExpressions
{
	[Flags]
	internal enum EPatternFlags
	{
		Default    = 0,
		SkipSpaces = 1,
		IgnoreCase = 2
	}
}

using System;

namespace AdvancedExpressions
{
	[Flags]
	public enum EAdvexSettings
	{
		Default         = 0b0000,
		Repeated        = 0b0010,
		ThrowExceptions = 0b0100
	}
}

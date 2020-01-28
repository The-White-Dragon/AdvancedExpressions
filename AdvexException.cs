using System;
using System.Runtime.Serialization;

namespace AdvancedExpressions
{
	[Serializable]
	public class AdvexException : Exception
	{
		public AdvexException()
		{ }

		public AdvexException(string message)
			: base(message)
		{ }

		public AdvexException(string message, Exception inner)
			: base(message, inner)
		{ }

		protected AdvexException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}
}

using System;
using System.Collections.Generic;

namespace AdvancedExpressions.Matching
{
	public class Group : Capture
	{
		private readonly List<Capture> _caps;

		internal Group(string source, Range range, List<Capture> caps)
			: base(source, range) =>
			_caps = caps;

		protected Group(string source, in Range range)
			: base(source, range)
		{ }

		public Capture this[int id] => _caps[id];

		public override Group ToGroup() =>
			this;
	}
}

using System;
using System.Collections.Generic;

namespace AdvancedExpressions.Matching
{
	public class Capture
	{
		private readonly Range  _range;
		private readonly string _source;

		internal Capture(string source,
						 Range  range)
		{
			_source = source;
			_range  = range;
		}

		public string Value => _source[_range];

		public int Start => _range.Start.Value;

		public int End => _range.End.Value;

		public override string ToString() =>
			Value;

		public virtual Group ToGroup() =>
			new Group(_source, _range, new List<Capture> { this });
	}
}

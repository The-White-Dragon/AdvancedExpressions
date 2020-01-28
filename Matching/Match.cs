using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedExpressions.Matching
{
	public class Match : Group
	{
		private readonly List<Group>             _groups;
		private readonly Dictionary<string, int> _ids;

		internal Match(string source, Range range, Dictionary<string, int> ids, List<Group> groups)
			: base(source, range)
		{
			_ids    = ids;
			_groups = groups;
		}

		public Group this[string name]
		{
			[DebuggerStepThrough] get => _groups[_ids[name]];
		}

		public Capture this[string name, int id]
		{
			[DebuggerStepThrough] get => this[name][id];
		}
	}
}

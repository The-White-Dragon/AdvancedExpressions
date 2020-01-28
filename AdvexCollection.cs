using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdvancedExpressions.Patterns;

namespace AdvancedExpressions
{
	[DebuggerStepThrough]
	public sealed class AdvexCollection : IEnumerable<Advex>
	{
		private readonly Dictionary<string, Advex> _dict;

		public AdvexCollection() =>
			_dict = new Dictionary<string, Advex>();

		public AdvexCollection(IDictionary<string, Advex> patterns)
			: this()
		{
			patterns.ToList()
					.ForEach(kv =>
							 {
								 kv.Value.Name = kv.Key;
								 Add(kv.Value);
							 }
							);
			Link();
		}

		public AdvexCollection(IDictionary<string, string> patterns)
			: this()
		{
			patterns.ToList()
					.ForEach(kv => Add(new Advex(kv.Key, kv.Value)));
			Link();
		}

		public Advex this[string name]
		{
			get => _dict[name];
			set
			{
				value.Name  = name;
				_dict[name] = value;
			}
		}

	#region IEnumerable<Advex> Members

		IEnumerator<Advex> IEnumerable<Advex>.GetEnumerator() =>
			_dict.Values.GetEnumerator();

		public IEnumerator GetEnumerator() =>
			((IEnumerable) _dict).GetEnumerator();

	#endregion

		public AdvexCollection Link()
		{
			foreach (var (_, pattern) in _dict)
				if (pattern.Pattern is CompositePattern cp)
					PatternLinker.Link(cp, this);

			return this;
		}

		public void Add(Advex advex)
		{
			if (string.IsNullOrEmpty(advex.Name))
				throw new ArgumentNullException(nameof(advex.Name));
			_dict[advex.Name] = advex;
		}
	}
}

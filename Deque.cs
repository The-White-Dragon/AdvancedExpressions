using System.Collections;
using System.Collections.Generic;

namespace AdvancedExpressions
{
	internal sealed class Deque<T> : ICollection<T>
	{
		private readonly LinkedList<T> _list;

		public Deque() =>
			_list = new LinkedList<T>();

		public T First => _list.First.Value;

		public T Last => _list.Last.Value;

	#region ICollection<T> Members

		public void Add(T item) =>
			_list.AddLast(item);

		public void Clear() =>
			_list.Clear();

		public bool Contains(T item) =>
			_list.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) =>
			_list.CopyTo(array, arrayIndex);

		public bool Remove(T item) =>
			_list.Remove(item);

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public IEnumerator<T> GetEnumerator() =>
			_list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			((IEnumerable) _list).GetEnumerator();

	#endregion

		public void Append(T item) =>
			_list.AddLast(item);

		public void Prepend(T item) =>
			_list.AddFirst(item);

		public T PopLast()
		{
			var e = Last;
			_list.RemoveLast();
			return e;
		}

		public T PopFirst()
		{
			var e = First;
			_list.RemoveFirst();
			return e;
		}
	}
}

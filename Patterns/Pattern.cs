using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AdvancedExpressions.Matching;

namespace AdvancedExpressions.Patterns
{
	/// <summary>
	///     Base class for all patterns
	/// </summary>
	internal abstract class Pattern
	{
		private Range _repeats = 1..1;

		internal void ApplyMatchName(PatternFactory factory)
		{
			factory.NextChar(); //eat ':'
			MatchName = factory.ProcessIdentifier();
			factory.Pos--;
		}

		internal void ApplyRepeats(PatternFactory factory)
		{
			factory.NextChar(); // eat '<'

			var start = factory.Pos;
			var data  = "";
			while (factory.CurrentChar != '>')
			{
				data += factory.CurrentChar;
				factory.NextChar();
			}

			if (!factory.InBounds)
			{
				//err: out of bounds
			}

			if (!_repeats.Equals(1..1))
			{
				//err: not allowed
			}

			var reps = data.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			switch (reps.Length)
			{
				case 1:
					var c = int.Parse(reps[0]);
					_repeats = c..c;
					break;

				case 2:
					_repeats = int.Parse(reps[0])..int.Parse(reps[1]);
					break;
			}
		}

		internal void ApplyInfiniteQuantifier(PatternFactory factory)
		{
			if (MaxRepeats == 0)
				throw new Exception($"{factory.Pos}");
			MaxRepeats = 0;
		}

		internal void ApplyOptionalQuantifier(PatternFactory factory)
		{
			if (MinRepeats == 0)
				throw new Exception("");
			MinRepeats = 0;
		}

		internal virtual void ApplyFlags(EPatternFlags flags) =>
			Flags |= flags;

	#region Parameters

		internal string MatchName { get; private set; }

		protected string Prefix => $"{(SkipSpaces ? "~" : "")}{(IgnoreCase ? "^" : "")}";

		protected string Postfix
		{
			[DebuggerStepThrough]
			get =>
				@$"{(string.IsNullOrEmpty(MatchName) ? "" : $":{MatchName}")
					}{(IsOnce             ? "" :
					   IsPureInfinite     ? "!" :
					   IsPureOptional     ? "?" :
					   IsOptionalInfinite ? "?!" : $"<{MinRepeats},{MaxRepeats}>")}";
		}

	#endregion

	#region Flags

		internal EPatternFlags Flags { get; set; }

		internal bool SkipSpaces => Flags.HasFlag(EPatternFlags.SkipSpaces);

		internal bool IgnoreCase => Flags.HasFlag(EPatternFlags.IgnoreCase);

	#endregion

	#region Matching

		protected abstract Capture MatchPattern(string source, int start);

		private bool CaptureRequired(int capCount) =>
			IsInfinite || capCount < MaxRepeats;

		protected bool InBounds(string src, int pos) =>
			pos >= 0 && pos < src.Length;

		internal Capture Match([NotNull] string source, int start = 0)
		{
			OuterPattern?.TrySkipSpaces(source, ref start);

			var offset = start;
			var caps   = new List<Capture>();

			while (InBounds(source, offset) && CaptureRequired(caps.Count))
				try
				{
					OuterPattern?.TrySkipSpaces(source, ref offset);
					var c = MatchPattern(source, offset);

					if (c is null)
						throw new PatternMismatchException(this, offset);

					caps.Add(c.ToGroup());
					offset = c.End;
				}
				catch (PatternMismatchException)
					when (caps.Count == MaxRepeats || IsInfinite && caps.Count > 0 || IsOptional)
				{
					break;
				}

			return new Group(source, start..offset, caps);
		}

		internal CompositePattern OuterPattern { get; set; }

		internal bool IsOptional
		{
			[DebuggerStepThrough] get => MinRepeats == 0;
		}

		internal bool IsInfinite
		{
			[DebuggerStepThrough] get => MaxRepeats == 0;
		}

		internal bool IsPureOptional
		{
			[DebuggerStepThrough] get => MinRepeats == 0 && MaxRepeats == 1;
		}

		internal bool IsPureInfinite
		{
			[DebuggerStepThrough] get => MinRepeats == 1 && MaxRepeats == 0;
		}

		internal bool IsOnce
		{
			[DebuggerStepThrough] get => MinRepeats == 1 && MaxRepeats == 1;
		}

		internal bool IsOptionalInfinite
		{
			[DebuggerStepThrough] get => MinRepeats == 0 && MaxRepeats == 0;
		}

		internal int MinRepeats
		{
			[DebuggerStepThrough] get => _repeats.Start.Value;
			set => _repeats = value.._repeats.End.Value;
		}

		internal int MaxRepeats
		{
			[DebuggerStepThrough] get => _repeats.End.Value;
			set => _repeats = _repeats.Start.Value..value;
		}

		private void TrySkipSpaces([NotNull] string source, ref int offset)
		{
			if (SkipSpaces)
				while (offset < source.Length && char.IsWhiteSpace(source[offset]))
					offset++;
		}

	#endregion
	}

	/// <summary>
	///     Adjustable base class for all patterns
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerStepThrough]
	internal abstract class Pattern<T> : Pattern
	{
		protected Pattern(T pattern) =>
			ActualPattern = pattern;

		internal T ActualPattern { get; set; }
	}
}

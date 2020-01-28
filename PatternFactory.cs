using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedExpressions.Patterns;
using static System.StringComparison;
using static AdvancedExpressions.EPatternFlags;

namespace AdvancedExpressions
{
	internal class PatternFactory
	{
		private static readonly Dictionary<string, Regex>  UniqueRegexes = new Dictionary<string, Regex>();
		private static readonly Dictionary<string, string> UniqueStrings = new Dictionary<string, string>();

		private readonly Stack<EPatternFlags>  _flags;
		private readonly Stack<Deque<Pattern>> _patternsStack;

		private readonly string _src;
		internal         int    Pos;

		internal PatternFactory(string src)
		{
			_src           = src;
			Pos            = 0;
			_patternsStack = new Stack<Deque<Pattern>>();
			_flags         = new Stack<EPatternFlags>();
		}

		internal Pattern Produce()
		{
			var seq = ProcessSequence();
			return seq.Count == 1
					   ? seq.First
					   : new CompositePattern(seq.ToList());
		}

	#region Processors

		private static Dictionary<char, PatternProcessor> ProducePattern { get; } =
			new Dictionary<char, PatternProcessor>
			{
				['@'] = RegexPattern.Produce,
				['"'] = StringPattern.Produce,
				['l'] = AdvexPattern.Produce,
				['('] = CompositePattern.Produce,
				['['] = SelectingPattern.Produce,
				['{'] = RematchingPattern.Produce
			};

		private static Dictionary<char, PostfixProcessor> ApplyPostfix { get; } =
			new Dictionary<char, PostfixProcessor>
			{
				['?'] = factory => factory.LastPattern.ApplyOptionalQuantifier(factory),
				[':'] = factory => factory.LastPattern.ApplyMatchName(factory),
				['<'] = factory => factory.LastPattern.ApplyRepeats(factory),
				['!'] = factory => factory.MakeAlternatingOrInfinite()
			};

		private static Dictionary<char, PrefixProcessor> ProducePrefix { get; } =
			new Dictionary<char, PrefixProcessor>
			{
				['~'] = _SkipSpaces,
				['^'] = _IgnoreCase
			};

		internal Deque<Pattern> ProcessSequence()
		{
			var deque = new Deque<Pattern>();
			CollectSequence(deque);

			while (InBounds && !CurrentCodeIsSequenceEnd())
				ProcessModifiedPattern();

			return PopSequence();
		}

	#region Primary

		private void ProcessModifiedPattern()
		{
			SkipSpacesInPattern();
			ProcessPrefix();
			if (PatternCode != 'p')
				CollectPattern(ProcessPattern());
			ProcessPostfix();
			ApplyFlags(LastPattern);
		}

		internal Pattern ProcessPattern() =>
			ProducePattern[PatternCode](this);

		[DebuggerStepThrough]
		internal void ProcessPrefix()
		{
			_flags.Push(Default);
			while (InBounds && ProducePrefix.ContainsKey(CurrentChar))
			{
				_flags.Push(ProducePrefix[CurrentChar](this));
				NextChar();
			}
		}

		[DebuggerStepThrough]
		internal void ProcessPostfix()
		{
			while (InBounds && ApplyPostfix.ContainsKey(CurrentChar))
			{
				ApplyPostfix[CurrentChar](this);
				/*if (LastPattern is AlternatingPattern)
					return;*/
				NextChar();
			}
		}

		[DebuggerStepThrough]
		private void ApplyFlags(Pattern pattern) =>
			pattern.ApplyFlags(_flags.Pop());

	#endregion

	#region Literals

		[DebuggerStepThrough]
		public string ProcessIdentifier()
		{
			var start = Pos;
			while (InBounds && CurrentIsIdentifier)
				NextChar();
			return _src[start..Pos];
		}

		[DebuggerStepThrough]
		internal string ProcessStringLiteral()
		{
			var start   = Pos;
			var endChar = CurrentChar;
			NextChar();
			while (InBounds)
			{
				if (_src[Pos..(Pos + 1)] == $"\\{endChar}") // skip escaped char
					NextChar();
				NextChar();

				if (InBounds && CurrentChar == endChar)
					return _src[(start + 1)..Pos]; // we need data inside of range
			}

			throw new AdvexException($"Expected {endChar} - String literal is not ended (start:{start})");
		}

	#endregion

	#region Modifiers

	#region Prefix

		[DebuggerStepThrough]
		private static EPatternFlags _IgnoreCase(PatternFactory factory) =>
			factory._flags.Peek()
		  | (!factory._flags.Pop().HasFlag(IgnoreCase)
				 ? IgnoreCase
				 : throw new
					   AdvexException($"Pattern already have 'ignore case' modifier (pos:{factory.Pos})"));

		[DebuggerStepThrough]
		private static EPatternFlags _SkipSpaces(PatternFactory factory) =>
			factory._flags.Peek()
		  | (!factory._flags.Pop().HasFlag(SkipSpaces)
				 ? SkipSpaces
				 : throw new
					   AdvexException($"Pattern already have 'skip spaces' modifier (pos:{factory.Pos})"));

	#endregion

	#region Postfix

		private void MakeAlternatingOrInfinite()
		{
			var pat = PopPattern();
			if (pat is AlternatingPattern)
				throw new AdvexException($"Alternating pattern can't have infinite quantifier {Pos}");

			if (!((pat = AlternatingPattern.Produce(this, pat)) is AlternatingPattern))
				pat.ApplyInfiniteQuantifier(this);

			CollectPattern(pat);
		}

	#endregion

	#endregion

	#endregion

	#region Collection Interactions

		[DebuggerStepThrough]
		public void CollectSequence(Deque<Pattern> sequence) =>
			_patternsStack.Push(sequence);

		[DebuggerStepThrough]
		internal Deque<Pattern> PopSequence() =>
			_patternsStack.Pop();

		[DebuggerStepThrough]
		internal void CollectPattern(Pattern pattern) =>
			_patternsStack.Peek().Add(pattern);

		[DebuggerStepThrough]
		internal Pattern PopPattern() =>
			_patternsStack.Peek().PopLast();

		internal List<Pattern> LastSequence
		{
			[DebuggerStepThrough] get => _patternsStack.Peek().ToList();
		}

		internal Pattern LastPattern
		{
			[DebuggerStepThrough] get => _patternsStack.Peek().Last;
		}

	#endregion

	#region Helpers

		internal char CurrentChar
		{
			[DebuggerStepThrough] get => _src[Pos];
		}

		[DebuggerStepThrough]
		internal void NextChar() =>
			Pos++;

		private bool CurrentIsIdentifier
		{
			[DebuggerStepThrough] get => char.IsLetterOrDigit(CurrentChar);
		}

		[DebuggerStepThrough]
		private bool CurrentCodeIsSequenceEnd() =>
			PatternCode == 'e';

		private char PatternCode
		{
			[DebuggerStepThrough]
			get =>
				@"'""`".Contains(CurrentChar, InvariantCulture)
					? '"' // Any quotes
					: ")}]".Contains(CurrentChar, InvariantCulture)
						? 'e' // End of special group
						: CurrentIsIdentifier
							? 'l' // Any letter or digit
							: CurrentCharIsWhiteSpace
								? ' ' // Any whitespace
								: "!?:<".Contains(CurrentChar, InvariantCulture)
									? 'p' // Postfix modifier
									: CurrentChar;
		}

		internal bool InBounds
		{
			[DebuggerStepThrough] get => Pos < _src.Length;
		}

		[DebuggerStepThrough]
		private void SkipSpacesInPattern()
		{
			while (InBounds && CurrentCharIsWhiteSpace)
				NextChar();
		}

		private bool CurrentCharIsWhiteSpace
		{
			[DebuggerStepThrough] get => char.IsWhiteSpace(CurrentChar);
		}

	#endregion

	#region Unification

		[DebuggerStepThrough]
		internal static Regex UniqueRegex(string str, RegexOptions options = RegexOptions.None)
		{
			if (!UniqueRegexes.ContainsKey(str))
				UniqueRegexes.Add(str, new Regex(str, RegexOptions.Compiled | options));

			return UniqueRegexes[str];
		}

		[DebuggerStepThrough]
		internal static string UniqueString(string str)
		{
			if (!UniqueStrings.ContainsKey(str))
				UniqueStrings.Add(str, str);

			return UniqueStrings[str];
		}

	#endregion
	}
}

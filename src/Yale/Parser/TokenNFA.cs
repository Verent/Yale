namespace Yale.Parser;

/**
 * A non-deterministic finite state automaton (NFA) for matching
 * tokens. It supports both fixed strings and simple regular
 * expressions, but should perform similar to a DFA due to highly
 * optimized data structures and tuning. The memory footprint during
 * matching should be near zero, since no heap memory is allocated
 * unless the pre-allocated queues need to be enlarged. The NFA also
 * does not use recursion, but iterates in a loop instead.
 *

 * @version  1.5
 * @since    1.5
 */
internal sealed class TokenNFA
{
    /**
     * The initial state lookup table, indexed by the first ASCII
     * character. This array is used to for speed optimizing the
     * first step in the match, since the initial state would
     * otherwise have a long list of transitions to consider.
     */
    private readonly NFAState[] initialChar = new NFAState[128];

    /**
     * The initial state. This state contains any transitions not
     * already stored in the initial text state array, i.e. non-ASCII
     * or complex transitions (such as regular expressions).
     */
    private readonly NFAState initial = new();

    /**
     * The NFA state queue to use.
     */
    private readonly NFAStateQueue queue = new();

    /**
     * Adds a string match to this automaton. New states and
     * transitions will be added to extend this automaton to support
     * the specified string.
     *
     * @param str            the string to match
     * @param ignoreCase     the case-insensitive match flag
     * @param value          the match value
     */
    public void AddTextMatch(string str, bool ignoreCase, TokenPattern value)
    {
        NFAState state;
        var ch = str[0];

        if (ch < 128 && !ignoreCase)
        {
            state = initialChar[ch];
            state ??= initialChar[ch] = new NFAState();
        }
        else
        {
            state = initial.AddOut(ch, ignoreCase, null);
        }
        for (var i = 1; i < str.Length; i++)
        {
            state = state.AddOut(str[i], ignoreCase, null);
        }
        state.value = value;
    }

    /**
     * Adds a regular expression match to this automaton. New states
     * and transitions will be added to extend this automaton to
     * support the specified string. Note that this method only
     * supports a subset of the full regular expression syntax, so
     * a more complete regular expression library must also be
     * provided.
     *
     * @param pattern        the regular expression string
     * @param ignoreCase     the case-insensitive match flag
     * @param value          the match value
     *
     * @throws RegExpException if the regular expression parsing
     *             failed
     */
    public void AddRegExpMatch(string pattern, bool ignoreCase, TokenPattern value)
    {
        TokenRegExpParser parser = new(pattern, ignoreCase);
        var debug = new StringBuilder("DFA regexp; " + parser.GetDebugInfo());
        var isAscii = parser._start.IsAsciiOutgoing();

        for (var i = 0; isAscii && i < 128; i++)
        {
            var match = false;
            for (var j = 0; j < parser._start.outgoing.Length; j++)
            {
                if (parser._start.outgoing[j].Match((char)i))
                {
                    if (match)
                    {
                        isAscii = false;
                        break;
                    }
                    match = true;
                }
            }
            if (match && initialChar[i] != null)
            {
                isAscii = false;
            }
        }
        if (parser._start.incoming.Length > 0)
        {
            initial.AddOut(new NFAEpsilonTransition(parser._start));
            debug.Append(", uses initial epsilon");
        }
        else if (isAscii && !ignoreCase)
        {
            for (var i = 0; isAscii && i < 128; i++)
            {
                for (var j = 0; j < parser._start.outgoing.Length; j++)
                {
                    if (parser._start.outgoing[j].Match((char)i))
                    {
                        initialChar[i] = parser._start.outgoing[j].state;
                    }
                }
            }
            debug.Append(", uses ASCII lookup");
        }
        else
        {
            parser._start.MergeInto(initial);
            debug.Append(", uses initial state");
        }
        parser._end.value = value;
        value.DebugInfo = debug.ToString();
    }

    /**
     * Checks if this NFA matches the specified input text. The
     * matching will be performed from position zero (0) in the
     * buffer. This method will not read any characters from the
     * stream, just peek ahead.
     *
     * @param buffer         the input buffer to check
     * @param match          the token match to update
     *
     * @return the number of characters matched, or
     *         zero (0) if no match was found
     *
     * @throws IOException if an I/O error occurred
     */
    public int Match(ReaderBuffer buffer, TokenMatch match)
    {
        var length = 0;
        var pos = 1;
        int peekChar;

        // The first step of the match loop has been unrolled and
        // optimized for performance below.
        queue.Clear();
        peekChar = buffer.Peek(0);
        if (peekChar is >= 0 and < 128)
        {
            var state = initialChar[peekChar];
            if (state is not null)
            {
                queue.AddLast(state);
            }
        }
        if (peekChar >= 0)
        {
            initial.MatchTransitions((char)peekChar, queue, true);
        }
        queue.MarkEnd();
        peekChar = buffer.Peek(1);

        // The remaining match loop processes all subsequent states
        while (queue.Empty is false)
        {
            if (queue.Marked)
            {
                pos++;
                peekChar = buffer.Peek(pos);
                queue.MarkEnd();
            }
            var state = queue.RemoveFirst();
            if (state?.value is not null)
            {
                match.Update(pos, state.value);
            }
            if (peekChar >= 0 && state is not null) //state is not null was added. Is throw a better option? When can peekChar >= 0 be true while state is null
            {
                state.MatchTransitions((char)peekChar, queue, false);
            }
        }
        return length;
    }
}

/**
 * An NFA state. The NFA consists of a series of states, each
 * having zero or more transitions to other states.
 */
internal sealed class NFAState
{
    /**
    * The optional state value (if it is a final state).
     */
    internal TokenPattern? value;

    /**
    * The incoming transitions to this state.
    */
    internal NFATransition[] incoming = Array.Empty<NFATransition>();

    /**
    * The outgoing transitions from this state.
    */
    internal NFATransition[] outgoing = Array.Empty<NFATransition>();

    /**
    * The outgoing epsilon transitions flag.
    */
    internal bool epsilonOut;

    /**
     * Checks if this state has any incoming or outgoing
     * transitions.
     *
     * @return true if this state has transitions, or
     *         false otherwise
     */
    public bool HasTransitions() => incoming.Length > 0 || outgoing.Length > 0;

    /**
     * Checks if all outgoing transitions only match ASCII
     * characters.
     *
     * @return true if all transitions are ASCII-only, or
     *         false otherwise
     */
    public bool IsAsciiOutgoing()
    {
        for (var i = 0; i < outgoing.Length; i++)
        {
            if (outgoing[i].IsAscii() == false)
            {
                return false;
            }
        }
        return true;
    }

    /**
     * Adds a new incoming transition.
     *
     * @param trans          the transition to add
     */
    public void AddIn(NFATransition trans)
    {
        Array.Resize(ref incoming, incoming.Length + 1);
        incoming[^1] = trans;
    }

    /**
     * Adds a new outgoing character transition. If the target
     * state specified was null and an identical transition
     * already exists, it will be reused and its target returned.
     *
     * @param ch             he character to match
     * @param ignoreCase     the case-insensitive flag
     * @param state          the target state, or null
     *
     * @return the transition target state
     */
    public NFAState AddOut(char ch, bool ignoreCase, NFAState? state)
    {
        if (ignoreCase)
        {
            state ??= new NFAState();
            AddOut(new NFACharTransition(char.ToLower(ch, CultureInfo.InvariantCulture), state));
            AddOut(new NFACharTransition(char.ToUpper(ch, CultureInfo.InvariantCulture), state));
            return state;
        }
        else
        {
            if (state is null)
            {
                state = FindUniqueCharTransition(ch);
                if (state is not null)
                {
                    return state;
                }
                state = new NFAState();
            }
            return AddOut(new NFACharTransition(ch, state));
        }
    }

    /**
     * Adds a new outgoing transition.
     *
     * @param trans          the transition to add
     *
     * @return the transition target state
     */
    public NFAState AddOut(NFATransition trans)
    {
        Array.Resize(ref outgoing, outgoing.Length + 1);
        outgoing[^1] = trans;
        if (trans is NFAEpsilonTransition)
        {
            epsilonOut = true;
        }
        return trans.state;
    }

    /**
     * Merges all the transitions in this state into another
     * state.
     *
     * @param state      the state to merge into
     */
    public void MergeInto(NFAState state)
    {
        for (var i = 0; i < incoming.Length; i++)
        {
            state.AddIn(incoming[i]);
            incoming[i].state = state;
        }
        incoming = Array.Empty<NFATransition>();
        for (var i = 0; i < outgoing.Length; i++)
        {
            state.AddOut(outgoing[i]);
        }
        outgoing = Array.Empty<NFATransition>();
    }

    /**
     * Finds a unique character transition if one exists. The
     * transition must be the only matching single character
     * transition and no other transitions may reach the same
     * state.
     *
     * @param ch             the character to search for
     *
     * @return the unique transition state found, or
     *         null if not found
     */
    private NFAState? FindUniqueCharTransition(char ch)
    {
        NFATransition? res = null;
        NFATransition trans;

        for (var i = 0; i < outgoing.Length; i++)
        {
            trans = outgoing[i];
            if (trans.Match(ch) && trans is NFACharTransition)
            {
                if (res != null)
                {
                    return null;
                }
                res = trans;
            }
        }
        for (var i = 0; res != null && i < outgoing.Length; i++)
        {
            trans = outgoing[i];
            if (trans != res && trans.state == res.state)
            {
                return null;
            }
        }
        return res?.state;
    }

    /**
     * Attempts a match on each of the transitions leading from
     * this state. If a match is found, its state will be added
     * to the queue. If the initial match flag is set, epsilon
     * transitions will also be matched (and their targets called
     * recursively).
     *
     * @param ch         the character to match
     * @param queue      the state queue
     * @param initial    the initial match flag
     */
    public void MatchTransitions(char ch, NFAStateQueue queue, bool initial)
    {
        NFATransition trans;
        NFAState target;

        for (var i = 0; i < outgoing.Length; i++)
        {
            trans = outgoing[i];
            target = trans.state;
            if (initial && trans is NFAEpsilonTransition)
            {
                target.MatchTransitions(ch, queue, true);
            }
            else if (trans.Match(ch))
            {
                queue.AddLast(target);
                if (target.epsilonOut)
                {
                    target.MatchEmpty(queue);
                }
            }
        }
    }

    /**
     * Adds all the epsilon transition targets to the specified
     * queue.
     *
     * @param queue      the state queue
     */
    public void MatchEmpty(NFAStateQueue queue)
    {
        NFATransition trans;
        NFAState target;

        for (var i = 0; i < outgoing.Length; i++)
        {
            trans = outgoing[i];
            if (trans is NFAEpsilonTransition)
            {
                target = trans.state;
                queue.AddLast(target);
                if (target.epsilonOut)
                {
                    target.MatchEmpty(queue);
                }
            }
        }
    }
}

/**
 * An NFA state transition. A transition checks a single
 * character of input an determines if it is a match. If a match
 * is encountered, the NFA should move forward to the transition
 * state.
 */
internal abstract class NFATransition
{
    /**
     * The target state of the transition.
     */
    internal NFAState state;

    /**
     * Creates a new state transition.
     *
     * @param state          the target state
     */
    public NFATransition(NFAState state)
    {
        this.state = state;
        this.state.AddIn(this);
    }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public abstract bool IsAscii();

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public abstract bool Match(char ch);

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public abstract NFATransition Copy(NFAState state);
}

/**
 * The special epsilon transition. This transition matches the
 * empty input, i.e. it is an automatic transition that doesn't
 * read any input. As such, it returns false in the match method
 * and is handled specially everywhere.
 */
internal sealed class NFAEpsilonTransition : NFATransition
{
    /**
     * Creates a new epsilon transition.
     *
     * @param state          the target state
     */
    public NFAEpsilonTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => false;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch) => false;

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFAEpsilonTransition(state);
}

/**
 * A single character match transition.
 */
internal sealed class NFACharTransition : NFATransition
{
    /**
     * The character to match.
     */
    private readonly char _match;

    /**
     * Creates a new character transition.
     *
     * @param match          the character to match
     * @param state          the target state
     */
    public NFACharTransition(char match, NFAState state)
        : base(state) => _match = match;

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => 0 <= _match && _match < 128;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch) => _match == ch;

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFACharTransition(_match, state);
}

/**
 * A character range match transition. Used for user-defined
 * character sets in regular expressions.
 */
internal sealed class NFACharRangeTransition : NFATransition
{
    /**
     * The inverse match flag.
     */
    private readonly bool inverse;

    /**
     * The case-insensitive match flag.
     */
    private readonly bool ignoreCase;

    /**
     * The character set content. This array may contain either
     * range objects or Character objects.
     */
    private object[] contents = Array.Empty<object>();

    /**
     * Creates a new character range transition.
     *
     * @param inverse        the inverse match flag
     * @param ignoreCase     the case-insensitive match flag
     * @param state          the target state
     */
    public NFACharRangeTransition(bool inverse, bool ignoreCase, NFAState state)
        : base(state)
    {
        this.inverse = inverse;
        this.ignoreCase = ignoreCase;
    }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii()
    {
        object obj;
        char c;

        if (inverse)
        {
            return false;
        }
        for (var i = 0; i < contents.Length; i++)
        {
            obj = contents[i];
            if (obj is char)
            {
                c = (char)obj;
                if (c < 0 || 128 <= c)
                {
                    return false;
                }
            }
            else if (obj is Range)
            {
                if (!((Range)obj).IsAscii())
                {
                    return false;
                }
            }
        }
        return true;
    }

    /**
     * Adds a single character to this character set.
     *
     * @param c              the character to add
     */
    public void AddCharacter(char c)
    {
        if (ignoreCase)
        {
            c = char.ToLower(c, CultureInfo.InvariantCulture);
        }
        AddContent(c);
    }

    /**
     * Adds a character range to this character set.
     *
     * @param min            the minimum character value
     * @param max            the maximum character value
     */
    public void AddRange(char min, char max)
    {
        if (ignoreCase)
        {
            min = char.ToLower(min, CultureInfo.InvariantCulture);
            max = char.ToLower(max, CultureInfo.InvariantCulture);
        }
        AddContent(new Range(min, max));
    }

    /**
     * Adds an object to the character set content array.
     *
     * @param obj            the object to add
     */
    private void AddContent(object obj)
    {
        Array.Resize(ref contents, contents.Length + 1);
        contents[^1] = obj;
    }

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        object obj;
        char c;
        Range r;

        if (ignoreCase)
        {
            ch = char.ToLower(ch, CultureInfo.InvariantCulture);
        }
        for (var i = 0; i < contents.Length; i++)
        {
            obj = contents[i];
            if (obj is char)
            {
                c = (char)obj;
                if (c == ch)
                {
                    return !inverse;
                }
            }
            else if (obj is Range)
            {
                r = (Range)obj;
                if (r.Inside(ch))
                {
                    return !inverse;
                }
            }
        }
        return inverse;
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state)
    {
        NFACharRangeTransition copy;

        copy = new NFACharRangeTransition(inverse, ignoreCase, state);
        copy.contents = contents;
        return copy;
    }

    /**
     * A character range class.
     */
    private readonly struct Range
    {
        /**
         * The minimum character value.
         */
        private readonly char _min;

        /**
         * The maximum character value.
         */
        private readonly char _max;

        /**
         * Creates a new character range.
         *
         * @param min        the minimum character value
         * @param max        the maximum character value
         */
        public Range(char min, char max)
        {
            _min = min;
            _max = max;
        }

        /**
         * Checks if this range only matches ASCII characters
         *
         * @return true if this range only matches ASCII, or
         *         false otherwise
         */
        public bool IsAscii() => 0 <= _min && _min < 128 && 0 <= _max && _max < 128;

        /**
         * Checks if the specified character is inside the range.
         *
         * @param c          the character to check
         *
         * @return true if the character is in the range, or
         *         false otherwise
         */
        public bool Inside(char c) => _min <= c && c <= _max;
    }
}

/**
 * The dot ('.') character set transition. This transition
 * matches a single character that is not equal to a newline
 * character.
 */
internal sealed class NFADotTransition : NFATransition
{
    /**
     * Creates a new dot character set transition.
     *
     * @param state          the target state
     */
    public NFADotTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => false;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        switch (ch)
        {
            case '\n':
            case '\r':
            case '\u0085':
            case '\u2028':
            case '\u2029':
                return false;
            default:
                return true;
        }
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFADotTransition(state);
}

/**
 * The digit character set transition. This transition matches a
 * single numeric character.
 */
internal sealed class NFADigitTransition : NFATransition
{
    /**
     * Creates a new digit character set transition.
     *
     * @param state          the target state
     */
    public NFADigitTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => true;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch) => '0' <= ch && ch <= '9';

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFADigitTransition(state);
}

/**
 * The non-digit character set transition. This transition
 * matches a single non-numeric character.
 */
internal sealed class NFANonDigitTransition : NFATransition
{
    /**
     * Creates a new non-digit character set transition.
     *
     * @param state          the target state
     */
    public NFANonDigitTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => false;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch) => ch < '0' || '9' < ch;

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFANonDigitTransition(state);
}

/**
 * The whitespace character set transition. This transition
 * matches a single whitespace character.
 */
internal sealed class NFAWhitespaceTransition : NFATransition
{
    /**
     * Creates a new whitespace character set transition.
     *
     * @param state          the target state
     */
    public NFAWhitespaceTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => true;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        switch (ch)
        {
            case ' ':
            case '\t':
            case '\n':
            case '\f':
            case '\r':
            case (char)11:
                return true;
            default:
                return false;
        }
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFAWhitespaceTransition(state);
}

/**
 * The non-whitespace character set transition. This transition
 * matches a single non-whitespace character.
 */
internal sealed class NFANonWhitespaceTransition : NFATransition
{
    /**
     * Creates a new non-whitespace character set transition.
     *
     * @param state          the target state
     */
    public NFANonWhitespaceTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => false;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        switch (ch)
        {
            case ' ':
            case '\t':
            case '\n':
            case '\f':
            case '\r':
            case (char)11:
                return false;
            default:
                return true;
        }
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFANonWhitespaceTransition(state);
}

/**
 * The word character set transition. This transition matches a
 * single word character.
 */
internal sealed class NFAWordTransition : NFATransition
{
    /**
     * Creates a new word character set transition.
     *
     * @param state          the target state
     */
    public NFAWordTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => true;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        return 'a' <= ch && ch <= 'z'
            || 'A' <= ch && ch <= 'Z'
            || '0' <= ch && ch <= '9'
            || ch == '_';
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFAWordTransition(state);
}

/**
 * The non-word character set transition. This transition matches
 * a single non-word character.
 */
internal sealed class NFANonWordTransition : NFATransition
{
    /**
     * Creates a new non-word character set transition.
     *
     * @param state          the target state
     */
    public NFANonWordTransition(NFAState state)
        : base(state) { }

    /**
     * Checks if this transition only matches ASCII characters.
     * I.e. characters with numeric values between 0 and 127.
     *
     * @return true if this transition only matches ASCII, or
     *         false otherwise
     */
    public override bool IsAscii() => false;

    /**
     * Checks if the specified character matches the transition.
     *
     * @param ch             the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    public override bool Match(char ch)
    {
        var word =
            'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || '0' <= ch && ch <= '9' || ch == '_';
        return !word;
    }

    /**
     * Creates a copy of this transition but with another target
     * state.
     *
     * @param state          the new target state
     *
     * @return an identical copy of this transition
     */
    public override NFATransition Copy(NFAState state) => new NFANonWordTransition(state);
}

/**
 * An NFA state queue. This queue is used during processing to
 * keep track of the current and subsequent NFA states. The
 * current state is read from the beginning of the queue, and new
 * states are added at the end. A marker index is used to
 * separate the current from the subsequent states.<p>
 *
 * The queue implementation is optimized for quick removal at the
 * beginning and addition at the end. It will attempt to use a
 * fixed-size array to store the whole queue, and moves the data
 * in this array only when absolutely needed. The array is also
 * enlarged automatically if too many states are being processed
 * at a single time.
 */
internal sealed class NFAStateQueue
{
    /**
    * The state queue array. Will be enlarged as needed.
    */
    private NFAState[] queue = new NFAState[2048];

    /**
    * The position of the first entry in the queue (inclusive).
    */
    private int first;

    /**
    * The position just after the last entry in the queue
    * (exclusive).
    */
    private int last;

    /**
    * The current queue mark position.
    */
    private int mark;

    /**
     * The empty queue property (read-only).
     */
    public bool Empty
    {
        get { return last <= first; }
    }

    /**
     * The marked first entry property (read-only). This is set
     * to true if the first entry in the queue has been marked.
     */
    public bool Marked
    {
        get { return first == mark; }
    }

    /**
     * Clears this queue. This operation is fast, as it just
     * resets the queue position indices.
     */
    public void Clear()
    {
        first = 0;
        last = 0;
        mark = 0;
    }

    /**
     * Marks the end of the queue. This means that the next entry
     * added to the queue will be marked (when it becomes the
     * first in the queue). This operation is fast.
     */
    public void MarkEnd() => mark = last;

    /**
     * Removes and returns the first entry in the queue. This
     * operation is fast, since it will only update the index of
     * the first entry in the queue.
     *
     * @return the previous first entry in the queue
     */
    public NFAState? RemoveFirst()
    {
        if (first < last)
        {
            first++;
            return queue[first - 1];
        }
        else
        {
            return null;
        }
    }

    /**
     * Adds a new entry at the end of the queue. This operation
     * is mostly fast, unless all the allocated queue space has
     * already been used.
     *
     * @param state          the state to add
     */
    public void AddLast(NFAState state)
    {
        if (last >= queue.Length)
        {
            if (first <= 0)
            {
                Array.Resize(ref queue, queue.Length * 2);
            }
            else
            {
                Array.Copy(queue, first, queue, 0, last - first);
                last -= first;
                mark -= first;
                first = 0;
            }
        }
        queue[last++] = state;
    }
}

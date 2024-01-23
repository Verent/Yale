using System.Diagnostics;

namespace Yale.Parser
{
    /**
     * A token look-ahead set. This class contains a set of token id
     * sequences. All sequences in the set are limited in length, so
     * that no single sequence is longer than a maximum value. This
     * class also filters out duplicates. Each token sequence also
     * contains a repeat flag, allowing the look-ahead set to contain
     * information about possible infinite repetitions of certain
     * sequences. That information is important when conflicts arise
     * between two look-ahead sets, as such a conflict cannot be
     * resolved if the conflicting sequences can be repeated (would
     * cause infinite loop).
     *
     */
    internal sealed class LookAheadSet
    {
        /**
         * The set of token look-ahead sequences. Each sequence in
         * turn is represented by a List with Integers for the
         * token id:s.
         */
        private readonly List<Sequence> elements = new();

        /**
         * The maximum length of any look-ahead sequence.
         */
        private readonly int maxLength;

        /**
         * Creates a new look-ahead set with the specified maximum
         * length.
         *
         * @param maxLength      the maximum token sequence length
         */
        public LookAheadSet(int maxLength)
        {
            this.maxLength = maxLength;
        }

        /**
         * Creates a duplicate look-ahead set, possibly with a
         * different maximum length.
         *
         * @param maxLength      the maximum token sequence length
         * @param set            the look-ahead set to copy
         */
        public LookAheadSet(int maxLength, LookAheadSet set)
            : this(maxLength)
        {
            AddAll(set);
        }

        /**
         * Returns the size of this look-ahead set.
         *
         * @return the number of token sequences in the set
         */
        public int Size()
        {
            return elements.Count;
        }

        /**
         * Returns the length of the shortest token sequence in this
         * set. This method will return zero (0) if the set is empty.
         *
         * @return the length of the shortest token sequence
         */
        public int GetMinLength()
        {
            var min = -1;

            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (min < 0 || seq.Length() < min)
                {
                    min = seq.Length();
                }
            }
            return min < 0 ? 0 : min;
        }

        /**
         * Returns the length of the longest token sequence in this
         * set. This method will return zero (0) if the set is empty.
         *
         * @return the length of the longest token sequence
         */
        public int GetMaxLength()
        {
            var max = 0;
            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (seq.Length() > max)
                {
                    max = seq.Length();
                }
            }
            return max;
        }

        /**
         * Returns a list of the initial token id:s in this look-ahead
         * set. The list returned will not contain any duplicates.
         *
         * @return a list of the initial token id:s in this look-ahead set
         */
        public TokenId[] GetInitialTokens()
        {
            List<TokenId> list = new();

            for (var i = 0; i < elements.Count; i++)
            {
                var token = elements[i].GetToken(0);
                if (token is not null && list.Contains(token.Value) == false)
                {
                    list.Add(token.Value);
                }
            }
            return list.ToArray();
        }

        /**
         * Checks if this look-ahead set contains a repetitive token
         * sequence.
         *
         * @return true if at least one token sequence is repetitive, or
         *         false otherwise
         */
        public bool IsRepetitive()
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (seq.IsRepetitive())
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if the next token(s) in the parser match any token
         * sequence in this set.
         *
         * @param parser         the parser to check
         *
         * @return true if the next tokens are in the set, or
         *         false otherwise
         */
        public bool IsNext(Parser parser)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (seq.IsNext(parser))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if the next token(s) in the parser match any token
         * sequence in this set.
         *
         * @param parser         the parser to check
         * @param length         the maximum number of tokens to check
         *
         * @return true if the next tokens are in the set, or
         *         false otherwise
         */
        public bool IsNext(Parser parser, int length)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (seq.IsNext(parser, length))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if another look-ahead set has an overlapping token
         * sequence. An overlapping token sequence is a token sequence
         * that is identical to another sequence, but for the length.
         * I.e. one of the two sequences may be longer than the other.
         *
         * @param set            the look-ahead set to check
         *
         * @return true if there is some token sequence that overlaps, or
         *         false otherwise
         */
        public bool IsOverlap(LookAheadSet set)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (set.IsOverlap(elements[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if a token sequence is overlapping. An overlapping token
         * sequence is a token sequence that is identical to another
         * sequence, but for the length. I.e. one of the two sequences may
         * be longer than the other.
         *
         * @param seq            the token sequence to check
         *
         * @return true if there is some token sequence that overlaps, or
         *         false otherwise
         */
        private bool IsOverlap(Sequence seq)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var elem = elements[i];
                if (seq.StartsWith(elem) || elem.StartsWith(seq))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if the specified token sequence is present in the
         * set.
         *
         * @param elem           the token sequence to check
         *
         * @return true if the sequence is present in this set, or
         *         false otherwise
         */
        private bool Contains(Sequence elem)
        {
            return FindSequence(elem) is not null;
        }

        /**
         * Checks if some token sequence is present in both this set
         * and a specified one.
         *
         * @param set            the look-ahead set to compare with
         *
         * @return true if the look-ahead sets intersect, or
         *         false otherwise
         */
        public bool Intersects(LookAheadSet set)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (set.Contains(elements[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Finds an identical token sequence if present in the set.
         *
         * @param elem           the token sequence to search for
         *
         * @return an identical the token sequence if found, or
         *         null if not found
         */
        private Sequence? FindSequence(Sequence elem)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (elements[i].Equals(elem))
                {
                    return elements[i];
                }
            }
            return null;
        }

        /**
         * Adds a token sequence to this set. The sequence will only
         * be added if it is not already in the set. Also, if the
         * sequence is longer than the allowed maximum, a truncated
         * sequence will be added instead.
         *
         * @param seq            the token sequence to add
         */
        private void Add(Sequence seq)
        {
            if (seq.Length() > maxLength)
            {
                seq = new Sequence(maxLength, seq);
            }
            if (Contains(seq) is false)
            {
                elements.Add(seq);
            }
        }

        /**
         * Adds a new token sequence with a single token to this set.
         * The sequence will only be added if it is not already in the
         * set.
         *
         * @param token          the token to add
         */
        public void Add(TokenId token)
        {
            Add(new Sequence(false, token));
        }

        /**
         * Adds all the token sequences from a specified set. Only
         * sequences not already in this set will be added.
         *
         * @param set            the set to add from
         */
        public void AddAll(LookAheadSet set)
        {
            for (int i = 0; i < set.elements.Count; i++)
            {
                Add(set.elements[i]);
            }
        }

        /**
         * Adds an empty token sequence to this set. The sequence will
         * only be added if it is not already in the set.
         */
        public void AddEmpty()
        {
            Add(new Sequence());
        }

        /**
         * Removes a token sequence from this set.
         *
         * @param seq            the token sequence to remove
         */
        private void Remove(Sequence seq)
        {
            elements.Remove(seq);
        }

        /**
         * Removes all the token sequences from a specified set. Only
         * sequences already in this set will be removed.
         *
         * @param set            the set to remove from
         */
        public void RemoveAll(LookAheadSet set)
        {
            for (var i = 0; i < set.elements.Count; i++)
            {
                Remove(set.elements[i]);
            }
        }

        /**
         * Creates a new look-ahead set that is the result of reading
         * the specified token. The new look-ahead set will contain
         * the rest of all the token sequences that started with the
         * specified token.
         *
         * @param token          the token to read
         *
         * @return a new look-ahead set containing the remaining tokens
         */
        public LookAheadSet CreateNextSet(TokenId token)
        {
            LookAheadSet result = new(maxLength - 1);

            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                var value = seq.GetToken(0);
                if (value is not null && token == value)
                {
                    result.Add(seq.Subsequence(1));
                }
            }
            return result;
        }

        /**
         * Creates a new look-ahead set that is the intersection of
         * this set with another set. The token sequences in the net
         * set will only have the repeat flag set if it was set in
         * both the identical token sequences.
         *
         * @param set            the set to intersect with
         *
         * @return a new look-ahead set containing the intersection
         */
        public LookAheadSet CreateIntersection(LookAheadSet set)
        {
            LookAheadSet result = new(maxLength);

            for (var i = 0; i < elements.Count; i++)
            {
                var seq1 = elements[i];
                var seq2 = set.FindSequence(seq1);
                if (seq2 is not null && seq1.IsRepetitive())
                {
                    result.Add(seq2);
                }
                else if (seq2 is not null)
                {
                    result.Add(seq1);
                }
            }
            return result;
        }

        /**
         * Creates a new look-ahead set that is the combination of
         * this set with another set. The combination is created by
         * creating new token sequences that consist of appending all
         * elements from the specified set onto all elements in this
         * set. This is sometimes referred to as the cartesian
         * product.
         *
         * @param set            the set to combine with
         *
         * @return a new look-ahead set containing the combination
         */
        public LookAheadSet CreateCombination(LookAheadSet set)
        {
            LookAheadSet result = new(maxLength);

            // Handle special cases
            if (Size() <= 0)
            {
                return set;
            }
            else if (set.Size() <= 0)
            {
                return this;
            }

            // Create combinations
            for (var i = 0; i < elements.Count; i++)
            {
                var first = elements[i];
                if (first.Length() >= maxLength)
                {
                    result.Add(first);
                }
                else if (first.Length() <= 0)
                {
                    result.AddAll(set);
                }
                else
                {
                    for (var j = 0; j < set.elements.Count; j++)
                    {
                        var second = set.elements[j];
                        result.Add(first.Concat(maxLength, second));
                    }
                }
            }
            return result;
        }

        /**
         * Creates a new look-ahead set with overlaps from another. All
         * token sequences in this set that overlaps with the other set
         * will be added to the new look-ahead set.
         *
         * @param set            the look-ahead set to check with
         *
         * @return a new look-ahead set containing the overlaps
         */
        public LookAheadSet CreateOverlaps(LookAheadSet set)
        {
            LookAheadSet result = new(maxLength);

            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (set.IsOverlap(seq))
                {
                    result.Add(seq);
                }
            }
            return result;
        }

        /**
         * Creates a new look-ahead set filter. The filter will contain
         * all sequences from this set, possibly left trimmed by each one
         * of the sequences in the specified set.
         *
         * @param set            the look-ahead set to trim with
         *
         * @return a new look-ahead set filter
         */
        public LookAheadSet CreateFilter(LookAheadSet set)
        {
            LookAheadSet result = new(maxLength);

            // Handle special cases
            if (Size() <= 0 || set.Size() <= 0)
            {
                return this;
            }

            // Create combinations
            for (var i = 0; i < elements.Count; i++)
            {
                var first = elements[i];
                for (int j = 0; j < set.elements.Count; j++)
                {
                    var second = set.elements[j];
                    if (first.StartsWith(second))
                    {
                        result.Add(first.Subsequence(second.Length()));
                    }
                }
            }
            return result;
        }

        /**
         * Creates a new identical look-ahead set, except for the
         * repeat flag being set in each token sequence.
         *
         * @return a new repetitive look-ahead set
         */
        public LookAheadSet CreateRepetitive()
        {
            LookAheadSet result = new(maxLength);

            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                if (seq.IsRepetitive())
                {
                    result.Add(seq);
                }
                else
                {
                    result.Add(new Sequence(true, seq));
                }
            }
            return result;
        }

        /**
         * Returns a string representation of this object.
         *
         * @return a string representation of this object
         */
        public override string ToString()
        {
            return ToString(null);
        }

        /**
         * Returns a string representation of this object.
         *
         * @param tokenizer      the tokenizer containing the tokens
         *
         * @return a string representation of this object
         */
        public string ToString(Tokenizer? tokenizer)
        {
            StringBuilder buffer = new();

            buffer.Append('{');
            for (var i = 0; i < elements.Count; i++)
            {
                var seq = elements[i];
                buffer.Append("\n  ");
                buffer.Append(seq.ToString(tokenizer));
            }
            buffer.Append("\n}");
            return buffer.ToString();
        }

        /**
         * A token sequence. This class contains a list of token ids.
         * It is immutable after creation, meaning that no changes
         * will be made to an instance after creation.
         *
    
         * @version  1.0
         */
        private sealed class Sequence
        {
            /**
             * The repeat flag. If this flag is set, the token
             * sequence or some part of it may be repeated infinitely.
             */
            private bool repeat;

            /**
             * The list of token ids in this sequence.
             */
            private readonly List<TokenId> tokens;

            /**
             * Creates a new empty token sequence. The repeat flag
             * will be set to false.
             */
            public Sequence()
            {
                repeat = false;
                tokens = new();
            }

            /**
             * Creates a new token sequence with a single token.
             *
             * @param repeat         the repeat flag value
             * @param token          the token to add
             */
            public Sequence(bool repeat, TokenId token)
            {
                this.repeat = repeat;
                tokens = new List<TokenId>(1) { token };
            }

            /**
             * Creates a new token sequence that is a duplicate of
             * another sequence. Only a limited number of tokens will
             * be copied however. The repeat flag from the original
             * will be kept intact.
             *
             * @param length         the maximum number of tokens to copy
             * @param seq            the sequence to copy
             */
            public Sequence(int length, Sequence seq)
            {
                repeat = seq.repeat;
                tokens = new List<TokenId>(length);
                if (seq.Length() < length)
                {
                    length = seq.Length();
                }
                for (int i = 0; i < length; i++)
                {
                    tokens.Add(seq.tokens[i]);
                }
            }

            /**
             * Creates a new token sequence that is a duplicate of
             * another sequence. The new value of the repeat flag will
             * be used however.
             *
             * @param repeat         the new repeat flag value
             * @param seq            the sequence to copy
             */
            public Sequence(bool repeat, Sequence seq)
            {
                this.repeat = repeat;
                tokens = seq.tokens;
            }

            /**
             * Returns the length of the token sequence.
             *
             * @return the number of tokens in the sequence
             */
            public int Length()
            {
                return tokens.Count;
            }

            /**
             * Returns a token at a specified position in the sequence.
             *
             * @param pos            the sequence position
             *
             * @return the token id found, or null
             */
            public TokenId? GetToken(int pos)
            {
                if (pos >= 0 && pos < tokens.Count)
                {
                    return tokens[pos];
                }
                else
                {
                    return null;
                }
            }

            /**
             * Checks if this sequence is equal to another object.
             * Only token sequences with the same tokens in the same
             * order will be considered equal. The repeat flag will be
             * disregarded.
             *
             * @param obj            the object to compare with
             *
             * @return true if the objects are equal, or
             *         false otherwise
             */
            public override bool Equals(object? obj)
            {
                if (obj is Sequence sequence)
                {
                    return Equals(sequence);
                }
                else
                {
                    return false;
                }
            }

            /**
             * Checks if this sequence is equal to another sequence.
             * Only sequences with the same tokens in the same order
             * will be considered equal. The repeat flag will be
             * disregarded.
             *
             * @param seq            the sequence to compare with
             *
             * @return true if the sequences are equal, or
             *         false otherwise
             */
            public bool Equals(Sequence seq)
            {
                return tokens.SequenceEqual(seq.tokens);
            }

            ///**
            // * Returns a hash code for this object.
            // *
            // * @return a hash code for this object
            // */
            //public override int GetHashCode()
            //{
            //    return tokens.Count.GetHashCode();
            //}


            /**
             * Checks if this token sequence starts with the tokens from
             * another sequence. If the other sequence is longer than this
             * sequence, this method will always return false.
             *
             * @param seq the token sequence to check
             *
             * @return true if this sequence starts with the other, or
             *         false otherwise
             */
            public bool StartsWith(Sequence seq)
            {
                if (Length() < seq.Length())
                {
                    return false;
                }
                for (var i = 0; i < seq.tokens.Count; i++)
                {
                    if (tokens[i].Equals(seq.tokens[i]) == false)
                    {
                        return false;
                    }
                }
                return true;
            }

            /**
             * Checks if this token sequence is repetitive. A repetitive
             * token sequence is one with the repeat flag set.
             *
             * @return true if this token sequence is repetitive, or
             *         false otherwise
             */
            public bool IsRepetitive()
            {
                return repeat;
            }

            /**
             * Checks if the next token(s) in the parser matches this
             * token sequence.
             *
             * @param parser         the parser to check
             *
             * @return true if the next tokens are in the sequence, or
             *         false otherwise
             */
            public bool IsNext(Parser parser)
            {
                Token token;

                for (int i = 0; i < tokens.Count; i++)
                {
                    var id = tokens[i];
                    token = parser.PeekToken(i);
                    if (token is null || token.TypeId != id)
                    {
                        return false;
                    }
                }
                return true;
            }

            /**
             * Checks if the next token(s) in the parser matches this
             * token sequence.
             *
             * @param parser         the parser to check
             * @param length         the maximum number of tokens to check
             *
             * @return true if the next tokens are in the sequence, or
             *         false otherwise
             */
            public bool IsNext(Parser parser, int length)
            {
                if (length > tokens.Count)
                {
                    length = tokens.Count;
                }
                for (var i = 0; i < length; i++)
                {
                    var id = tokens[i];
                    var token = parser.PeekToken(i);
                    if (token == null || token.TypeId != id)
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                Debugger.Break();
                //Todo: What is the point of this?
                return ToString(null);
            }

            /**
             * Returns a string representation of this object.
             *
             * @param tokenizer      the tokenizer containing the tokens
             *
             * @return a string representation of this object
             */
            public string ToString(Tokenizer? tokenizer)
            {
                StringBuilder buffer = new();

                if (tokenizer is null)
                {
                    buffer.Append(tokens.ToString());
                }
                else
                {
                    buffer.Append('[');
                    for (var i = 0; i < tokens.Count; i++)
                    {
                        var id = tokens[i];
                        var str = tokenizer.GetPatternDescription(id);
                        if (i > 0)
                        {
                            buffer.Append(' ');
                        }
                        buffer.Append(str);
                    }
                    buffer.Append(']');
                }
                if (repeat)
                {
                    buffer.Append(" *");
                }
                return buffer.ToString();
            }

            /**
             * Creates a new token sequence that is the concatenation
             * of this sequence and another. A maximum length for the
             * new sequence is also specified.
             *
             * @param length         the maximum length of the result
             * @param seq            the other sequence
             *
             * @return the concatenated token sequence
             */
            public Sequence Concat(int length, Sequence seq)
            {
                Sequence res = new(length, this);

                if (seq.repeat)
                {
                    res.repeat = true;
                }
                length -= Length();
                if (length > seq.Length())
                {
                    res.tokens.AddRange(seq.tokens);
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        res.tokens.Add(seq.tokens[i]);
                    }
                }
                return res;
            }

            /**
             * Creates a new token sequence that is a subsequence of
             * this one.
             *
             * @param start          the subsequence start position
             *
             * @return the new token subsequence
             */
            public Sequence Subsequence(int start)
            {
                Sequence res = new(Length(), this);

                while (start > 0 && res.tokens.Count > 0)
                {
                    res.tokens.RemoveAt(0);
                    start--;
                }
                return res;
            }
        }
    }
}

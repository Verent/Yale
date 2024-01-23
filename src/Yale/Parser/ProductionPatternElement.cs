namespace Yale.Parser
{
    /**
     * A production pattern element. This class represents a reference to
     * either a token or a production. Each element also contains minimum
     * and maximum occurrence counters, controlling the number of
     * repetitions allowed. A production pattern element is always
     * contained within a production pattern rule.
     */
    internal sealed class ProductionPatternElement
    {
        /**
         * Creates a new element. If the maximum value if zero (0) or
         * negative, it will be set to Int32.MaxValue.
         *
         * @param isToken        the token flag
         * @param id             the node identity
         * @param min            the minimum number of occupancies
         * @param max            the maximum number of occupancies, or
         *                       negative for infinite
         */
        public ProductionPatternElement(bool isToken, TokenId id, int min, int max)
        {
            IsToken = isToken;
            Id = id;
            if (min < 0)
            {
                min = 0;
            }
            MinCount = min;
            if (max <= 0)
            {
                max = int.MaxValue;
            }
            else if (max < min)
            {
                max = min;
            }
            MaxCount = max;
            LookAhead = null;
        }

        /**
         * The node identity property (read-only).
         */
        public TokenId Id { get; }

        /**
         * The minimum occurrence count property (read-only).
         */
        public int MinCount { get; }

        /**
         * The maximum occurrence count property (read-only).
         */
        public int MaxCount { get; }

        /**
         * The look-ahead set property. This is the look-ahead set
         * associated with this alternative.
         */
        internal LookAheadSet? LookAhead { get; set; }

        /**
         * Returns true if this element represents a token.
         */
        public bool IsToken { get; }

        /**
         * Returns true if this element represents a production.
         */
        public bool IsProduction()
        {
            return IsToken is false;
        }

        /**
         * Checks if a specific token matches this element. This
         * method will only return true if this element is a token
         * element, and the token has the same id and this element.
         *
         * @param token          the token to check
         *
         * @return true if the token matches this element, or
         *         false otherwise
         */
        public bool IsMatch(Token token)
        {
            return IsToken && token is not null && token.TypeId == Id;
        }

        /**
         * Checks if this object is equal to another. This method only
         * returns true for another identical production pattern
         * element.
         *
         * @param obj            the object to compare with
         *
         * @return true if the object is identical to this one, or
         *         false otherwise
         */
        public override bool Equals(object? obj)
        {
            if (obj is ProductionPatternElement element)
            {
                return IsToken == element.IsToken
                    && Id == element.Id
                    && MinCount == element.MinCount
                    && MaxCount == element.MaxCount;
            }
            else
            {
                return false;
            }
        }

        //public override int GetHashCode()
        //{
        //    return Id * 37;
        //}

        public override string ToString()
        {
            StringBuilder buffer = new();

            buffer.Append(Id);
            if (IsToken)
            {
                buffer.Append("(Token)");
            }
            else
            {
                buffer.Append("(Production)");
            }
            if (MinCount != 1 || MaxCount != 1)
            {
                buffer.Append('{');
                buffer.Append(MinCount);
                buffer.Append(',');
                buffer.Append(MaxCount);
                buffer.Append('}');
            }
            return buffer.ToString();
        }
    }
}

namespace Yale.Parser
{
    /**
     * A production pattern. This class represents a set of production
     * alternatives that together forms a single production. A
     * production pattern is identified by an integer id and a name,
     * both provided upon creation. The pattern id is used for
     * referencing the production pattern from production pattern
     * elements.
     *
     */
    internal sealed class ProductionPattern
    {
        /**
         * The list of production pattern alternatives.
         */
        private readonly List<object> alternatives;

        /**
         * The default production pattern alternative. This alternative
         * is used when no other alternatives match. It may be set to
         * -1, meaning that there is no default (or fallback) alternative.
         */
        private int defaultAlt;

        /**
         * Creates a new production pattern.
         *
         * @param id             the production pattern id
         * @param name           the production pattern name
         */
        public ProductionPattern(TokenId id, string name)
        {
            Id = id;
            Name = name;
            Synthetic = false;
            alternatives = new();
            defaultAlt = -1;
            LookAhead = null;
        }

        /**
         * The production pattern identity property (read-only). This
         * property contains the unique identity value.
         *
         */
        public TokenId Id { get; }

        /**
         * The production pattern name property (read-only).
         *
         */
        public string Name { get; }

        /**
         * The synthetic production pattern property. If this property
         * is set, the production identified by this pattern has been
         * artificially inserted into the grammar. No parse tree nodes
         * will be created for such nodes, instead the child nodes
         * will be added directly to the parent node. By default this
         * property is set to false.
         *
         */
        public bool Synthetic { get; set; }

        /**
         * The look-ahead set property. This property contains the
         * look-ahead set associated with this alternative.
         */
        internal LookAheadSet? LookAhead { get; set; }

        /**
         * The default pattern alternative property. The default
         * alternative is used when no other alternative matches. The
         * default alternative must previously have been added to the
         * list of alternatives. This property is set to null if no
         * default pattern alternative has been set.
         */
        internal ProductionPatternAlternative DefaultAlternative
        {
            get
            {
                if (defaultAlt >= 0)
                {
                    object obj = alternatives[defaultAlt];
                    return (ProductionPatternAlternative)obj;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                defaultAlt = 0;
                for (int i = 0; i < alternatives.Count; i++)
                {
                    if (alternatives[i] == value)
                    {
                        defaultAlt = i;
                    }
                }
            }
        }

        /**
         * The production pattern alternative count property
         * (read-only).
         *
         * @since 1.5
         */
        public int Count
        {
            get { return alternatives.Count; }
        }

        /**
         * The production pattern alternative index (read-only).
         *
         * @param index          the alternative index, 0 <= pos < Count
         *
         * @return the alternative found
         *
         * @since 1.5
         */
        public ProductionPatternAlternative this[int index]
        {
            get { return (ProductionPatternAlternative)alternatives[index]; }
        }

        /**
         * Returns an alternative in this pattern.
         *
         * @param pos            the alternative position, 0 <= pos < count
         *
         * @return the alternative found
         *
         * @deprecated Use the class indexer instead.
         */
        public ProductionPatternAlternative GetAlternative(int pos)
        {
            return this[pos];
        }

        /**
         * Checks if this pattern is recursive on the left-hand side.
         * This method checks if any of the production pattern
         * alternatives is left-recursive.
         *
         * @return true if at least one alternative is left recursive, or
         *         false otherwise
         */
        public bool IsLeftRecursive()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)alternatives[i];
                if (alt.IsLeftRecursive())
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if this pattern is recursive on the right-hand side.
         * This method checks if any of the production pattern
         * alternatives is right-recursive.
         *
         * @return true if at least one alternative is right recursive, or
         *         false otherwise
         */
        public bool IsRightRecursive()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)alternatives[i];
                if (alt.IsRightRecursive())
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if this pattern would match an empty stream of
         * tokens. This method checks if any one of the production
         * pattern alternatives would match the empty token stream.
         *
         * @return true if at least one alternative match no tokens, or
         *         false otherwise
         */
        public bool IsMatchingEmpty()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)alternatives[i];
                if (alt.IsMatchingEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Adds a production pattern alternative.
         *
         * @param alt            the production pattern alternative to add
         *
         * @throws ParserCreationException if an identical alternative has
         *             already been added
         */
        public void AddAlternative(ProductionPatternAlternative alt)
        {
            if (alternatives.Contains(alt))
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.InvalidProduction,
                    Name,
                    "two identical alternatives exist"
                );
            }
            alt.SetPattern(this);
            alternatives.Add(alt);
        }

        /**
         * Returns a string representation of this object.
         *
         * @return a token string representation
         */
        public override string ToString()
        {
            StringBuilder buffer = new();
            StringBuilder indent = new();
            int i;

            buffer.Append(Name);
            buffer.Append('(');
            buffer.Append(Id);
            buffer.Append(") ");
            for (i = 0; i < buffer.Length; i++)
            {
                indent.Append(' ');
            }
            for (i = 0; i < alternatives.Count; i++)
            {
                if (i == 0)
                {
                    buffer.Append("= ");
                }
                else
                {
                    buffer.Append('\n');
                    buffer.Append(indent);
                    buffer.Append("| ");
                }
                buffer.Append(alternatives[i]);
            }
            return buffer.ToString();
        }
    }
}

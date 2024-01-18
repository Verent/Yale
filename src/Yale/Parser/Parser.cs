using System;
using System.IO;

namespace Yale.Parser
{
    /**
     * A base parser class. This class provides the standard parser
     * interface, as well as token handling.
     *
     * @author   Per Cederberg
     * @version  1.6
     */
    internal abstract class Parser
    {
        /**
         * The parser initialization flag.
         */
        protected bool initialized;

        /**
         * The list of production patterns.
         */
        protected readonly List<ProductionPattern> Patterns = new();

        /**
         * The map with production patterns and their id:s. This map
         * contains the production patterns indexed by their id:s.
         */
        private readonly Dictionary<TokenId, ProductionPattern> patternIds = new();

        /**
         * The list of buffered tokens. This list will contain tokens that
         * have been read from the tokenizer, but not yet consumed.
         */
        private readonly List<Token> tokens = new();

        /**
         * The error log. All parse errors will be added to this log as
         * the parser attempts to recover from the error. If the error
         * count is higher than zero (0), this log will be thrown as the
         * result from the parse() method.
         */
        private ParserLogException errorLog = new();

        /**
         * The error recovery counter. This counter is initially set to a
         * negative value to indicate that no error requiring recovery
         * has been encountered. When a parse error is found, the counter
         * is set to three (3), and is then decreased by one for each
         * correctly read token until it reaches zero (0).
         */
        private int errorRecovery = -1;

        /**
         * Creates a new parser.
         *
         * @param tokenizer       the tokenizer to use
         * @param analyzer        the analyzer callback to use
         */
        internal Parser(Tokenizer tokenizer, Analyzer analyzer)
        {
            Tokenizer = tokenizer;
            Analyzer = analyzer;
        }

        /**
         * The tokenizer property (read-only). This property contains
         * the tokenizer in use by this parser.
         *
         * @since 1.5
         */
        public Tokenizer Tokenizer { get; }

        /**
         * The analyzer property (read-only). This property contains
         * the analyzer in use by this parser.
         *
         * @since 1.5
         */
        public Analyzer Analyzer { get; }

        /**
         * Adds a new production pattern to the parser. The first pattern
         * added is assumed to be the starting point in the grammar. The
         * patterns added may be validated to some extent.
         *
         * @param pattern        the pattern to add
         *
         * @throws ParserCreationException if the pattern couldn't be
         *             added correctly to the parser
         */
        public virtual void AddPattern(ProductionPattern pattern)
        {
            if (pattern.Count <= 0)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.InvalidProduction,
                    pattern.Name,
                    "no production alternatives are present (must have at least one)"
                );
            }
            if (patternIds.ContainsKey(pattern.Id))
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.InvalidProduction,
                    pattern.Name,
                    "another pattern with the same id (" + pattern.Id + ") has already been added"
                );
            }
            Patterns.Add(pattern);
            patternIds.Add(pattern.Id, pattern);
            initialized = false;
        }

        /**
         * Initializes the parser. All the added production patterns will
         * be analyzed for ambiguities and errors. This method also
         * initializes internal data structures used during the parsing.
         *
         * @throws ParserCreationException if the parser couldn't be
         *             initialized correctly
         */
        public virtual void Prepare()
        {
            if (Patterns.Count <= 0)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.InvalidParser,
                    "no production patterns have been added"
                );
            }
            for (var i = 0; i < Patterns.Count; i++)
            {
                CheckPattern(Patterns[i]);
            }
            initialized = true;
        }

        /**
         * Checks a production pattern for completeness. If some rule
         * in the pattern referenced an production pattern not added
         * to this parser, a parser creation exception will be thrown.
         *
         * @param pattern        the production pattern to check
         *
         * @throws ParserCreationException if the pattern referenced a
         *             pattern not added to this parser
         */
        private void CheckPattern(ProductionPattern pattern)
        {
            for (var i = 0; i < pattern.Count; i++)
            {
                CheckAlternative(pattern.Name, pattern[i]);
            }
        }

        /**
         * Checks a production pattern alternative for completeness.
         * If some element in the alternative referenced a production
         * pattern not added to this parser, a parser creation
         * exception will be thrown.
         *
         * @param name           the name of the pattern being checked
         * @param alt            the production pattern alternative
         *
         * @throws ParserCreationException if the alternative
         *             referenced a pattern not added to this parser
         */
        private void CheckAlternative(string name, ProductionPatternAlternative alt)
        {
            for (var i = 0; i < alt.Count; i++)
            {
                CheckElement(name, alt[i]);
            }
        }

        /**
         * Checks a production pattern element for completeness. If
         * the element references a production pattern not added to
         * this parser, a parser creation exception will be thrown.
         *
         * @param name           the name of the pattern being checked
         * @param elem           the production pattern element to check
         *
         * @throws ParserCreationException if the element referenced a
         *             pattern not added to this parser
         */
        private void CheckElement(string name, ProductionPatternElement elem)
        {
            if (elem.IsProduction() && GetPattern(elem.Id) is null)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.InvalidProduction,
                    name,
                    "an undefined production pattern id (" + elem.Id + ") is referenced"
                );
            }
        }

        /**
         * Resets this parser for usage with another input stream. The
         * associated tokenizer and analyzer will also be reset. This
         * method will clear all the internal state and the error log in
         * the parser. It is normally called in order to reuse a parser
         * and tokenizer pair with multiple input streams, thereby
         * avoiding the cost of re-analyzing the grammar structures.
         *
         * @param input          the new input stream to read
         *
         * @see Tokenizer#Reset
         * @see Analyzer#Reset
         *
         * @since 1.5
         */
        public void Reset(TextReader input)
        {
            Tokenizer.Reset(input);
            Analyzer.Reset();
        }

        /**
         * Parses the token stream and returns a parse tree. This
         * method will call Prepare() if not previously called. It
         * will also call the Reset() method, to make sure that only
         * the Tokenizer.Reset() method must be explicitly called in
         * order to reuse a parser for multiple input streams. In case
         * of a parse error, the parser will attempt to recover and
         * throw all the errors found in a parser log exception in the
         * end.
         *
         * @return the parse tree
         *
         * @throws ParserCreationException if the parser couldn't be
         *             initialized correctly
         * @throws ParserLogException if the input couldn't be parsed
         *             correctly
         *
         * @see #Prepare
         * @see #Reset
         * @see Tokenizer#Reset
         */
        public Node Parse()
        {
            Node? root = default;

            // Initialize parser
            if (initialized == false)
            {
                Prepare();
            }
            tokens.Clear();
            errorLog = new ParserLogException();
            errorRecovery = -1;

            // Parse input
            try
            {
                root = ParseStart();
            }
            catch (ParseException e)
            {
                AddError(e, true);
            }

            // Check for errors
            if (errorLog.Count > 0)
            {
                throw errorLog;
            }

            //Todo: Fix this
            return root!;
        }

        /**
         * Parses the token stream and returns a parse tree.
         *
         * @return the parse tree
         *
         * @throws ParseException if the input couldn't be parsed
         *             correctly
         */
        protected abstract Node ParseStart();

        /**
         * Factory method to create a new production node. This method
         * can be overridden to provide other production implementations
         * than the default one.
         *
         * @param pattern        the production pattern
         *
         * @return the new production node
         *
         * @since 1.5
         */
        protected virtual Production NewProduction(ProductionPattern pattern)
        {
            return Analyzer.NewProduction(pattern);
        }

        /**
         * Adds an error to the error log. If the parser is in error
         * recovery mode, the error will not be added to the log. If the
         * recovery flag is set, this method will set the error recovery
         * counter thus enter error recovery mode. Only lexical or
         * syntactical errors require recovery, so this flag shouldn't be
         * set otherwise.
         *
         * @param e              the error to add
         * @param recovery       the recover flag
         */
        internal void AddError(ParseException e, bool recovery)
        {
            if (errorRecovery <= 0)
            {
                errorLog.AddError(e);
            }
            if (recovery)
            {
                errorRecovery = 3;
            }
        }

        /**
         * Returns the production pattern with the specified id.
         *
         * @param id             the production pattern id
         *
         * @return the production pattern found, or
         *         null if non-existent
         */
        internal ProductionPattern GetPattern(TokenId id)
        {
            return patternIds[id];
        }

        /**
         * Returns the production pattern for the starting production.
         *
         * @throws ArgumentOutOfRangeException if there is not patterns
         */
        internal ProductionPattern GetStartPattern()
        {
            return Patterns[0];
        }

        /**
         * Handles the parser entering a production. This method calls the
         * appropriate analyzer callback if the node is not hidden. Note
         * that this method will not call any callback if an error
         * requiring recovery has occurred.
         *
         * @param node           the parse tree node
         */
        internal void EnterNode(Token token)
        {
            if (token.IsHidden() == false && errorRecovery < 0)
            {
                try
                {
                    Analyzer.Enter(token);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
        }

        internal void EnterNode(Production production)
        {
            if (production.IsHidden() == false && errorRecovery < 0)
            {
                try
                {
                    Analyzer.Enter(production);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
        }

        /**
         * Handles the parser leaving a production. This method calls the
         * appropriate analyzer callback if the node is not hidden, and
         * returns the result. Note that this method will not call any
         * callback if an error requiring recovery has occurred.
         *
         * @param node           the parse tree node
         *
         * @return the parse tree node, or
         *         null if no parse tree should be created
         */

        internal Token ExitNode(Token token)
        {
            if (token.IsHidden() == false && errorRecovery < 0)
            {
                try
                {
                    return Analyzer.Exit(token);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
            return token;
        }

        internal Production ExitNode(Production production)
        {
            if (production.IsHidden() == false && errorRecovery < 0)
            {
                try
                {
                    return Analyzer.Exit(production);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
            return production;
        }

        /**
         * Handles the parser adding a child node to a production. This
         * method calls the appropriate analyzer callback. Note that this
         * method will not call any callback if an error requiring
         * recovery has occurred.
         *
         * @param node           the parent parse tree node
         * @param child          the child parse tree node, or null
         */
        internal void AddNode(Production node, Node? child)
        {
            if (errorRecovery >= 0)
                return;
            else if (node.IsHidden())
            {
                if (child is not null)
                {
                    node.AddChild(child);
                }
            }
            else if (child is not null && child.IsHidden())
            {
                for (var i = 0; i < child.Count; i++)
                {
                    AddNode(node, child[i]);
                }
            }
            else
            {
                try
                {
                    Analyzer.Child(node, child);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
        }

        /**
         * Reads and consumes the next token in the queue. If no token
         * was available for consummation, a parse error will be
         * thrown.
         *
         * @return the token consumed
         *
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        internal Token NextToken()
        {
            Token? token = PeekToken(0);

            if (token is not null)
            {
                tokens.RemoveAt(0);
                return token;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.UnexpectedEof,
                    null,
                    Tokenizer.GetCurrentLine(),
                    Tokenizer.GetCurrentColumn()
                );
            }
        }

        /**
         * Reads and consumes the next token in the queue. If no token was
         * available for consummation, a parse error will be thrown. A
         * parse error will also be thrown if the token id didn't match
         * the specified one.
         *
         * @param id             the expected token id
         *
         * @return the token consumed
         *
         * @throws ParseException if the input stream couldn't be parsed
         *             correctly, or if the token wasn't expected
         */
        internal Token NextToken(TokenId id)
        {
            Token token = NextToken();
            List<string> list;

            if (token.TypeId == id)
            {
                if (errorRecovery > 0)
                {
                    errorRecovery--;
                }
                return token;
            }
            else
            {
                list = new List<string> { Tokenizer.GetPatternDescription(id) };
                throw new ParseException(
                    ParseException.ErrorType.UnexpectedToken,
                    token.ToShortString(),
                    list,
                    token.StartLine,
                    token.StartColumn
                );
            }
        }

        /**
         * Returns a token from the queue. This method is used to check
         * coming tokens before they have been consumed. Any number of
         * tokens forward can be checked.
         *
         * @param steps          the token queue number, zero (0) for first
         *
         * @return the token in the queue, or
         *         null if no more tokens in the queue
         */
        internal Token? PeekToken(int steps)
        {
            while (steps >= tokens.Count)
            {
                try
                {
                    var token = Tokenizer.Next();
                    if (token is null)
                    {
                        return null;
                    }
                    else
                    {
                        tokens.Add(token);
                    }
                }
                catch (ParseException e)
                {
                    AddError(e, true);
                }
            }
            return tokens[steps];
        }

        /**
         * Returns a string representation of this parser. The string will
         * contain all the production definitions and various additional
         * information.
         *
         * @return a detailed string representation of this parser
         */
        public override string ToString()
        {
            StringBuilder buffer = new();

            for (var i = 0; i < Patterns.Count; i++)
            {
                buffer.Append(ToString(Patterns[i]));
                buffer.Append('\n');
            }
            return buffer.ToString();
        }

        /**
         * Returns a string representation of a production pattern.
         *
         * @param prod           the production pattern
         *
         * @return a detailed string representation of the pattern
         */
        private string ToString(ProductionPattern prod)
        {
            StringBuilder buffer = new();
            StringBuilder indent = new();

            buffer.Append(prod.Name);
            buffer.Append(" (");
            buffer.Append(prod.Id);
            buffer.Append(") ");
            for (var i = 0; i < buffer.Length; i++)
            {
                indent.Append(' ');
            }
            buffer.Append("= ");
            indent.Append("| ");
            for (var i = 0; i < prod.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(indent);
                }
                buffer.Append(ToString(prod[i]));
                buffer.Append('\n');
            }
            for (var i = 0; i < prod.Count; i++)
            {
                var set = prod[i].LookAhead;
                if (set.GetMaxLength() > 1)
                {
                    buffer.Append("Using ");
                    buffer.Append(set.GetMaxLength());
                    buffer.Append(" token look-ahead for alternative ");
                    buffer.Append(i + 1);
                    buffer.Append(": ");
                    buffer.Append(set.ToString(Tokenizer));
                    buffer.Append('\n');
                }
            }
            return buffer.ToString();
        }

        /**
         * Returns a string representation of a production pattern
         * alternative.
         *
         * @param alt            the production pattern alternative
         *
         * @return a detailed string representation of the alternative
         */
        private string ToString(ProductionPatternAlternative alt)
        {
            StringBuilder buffer = new();

            for (var i = 0; i < alt.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(' ');
                }
                buffer.Append(ToString(alt[i]));
            }
            return buffer.ToString();
        }

        /**
         * Returns a string representation of a production pattern
         * element.
         *
         * @param elem           the production pattern element
         *
         * @return a detailed string representation of the element
         */
        private string ToString(ProductionPatternElement elem)
        {
            StringBuilder buffer = new();
            int min = elem.MinCount;
            int max = elem.MaxCount;

            if (min == 0 && max == 1)
            {
                buffer.Append('[');
            }
            if (elem.IsToken)
            {
                buffer.Append(GetTokenDescription(elem.Id));
            }
            else
            {
                buffer.Append(GetPattern(elem.Id).Name);
            }
            if (min == 0 && max == 1)
            {
                buffer.Append(']');
            }
            else if (min == 0 && max == int.MaxValue)
            {
                buffer.Append('*');
            }
            else if (min == 1 && max == int.MaxValue)
            {
                buffer.Append('+');
            }
            else if (min != 1 || max != 1)
            {
                buffer.Append('{');
                buffer.Append(min);
                buffer.Append(',');
                buffer.Append(max);
                buffer.Append('}');
            }
            return buffer.ToString();
        }

        /**
         * Returns a token description for a specified token.
         *
         * @param token the token to describe
         *
         * @return the token description
         */
        internal string? GetTokenDescription(TokenId token)
        {
            return Tokenizer?.GetPatternDescription(token) ?? string.Empty;
        }
    }
}

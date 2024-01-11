/*
 * Tokenizer.cs
 *
 * This program is free software: you can redistribute it and/or
 * modify it under the terms of the BSD license.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * LICENSE.txt file for more details.
 *
 * Copyright (c) 2003-2015 Per Cederberg. All rights reserved.
 */

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Yale.Parser.RE;

namespace Yale.Parser
{
    /**
     * A character stream tokenizer. This class groups the characters read
     * from the stream together into tokens ("words"). The grouping is
     * controlled by token patterns that contain either a fixed string to
     * search for, or a regular expression. If the stream of characters
     * don't match any of the token patterns, a parse exception is thrown.
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    internal class Tokenizer
    {
        /**
         * The string DFA token matcher. This token matcher uses a
         * deterministic finite automaton (DFA) implementation and is
         * used for all string token patterns. It has a slight speed
         * advantage to the NFA implementation, but should be equivalent
         * on memory usage.
         */
        private readonly StringDFAMatcher stringDfaMatcher;

        /**
         * The regular expression NFA token matcher. This token matcher
         * uses a non-deterministic finite automaton (DFA) implementation
         * and is used for most regular expression token patterns. It is
         * somewhat faster than the other recursive regular expression
         * implementations available, but doesn't support the full
         * syntax. It conserves memory by using a fast queue instead of
         * the stack during processing (no stack overflow).
         */
        private readonly NFAMatcher nfaMatcher;

        /**
         * The regular expression token matcher. This token matcher is
         * used for complex regular expressions, but should be avoided
         * due to possibly degraded speed and memory usage compared to
         * the automaton implementations.
         */
        private readonly RegExpMatcher regExpMatcher;

        /**
        * The character stream reader buffer.
        */
        private ReaderBuffer buffer;

        /**
        * The last token match found.
        */
        private readonly TokenMatch lastMatch = new();

        /**
         * The previous token in the token list.
         */
        private Token? previousToken;

        /**
         * Creates a new case-sensitive tokenizer for the specified
         * input stream.
         *
         * @param input          the input stream to read
         */
        public Tokenizer(TextReader input)
            : this(input, false) { }

        /**
         * Creates a new tokenizer for the specified input stream. The
         * tokenizer can be set to process tokens either in
         * case-sensitive or case-insensitive mode.
         *
         * @param input          the input stream to read
         * @param ignoreCase     the character case ignore flag
         *
         * @since 1.5
         */
        public Tokenizer(TextReader input, bool ignoreCase)
        {
            stringDfaMatcher = new StringDFAMatcher(ignoreCase);
            nfaMatcher = new NFAMatcher(ignoreCase);
            regExpMatcher = new RegExpMatcher(ignoreCase);
            buffer = new ReaderBuffer(input);
        }

        /**
         * The token list flag property. If the token list flag is
         * set, all tokens (including ignored tokens) link to each
         * other in a double-linked list. By default the token list
         * flag is set to false.
         *
         * @see Token#Previous
         * @see Token#Next
         *
         * @since 1.5
         */
        public bool UseTokenList { get; set; }

        /**
         * Returns a description of the token pattern with the
         * specified id.
         *
         * @param id             the token pattern id
         *
         * @return the token pattern description, or
         *         null if not present
         */
        public string? GetPatternDescription(TokenId id)
        {
            var pattern = stringDfaMatcher.GetPattern(id)
                ?? nfaMatcher.GetPattern(id)
                ?? regExpMatcher.GetPattern(id);

            return pattern?.ToShortString();
        }

        /**
         * Returns the current line number. This number will be the line
         * number of the next token returned.
         *
         * @return the current line number
         */
        public int GetCurrentLine()
        {
            return buffer.LineNumber;
        }

        /**
         * Returns the current column number. This number will be the
         * column number of the next token returned.
         *
         * @return the current column number
         */
        public int GetCurrentColumn()
        {
            return buffer.ColumnNumber;
        }

        /**
         * Adds a new token pattern to the tokenizer. The pattern will be
         * added last in the list, choosing a previous token pattern in
         * case two matches the same string.
         *
         * @param pattern        the pattern to add
         *
         * @throws ParserCreationException if the pattern couldn't be
         *             added to the tokenizer
         */
        public void AddPattern(TokenPattern pattern)
        {
            switch (pattern.Type)
            {
                case TokenPattern.PatternType.STRING:
                    try
                    {
                        stringDfaMatcher.AddPattern(pattern);
                    }
                    catch (Exception e)
                    {
                        throw new ParserCreationException(
                            ParserCreationException.ErrorType.InvalidToken,
                            pattern.Name,
                            "error adding string token: " + e.Message
                        );
                    }
                    break;
                case TokenPattern.PatternType.REGEXP:
                    try
                    {
                        nfaMatcher.AddPattern(pattern);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            regExpMatcher.AddPattern(pattern);
                        }
                        catch (Exception e)
                        {
                            throw new ParserCreationException(
                                ParserCreationException.ErrorType.InvalidToken,
                                pattern.Name,
                                "regular expression contains error(s): " + e.Message
                            );
                        }
                    }
                    break;
                default:
                    throw new ParserCreationException(
                        ParserCreationException.ErrorType.InvalidToken,
                        pattern.Name,
                        "pattern type " + pattern.Type + " is undefined"
                    );
            }
        }

        /**
         * Resets this tokenizer for usage with another input stream.
         * This method will clear all the internal state in the
         * tokenizer as well as close the previous input stream. It
         * is normally called in order to reuse a parser and
         * tokenizer pair with multiple input streams, thereby
         * avoiding the cost of re-analyzing the grammar structures.
         *
         * @param input          the new input stream to read
         *
         * @see Parser#reset(Reader)
         *
         * @since 1.5
         */
        public void Reset(TextReader input)
        {
            buffer.Dispose();
            buffer = new ReaderBuffer(input);
            previousToken = null;
            lastMatch.Clear();
        }

        /**
         * Finds the next token on the stream. This method will return
         * null when end of file has been reached. It will return a
         * parse exception if no token matched the input stream, or if
         * a token pattern with the error flag set matched. Any tokens
         * matching a token pattern with the ignore flag set will be
         * silently ignored and the next token will be returned.
         *
         * @return the next token found, or
         *         null if end of file was encountered
         *
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        public Token? Next()
        {
            Token? token;

            do
            {
                token = NextToken();
                if (token is null)
                {
                    previousToken = null;
                    return null;
                }
                if (UseTokenList)
                {
                    token.Previous = previousToken;
                    previousToken = token;
                }
                if (token.Pattern.Ignore)
                {
                    token = null;
                }
                else if (token.Pattern.Error)
                {
                    throw new ParseException(
                        ParseException.ErrorType.InvalidToken,
                        token.Pattern.ErrorMessage,
                        token.StartLine,
                        token.StartColumn
                    );
                }
            } while (token is null);
            return token;
        }

        /**
         * Finds the next token on the stream. This method will return
         * null when end of file has been reached. It will return a
         * parse exception if no token matched the input stream.
         *
         * @return the next token found, or
         *         null if end of file was encountered
         *
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        private Token NextToken()
        {
            string str;
            int line;
            int column;

            try
            {
                lastMatch.Clear();
                stringDfaMatcher.Match(buffer, lastMatch);
                nfaMatcher.Match(buffer, lastMatch);
                regExpMatcher.Match(buffer, lastMatch);
                if (lastMatch.Length > 0)
                {
                    line = buffer.LineNumber;
                    column = buffer.ColumnNumber;
                    str = buffer.Read(lastMatch.Length);
                    return NewToken(lastMatch.Pattern, str, line, column);
                }
                else if (buffer.Peek(0) < 0)
                {
                    return null;
                }
                else
                {
                    line = buffer.LineNumber;
                    column = buffer.ColumnNumber;
                    throw new ParseException(
                        ParseException.ErrorType.UnexpectedChar,
                        buffer.Read(1),
                        line,
                        column
                    );
                }
            }
            catch (IOException e)
            {
                throw new ParseException(ParseException.ErrorType.IO, e.Message, -1, -1);
            }
        }

        /**
         * Factory method for creating a new token. This method can be
         * overridden to provide other token implementations than the
         * default one.
         *
         * @param pattern        the token pattern
         * @param image          the token image (i.e. characters)
         * @param line           the line number of the first character
         * @param column         the column number of the first character
         *
         * @return the token created
         *
         * @since 1.5
         */
        protected virtual Token NewToken(TokenPattern pattern, string image, int line, int column)
        {
            return new Token(pattern, image, line, column);
        }

        /**
         * Returns a string representation of this object. The returned
         * string will contain the details of all the token patterns
         * contained in this tokenizer.
         *
         * @return a detailed string representation
         */
        public override string ToString()
        {
            StringBuilder buffer = new();

            buffer.Append(stringDfaMatcher);
            buffer.Append(nfaMatcher);
            buffer.Append(regExpMatcher);
            return buffer.ToString();
        }
    }

    /**
     * A token pattern matcher. This class is the base class for the
     * various types of token matchers that exist. The token matcher
     * checks for matches with the tokenizer buffer, and maintains the
     * state of the last match.
     */
    internal abstract class TokenMatcher
    {
        /**
         * The array of token patterns.
         */
        protected TokenPattern[] patterns = Array.Empty<TokenPattern>();

        /**
         * The ignore character case flag.
         */
        protected bool ignoreCase;

        /**
         * Creates a new token matcher.
         *
         * @param ignoreCase      the character case ignore flag
         */
        public TokenMatcher(bool ignoreCase)
        {
            this.ignoreCase = ignoreCase;
        }

        /**
         * Searches for matching token patterns at the start of the
         * input stream. If a match is found, the token match object
         * is updated.
         *
         * @param buffer         the input buffer to check
         * @param match          the token match to update
         *
         * @throws IOException if an I/O error occurred
         */
        public abstract void Match(ReaderBuffer buffer, TokenMatch match);

        /**
         * Returns the token pattern with the specified id. Only
         * token patterns handled by this matcher can be returned.
         *
         * @param id         the token pattern id
         *
         * @return the token pattern found, or
         *         null if not found
         */
        public TokenPattern? GetPattern(TokenId id)
        {
            for (var i = 0; i < patterns.Length; i++)
            {
                if (patterns[i].Id == id)
                {
                    return patterns[i];
                }
            }
            return null;
        }

        /**
         * Adds a string token pattern to this matcher.
         *
         * @param pattern        the pattern to add
         *
         * @throws Exception if the pattern couldn't be added to the matcher
         */
        public virtual void AddPattern(TokenPattern pattern)
        {
            Array.Resize(ref patterns, patterns.Length + 1);
            patterns[patterns.Length - 1] = pattern;
        }

        /**
         * Returns a string representation of this matcher. This will
         * contain all the token patterns.
         *
         * @return a detailed string representation of this matcher
         */
        public override string ToString()
        {
            StringBuilder buffer = new();

            for (int i = 0; i < patterns.Length; i++)
            {
                buffer.Append(patterns[i]);
                buffer.Append("\n\n");
            }
            return buffer.ToString();
        }
    }

    /**
     * A token pattern matcher using a DFA for string tokens. This
     * class only supports string tokens and must be complemented
     * with another matcher for regular expressions. Internally it
     * uses a DFA to provide high performance.
     */
    internal sealed class StringDFAMatcher : TokenMatcher
    {
        /**
         * The deterministic finite state automaton used for
         * matching.
         */
        private TokenStringDFA automaton = new();

        /**
         * Creates a new string token matcher.
         *
         * @param ignoreCase      the character case ignore flag
         */
        public StringDFAMatcher(bool ignoreCase)
            : base(ignoreCase) { }

        /**
         * Adds a string token pattern to this matcher.
         *
         * @param pattern        the pattern to add
         */
        public override void AddPattern(TokenPattern pattern)
        {
            automaton.AddMatch(pattern.Pattern, ignoreCase, pattern);
            base.AddPattern(pattern);
        }

        /**
         * Searches for matching token patterns at the start of the
         * input stream. If a match is found, the token match object
         * is updated.
         *
         * @param buffer         the input buffer to check
         * @param match          the token match to update
         *
         * @throws IOException if an I/O error occurred
         */
        public override void Match(ReaderBuffer buffer, TokenMatch match)
        {
            TokenPattern res = automaton.Match(buffer, ignoreCase);

            if (res is not null)
            {
                match.Update(res.Pattern.Length, res);
            }
        }
    }

    /**
     * A token pattern matcher using a NFA for both string and
     * regular expression tokens. This class has limited support for
     * regular expressions and must be complemented with another
     * matcher providing full regular expression support. Internally
     * it uses a NFA to provide high performance and low memory
     * usage.
     */
    internal sealed class NFAMatcher : TokenMatcher
    {
        /**
         * The non-deterministic finite state automaton used for
         * matching.
         */
        private readonly TokenNFA automaton = new();

        /**
         * Creates a new NFA token matcher.
         *
         * @param ignoreCase      the character case ignore flag
         */
        public NFAMatcher(bool ignoreCase)
            : base(ignoreCase) { }

        /**
         * Adds a token pattern to this matcher.
         *
         * @param pattern        the pattern to add
         *
         * @throws Exception if the pattern couldn't be added to the matcher
         */
        public override void AddPattern(TokenPattern pattern)
        {
            if (pattern.Type == TokenPattern.PatternType.STRING)
            {
                automaton.AddTextMatch(pattern.Pattern, ignoreCase, pattern);
            }
            else
            {
                automaton.AddRegExpMatch(pattern.Pattern, ignoreCase, pattern);
            }
            base.AddPattern(pattern);
        }

        /**
         * Searches for matching token patterns at the start of the
         * input stream. If a match is found, the token match object
         * is updated.
         *
         * @param buffer         the input buffer to check
         * @param match          the token match to update
         *
         * @throws IOException if an I/O error occurred
         */
        public override void Match(ReaderBuffer buffer, TokenMatch match)
        {
            automaton.Match(buffer, match);
        }
    }

    /**
     * A token pattern matcher for complex regular expressions. This
     * class only supports regular expression tokens and must be
     * complemented with another matcher for string tokens.
     * Internally it uses the Grammatica RE package for high
     * performance or the native java.util.regex package for maximum
     * compatibility.
     */
    internal sealed class RegExpMatcher : TokenMatcher
    {
        /**
         * The regular expression handlers.
         */
        private REHandler[] regExps = Array.Empty<REHandler>();

        /**
         * Creates a new regular expression token matcher.
         *
         * @param ignoreCase      the character case ignore flag
         */
        public RegExpMatcher(bool ignoreCase)
            : base(ignoreCase) { }

        /**
         * Adds a regular expression token pattern to this matcher.
         *
         * @param pattern        the pattern to add
         *
         * @throws Exception if the pattern couldn't be added to the matcher
         */
        public override void AddPattern(TokenPattern pattern)
        {
            REHandler re;

            try
            {
                re = new GrammaticaRE(pattern.Pattern, ignoreCase);
                pattern.DebugInfo = "Grammatica regexp\n" + re;
            }
            catch (Exception)
            {
                re = new SystemRE(pattern.Pattern, ignoreCase);
                pattern.DebugInfo = "native .NET regexp";
            }
            Array.Resize(ref regExps, regExps.Length + 1);
            regExps[^1] = re;
            base.AddPattern(pattern);
        }

        /**
         * Searches for matching token patterns at the start of the
         * input stream. If a match is found, the token match object
         * is updated.
         *
         * @param buffer         the input buffer to check
         * @param match          the token match to update
         *
         * @throws IOException if an I/O error occurred
         */
        public override void Match(ReaderBuffer buffer, TokenMatch match)
        {
            for (int i = 0; i < regExps.Length; i++)
            {
                int length = regExps[i].Match(buffer);
                if (length > 0)
                {
                    match.Update(length, patterns[i]);
                }
            }
        }
    }

    /**
     * The regular expression handler base class.
     */
    internal abstract class REHandler
    {
        /**
         * Checks if the start of the input stream matches this
         * regular expression.
         *
         * @param buffer         the input buffer to check
         *
         * @return the longest match found, or
         *         zero (0) if no match was found
         *
         * @throws IOException if an I/O error occurred
         */
        public abstract int Match(ReaderBuffer buffer);
    }

    /**
     * The Grammatica built-in regular expression handler.
     */
    internal sealed class GrammaticaRE : REHandler
    {
        /**
         * The compiled regular expression.
         */
        private readonly RegExp regExp;

        /**
         * The regular expression matcher to use.
         */
        private Matcher? matcher;

        /**
         * Creates a new Grammatica regular expression handler.
         *
         * @param regex          the regular expression text
         * @param ignoreCase      the character case ignore flag
         *
         * @throws Exception if the regular expression contained
         *             invalid syntax
         */
        public GrammaticaRE(string regex, bool ignoreCase)
        {
            regExp = new RegExp(regex, ignoreCase);
        }

        /**
         * Checks if the start of the input stream matches this
         * regular expression.
         *
         * @param buffer         the input buffer to check
         *
         * @return the longest match found, or
         *         zero (0) if no match was found
         *
         * @throws IOException if an I/O error occurred
         */
        public override int Match(ReaderBuffer buffer)
        {
            if (matcher is null)
            {
                matcher = regExp.Matcher(buffer);
            }
            else
            {
                matcher.Reset(buffer);
            }
            return matcher.MatchFromBeginning() ? matcher.Length() : 0;
        }
    }

    /**
     * The .NET system regular expression handler.
     */
    internal sealed class SystemRE : REHandler
    {
        /**
         * The parsed regular expression.
         */
        private readonly Regex reg;

        /**
         * Creates a new .NET system regular expression handler.
         *
         * @param regex          the regular expression text
         * @param ignoreCase      the character case ignore flag
         *
         * @throws Exception if the regular expression contained
         *             invalid syntax
         */
        public SystemRE(string regex, bool ignoreCase)
        {
            if (ignoreCase)
            {
                reg = new Regex(regex, RegexOptions.IgnoreCase);
            }
            else
            {
                reg = new Regex(regex);
            }
        }

        /**
         * Checks if the start of the input stream matches this
         * regular expression.
         *
         * @param buffer         the input buffer to check
         *
         * @return the longest match found, or
         *         zero (0) if no match was found
         *
         * @throws IOException if an I/O error occurred
         */
        public override int Match(ReaderBuffer buffer)
        {
            Match m;

            // Ugly hack since .NET doesn't have a flag for when the
            // end of the input string was encountered...
            buffer.Peek(1024 * 16);
            // Also, there is no API to limit the search to the specified
            // position, so we double-check the index afterwards instead.
            m = reg.Match(buffer.ToString(), buffer.Position);
            if (m.Success && m.Index == buffer.Position)
            {
                return m.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}

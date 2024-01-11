/*
 * TokenStringDFA.cs
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
using System.Globalization;
using System.Text;

namespace Yale.Parser
{
    /**
     * A deterministic finite state automaton for matching exact strings.
     * It uses a sorted binary tree representation of the state
     * transitions in order to enable quick matches with a minimal memory
     * footprint. It only supports a single character transition between
     * states, but may be run in an all case-insensitive mode.
     *
     * @author   Per Cederberg
     * @version  1.5
     * @since    1.5
     */
    internal sealed class TokenStringDFA
    {
        /**
         * The lookup table for root states, indexed by the first ASCII
         * character. This array is used to for speed optimizing the
         * first step in the match.
         */
        private DFAState[] ascii = new DFAState[128];

        /**
         * The automaton state transition tree for non-ASCII characters.
         * Each transition from one state to another is added to the tree
         * with the corresponding character.
         */
        private DFAState nonAscii = new();

        /**
         * Creates a new empty string automaton.
         */
        public TokenStringDFA() { }

        /**
         * Adds a string match to this automaton. New states and
         * transitions will be added to extend this automaton to
         * support the specified string.
         *
         * @param str              the string to match
         * @param caseInsensitive  the case-insensitive flag
         * @param value            the match value
         */
        public void AddMatch(string str, bool caseInsensitive, TokenPattern value)
        {
            DFAState state;
            DFAState next;
            char c = str[0];
            int start = 0;

            if (caseInsensitive)
            {
                c = char.ToLower(c, culture: CultureInfo.InvariantCulture);
            }
            if (c < 128)
            {
                state = ascii[c];
                state ??= ascii[c] = new DFAState();
                start++;
            }
            else
            {
                state = nonAscii;
            }
            for (int i = start; i < str.Length; i++)
            {
                next = state.tree.Find(str[i], caseInsensitive);
                if (next == null)
                {
                    next = new DFAState();
                    state.tree.Add(str[i], caseInsensitive, next);
                }
                state = next;
            }
            state.value = value;
        }

        /**
         * Checks if the automaton matches an input stream. The
         * matching will be performed from a specified position. This
         * method will not read any characters from the stream, just
         * peek ahead. The comparison can be done either in
         * case-sensitive or case-insensitive mode.
         *
         * @param input            the input stream to check
         * @param pos              the starting position
         * @param caseInsensitive  the case-insensitive flag
         *
         * @return the match value, or
         *         null if no match was found
         *
         * @throws IOException if an I/O error occurred
         */
        public TokenPattern Match(ReaderBuffer buffer, bool caseInsensitive)
        {
            TokenPattern result = null;
            DFAState state;
            int pos = 0;
            int c;

            c = buffer.Peek(0);
            if (c < 0)
            {
                return null;
            }
            if (caseInsensitive)
            {
                c = char.ToLower((char)c);
            }
            if (c < 128)
            {
                state = ascii[c];
                if (state == null)
                {
                    return null;
                }
                else if (state.value != null)
                {
                    result = state.value;
                }
                pos++;
            }
            else
            {
                state = nonAscii;
            }
            while ((c = buffer.Peek(pos)) >= 0)
            {
                state = state.tree.Find((char)c, caseInsensitive);
                if (state == null)
                {
                    break;
                }
                else if (state.value != null)
                {
                    result = state.value;
                }
                pos++;
            }
            return result;
        }

        /**
         * Returns a detailed string representation of this automaton.
         *
         * @return a detailed string representation of this automaton
         */
        public override string ToString()
        {
            StringBuilder buffer = new();

            for (int i = 0; i < ascii.Length; i++)
            {
                if (ascii[i] != null)
                {
                    buffer.Append((char)i);
                    if (ascii[i].value != null)
                    {
                        buffer.Append(": ");
                        buffer.Append(ascii[i].value);
                        buffer.Append('\n');
                    }
                    ascii[i].tree.PrintTo(buffer, " ");
                }
            }
            nonAscii.tree.PrintTo(buffer, "");
            return buffer.ToString();
        }
    }

    /**
     * An automaton state. This class represents a state in the DFA
     * graph.
     *
     * @author   Per Cederberg
     * @version  1.5
     * @since    1.5
     */
    internal sealed class DFAState
    {
        /**
         * The token pattern matched at this state.
         */
        internal TokenPattern? value;

        /**
         * The automaton state transition tree. Each transition from one
         * state to another is added to the tree with the corresponding
         * character.
         */
        internal TransitionTree tree = new();
    }

    /**
     * An automaton state transition tree. This class contains a
     * binary search tree for the automaton transitions from one
     * state to another. All transitions are linked to a single
     * character.
     *
     * @author   Per Cederberg
     * @version  1.5
     * @since    1.5
     */
    internal sealed class TransitionTree
    {
        /**
         * The transition character. If this value is set to the zero
         * character ('\0'), this tree is empty.
         */
        private char _value;

        /**
         * The transition target state.
         */
        private DFAState? _state;

        /**
         * The left subtree.
         */
        private TransitionTree? _left;

        /**
         * The right subtree.
         */
        private TransitionTree? _right;

        /**
         * Creates a new empty automaton transition tree.
         */
        public TransitionTree() { }

        /**
         * Finds an automaton state from the specified transition
         * character. This method searches this transition tree for a
         * matching transition. The comparison can optionally be done
         * with a lower-case conversion of the character.
         *
         * @param c              the character to search for
         * @param lowerCase      the lower-case conversion flag
         *
         * @return the automaton state found, or
         *         null if no transition exists
         */
        public DFAState? Find(char c, bool lowerCase)
        {
            if (lowerCase)
            {
                c = char.ToLower(c);
            }
            if (_value == '\0' || _value == c)
            {
                return _state;
            }
            else if (_value > c)
            {
                return _left?.Find(c, false);
            }
            else
            {
                return _right?.Find(c, false);
            }
        }

        /**
         * Adds a transition to this tree. If the lower-case flag is
         * set, the character will be converted to lower-case before
         * being added.
         *
         * @param c              the character to transition for
         * @param lowerCase      the lower-case conversion flag
         * @param state          the state to transition to
         */
        public void Add(char c, bool lowerCase, DFAState state)
        {
            if (lowerCase)
            {
                c = char.ToLower(c);
            }

            //Initial value \0, initialize _left, _right and _state
            if (_value == '\0')
            {
                _value = c;
                _state = state;
                _left = new TransitionTree();
                _right = new TransitionTree();
            }
            else if (_value > c)
            {
                _left!.Add(c, false, state);
            }
            else
            {
                _right!.Add(c, false, state);
            }
        }

        /**
         * Prints the automaton tree to the specified string buffer.
         *
         * @param buffer         the string buffer
         * @param indent         the current indentation
         */
        public void PrintTo(StringBuilder buffer, string indent)
        {
            _left?.PrintTo(buffer, indent);

            if (_value is not '\0')
            {
                if (buffer.Length > 0 && buffer[^1] == '\n')
                {
                    buffer.Append(indent);
                }
                buffer.Append(_value);
                if (_state!.value is not null) //state is not null when value is not \0
                {
                    buffer.Append(": ");
                    buffer.Append(_state.value);
                    buffer.Append('\n');
                }
                _state.tree.PrintTo(buffer, indent + " ");
            }

            _right?.PrintTo(buffer, indent);
        }
    }
}

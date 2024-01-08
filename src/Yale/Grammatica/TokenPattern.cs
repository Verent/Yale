/*
 * TokenPattern.cs
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
using System.Text;

namespace PerCederberg.Grammatica.Runtime
{
    /**
     * A token pattern. This class contains the definition of a token
     * (i.e. it's pattern), and allows testing a string against this
     * pattern. A token pattern is uniquely identified by an integer id,
     * that must be provided upon creation.
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    internal sealed class TokenPattern
    {
        /**
         * The pattern type enumeration.
         */
        public enum PatternType
        {
            /**
             * The string pattern type is used for tokens that only
             * match an exact string.
             */
            STRING,

            /**
             * The regular expression pattern type is used for tokens
             * that match a regular expression.
             */
            REGEXP
        }

        /**
         * The token error flag. If this flag is set, it means that an
         * error should be reported if the token is found. The error
         * message is present in the errorMessage variable.
         *
         * @see #errorMessage
         */
        private bool error;

        /**
         * The token error message. This message will only be set if the
         * token error flag is set.
         *
         * @see #error
         */
        private string? errorMessage;

        /**
         * The token ignore message. If this message is set when the token
         * ignore flag is also set, a warning message will be printed if
         * the token is found.
         *
         * @see #ignore
         */
        private string? ignoreMessage;

        /**
         * The optional debug information message. This is normally set
         * when the token pattern is analyzed by the tokenizer.
         */
        private string? debugInfo;

        /**
         * Creates a new token pattern.
         *
         * @param id             the token pattern id
         * @param name           the token pattern name
         * @param type           the token pattern type
         * @param pattern        the token pattern
         */
        public TokenPattern(int id, string name, PatternType type, string pattern)
        {
            Id = id;
            Name = name;
            Type = type;
            Pattern = pattern;
        }

        /**
         * The token pattern identity property (read-only). This
         * property contains the unique token pattern identity value.
         *
         * @since 1.5
         */
        public int Id { get; }

        /**
         * The token pattern name property (read-only).
         *
         * @since 1.5
         */
        public string Name { get; }

        /**
         * The token pattern type property (read-only).
         *
         * @since 1.5
         */
        public PatternType Type { get; }

        /**
         * The token pattern property (read-only). This property
         * contains the actual pattern (string or regexp) which have
         * to be matched.
         *
         * @since 1.5
         */
        public string Pattern { get; }

        /**
         * The error flag property. If this property is true, the
         * token pattern corresponds to an error token and an error
         * should be reported if a match is found. When setting this
         * property to true, a default error message is created if
         * none was previously set.
         *
         * @since 1.5
         */
        public bool Error
        {
            get { return error; }
            set
            {
                error = value;
                if (error && errorMessage is null)
                {
                    errorMessage = "unrecognized token found";
                }
            }
        }

        /**
         * The token error message property. The error message is
         * printed whenever the token is matched. Setting the error
         * message property also sets the error flag to true.
         *
         * @see #Error
         *
         * @since 1.5
         */
        public string? ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                error = true;
                errorMessage = value;
            }
        }

        /**
         * The ignore flag property. If this property is true, the
         * token pattern corresponds to an ignore token and should be
         * skipped if a match is found.
         *
         * @since 1.5
         */
        public bool Ignore { get; set; }

        /**
         * The token ignore message property. The ignore message is
         * printed whenever the token is matched. Setting the ignore
         * message property also sets the ignore flag to true.
         *
         * @see #Ignore
         *
         * @since 1.5
         */
        public string? IgnoreMessage
        {
            get { return ignoreMessage; }
            set
            {
                Ignore = true;
                ignoreMessage = value;
            }
        }

        /**
         * The token debug info message property. This is normally be
         * set when the token pattern is analyzed by the tokenizer.
         *
         * @since 1.5
         */
        public string? DebugInfo
        {
            get { return debugInfo; }
            set { debugInfo = value; }
        }

        /**
         * Returns a string representation of this object.
         *
         * @return a token pattern string representation
         */
        public override string ToString()
        {
            StringBuilder buffer = new();

            buffer.Append(Name);
            buffer.Append(" (");
            buffer.Append(Id);
            buffer.Append("): ");
            switch (Type)
            {
                case PatternType.STRING:
                    buffer.Append('"');
                    buffer.Append(Pattern);
                    buffer.Append('"');
                    break;
                case PatternType.REGEXP:
                    buffer.Append("<<");
                    buffer.Append(Pattern);
                    buffer.Append(">>");
                    break;
            }
            if (Error)
            {
                buffer.Append(" ERROR: \"");
                buffer.Append(errorMessage);
                buffer.Append('"');
            }
            if (Ignore)
            {
                buffer.Append(" IGNORE");
                if (ignoreMessage != null)
                {
                    buffer.Append(": \"");
                    buffer.Append(ignoreMessage);
                    buffer.Append('"');
                }
            }
            if (DebugInfo is not null)
            {
                buffer.Append("\n  ");
                buffer.Append(DebugInfo);
            }
            return buffer.ToString();
        }

        /**
         * Returns a short string representation of this object.
         *
         * @return a short string representation of this object
         */
        public string ToShortString()
        {
            StringBuilder buffer = new();
            int newline = Pattern.IndexOf('\n');

            if (Type == PatternType.STRING)
            {
                buffer.Append('"');
                if (newline >= 0)
                {
                    if (newline > 0 && Pattern[newline - 1] == '\r')
                    {
                        newline--;
                    }
                    buffer.Append(Pattern.AsSpan(0, newline));
                    buffer.Append("(...)");
                }
                else
                {
                    buffer.Append(Pattern);
                }
                buffer.Append('"');
            }
            else
            {
                buffer.Append('<');
                buffer.Append(Name);
                buffer.Append('>');
            }

            return buffer.ToString();
        }
    }
}

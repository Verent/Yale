/*
 * ParserCreationException.cs
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
using System.Collections;
using System.Text;

namespace PerCederberg.Grammatica.Runtime
{
    /**
     * A parser creation exception. This exception is used for signalling
     * an error in the token or production patterns, making it impossible
     * to create a working parser or tokenizer.
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    public class ParserCreationException : Exception
    {
        /**
         * The error type enumeration.
         */
        public enum ErrorType
        {
            /**
             * The internal error type is only used to signal an
             * error that is a result of a bug in the parser or
             * tokenizer code.
             */
            Internal,

            /**
             * The invalid parser error type is used when the parser
             * as such is invalid. This error is typically caused by
             * using a parser without any patterns.
             */
            InvalidParser,

            /**
             * The invalid token error type is used when a token
             * pattern is erroneous. This error is typically caused
             * by an invalid pattern type or an erroneous regular
             * expression.
             */
            InvalidToken,

            /**
             * The invalid production error type is used when a
             * production pattern is erroneous. This error is
             * typically caused by referencing undeclared productions,
             * or violating some other production pattern constraint.
             */
            InvalidProduction,

            /**
             * The infinite loop error type is used when an infinite
             * loop has been detected in the grammar. One of the
             * productions in the loop will be reported.
             */
            InfiniteLoop,

            /**
             * The inherent ambiguity error type is used when the set
             * of production patterns (i.e. the grammar) contains
             * ambiguities that cannot be resolved.
             */
            InherentAmbiguity
        }

        /**
         * The error details list. This variable is only set for some
         * error types.
         */
        private readonly ArrayList? details;

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param info           the additional error information
         */
        public ParserCreationException(ErrorType type, string info)
            : this(type: type, name: null, info: info) { }

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param name           the token or production pattern name
         * @param info           the additional error information
         */
        public ParserCreationException(ErrorType type, string? name, string info)
            : this(type: type, name: name, info: info, details: null) { }

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param name           the token or production pattern name
         * @param info           the additional error information
         * @param details        the error details list
         */
        public ParserCreationException(
            ErrorType type,
            string? name,
            string info,
            ArrayList? details
        )
        {
            Type = type;
            Name = name;
            Info = info;
            this.details = details;
        }

        /**
         * The error type property (read-only).
         *
         * @since 1.5
         */
        public ErrorType Type { get; }

        /**
         * The token or production name property (read-only).
         *
         * @since 1.5
         */
        public string? Name { get; }

        /**
         * The additional error information property (read-only).
         *
         * @since 1.5
         */
        public string Info { get; }

        /**
         * The detailed error information property (read-only).
         *
         * @since 1.5
         */
        public string? Details
        {
            get
            {
                StringBuilder buffer = new();

                if (details is null)
                {
                    return null;
                }
                for (var i = 0; i < details.Count; i++)
                {
                    if (i > 0)
                    {
                        buffer.Append(", ");
                        if (i + 1 == details.Count)
                        {
                            buffer.Append("and ");
                        }
                    }
                    buffer.Append(details[i]);
                }

                return buffer.ToString();
            }
        }

        /**
         * The message property (read-only). This property contains
         * the detailed exception error message.
         */
        public override string Message
        {
            get
            {
                StringBuilder buffer = new();

                switch (Type)
                {
                    case ErrorType.InvalidParser:
                        buffer.Append("parser is invalid, as ");
                        buffer.Append(Info);
                        break;
                    case ErrorType.InvalidToken:
                        buffer.Append("token '");
                        buffer.Append(Name);
                        buffer.Append("' is invalid, as ");
                        buffer.Append(Info);
                        break;
                    case ErrorType.InvalidProduction:
                        buffer.Append("production '");
                        buffer.Append(Name);
                        buffer.Append("' is invalid, as ");
                        buffer.Append(Info);
                        break;
                    case ErrorType.InfiniteLoop:
                        buffer.Append("infinite loop found in production pattern '");
                        buffer.Append(Name);
                        buffer.Append('\'');
                        break;
                    case ErrorType.InherentAmbiguity:
                        buffer.Append("inherent ambiguity in production '");
                        buffer.Append(Name);
                        buffer.Append('\'');
                        if (Info is not null)
                        {
                            buffer.Append(' ');
                            buffer.Append(Info);
                        }
                        if (details is not null)
                        {
                            buffer.Append(" starting with ");
                            if (details.Count > 1)
                            {
                                buffer.Append("tokens ");
                            }
                            else
                            {
                                buffer.Append("token ");
                            }
                            buffer.Append(Details);
                        }
                        break;
                    default:
                        buffer.Append("internal error");
                        break;
                }
                return buffer.ToString();
            }
        }
    }
}

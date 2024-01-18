namespace Yale.Parser
{
    /**
     * A token node. This class represents a token (i.e. a set of adjacent
     * characters) in a parse tree. The tokens are created by a tokenizer,
     * that groups characters together into tokens according to a set of
     * token patterns.
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    internal sealed class Token : Node
    {
        private readonly TokenPattern pattern;

        private Token? previous;

        private Token? next;

        /**
         * Creates a new token.
         *
         * @param pattern        the token pattern
         * @param image          the token image (i.e. characters)
         * @param line           the line number of the first character
         * @param col            the column number of the first character
         */
        public Token(TokenPattern pattern, string image, int line, int col)
        {
            this.pattern = pattern;
            Image = image;
            StartLine = line;
            StartColumn = col;
            EndLine = line;
            EndColumn = col + image.Length - 1;
            for (var pos = 0; image.IndexOf('\n', pos) >= 0; )
            {
                pos = image.IndexOf('\n', pos) + 1;
                EndLine++;
                EndColumn = image.Length - pos;
            }
        }

        /**
         * The node type id property (read-only). This value is set as
         * a unique identifier for each type of node, in order to
         * simplify later identification.
         *
         * @since 1.5
         */
        public override TokenId TypeId
        {
            get { return pattern.Id; }
        }

        /**
         * The node name property (read-only).
         *
         * @since 1.5
         */
        public override string Name
        {
            get { return pattern.Name; }
        }

        /**
         * The line number property of the first character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the first child.
         *
         * @since 1.5
         */
        public override int StartLine { get; }

        /**
         * The column number property of the first character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the first child.
         *
         * @since 1.5
         */
        public override int StartColumn { get; }

        /**
         * The line number property of the last character in this node
         * (read-only). If the node has child elements, this value
         * will be fetched from the last child.
         *
         * @since 1.5
         */
        public override int EndLine { get; }

        /**
         * The column number property of the last character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the last child.
         *
         * @since 1.5
         */
        public override int EndColumn { get; }

        /**
         * The token image property (read-only). The token image
         * consists of the input characters matched to form this
         * token.
         *
         * @since 1.5
         */
        public string Image { get; }

        /**
         * The token pattern property (read-only).
         */
        internal TokenPattern Pattern
        {
            get { return pattern; }
        }

        /**
         * The previous token property. If the token list feature is
         * used in the tokenizer, all tokens found will be chained
         * together in a double-linked list. The previous token may be
         * a token that was ignored during the parsing, due to it's
         * ignore flag being set. If there is no previous token or if
         * the token list feature wasn't used in the tokenizer (the
         * default), the previous token will always be null.
         *
         * @see #Next
         * @see Tokenizer#UseTokenList
         *
         * @since 1.5
         */
        public Token? Previous
        {
            get { return previous; }
            set
            {
                if (previous is not null)
                {
                    previous.next = null;
                }
                previous = value;
                if (previous is not null)
                {
                    previous.next = this;
                }
            }
        }

        /**
         * The next token property. If the token list feature is used
         * in the tokenizer, all tokens found will be chained together
         * in a double-linked list. The next token may be a token that
         * was ignored during the parsing, due to it's ignore flag
         * being set. If there is no next token or if the token list
         * feature wasn't used in the tokenizer (the default), the
         * next token will always be null.
         *
         * @see #Previous
         * @see Tokenizer#UseTokenList
         *
         * @since 1.5
         */
        public Token? Next
        {
            get { return next; }
            set
            {
                if (next is not null)
                {
                    next.previous = null;
                }
                next = value;
                if (next is not null)
                {
                    next.previous = this;
                }
            }
        }

        /**
         * @return a string representation of this token
         */
        public override string ToString()
        {
            StringBuilder buffer = new();
            int newline = Image.IndexOf('\n');

            buffer.Append(pattern.Name);
            buffer.Append('(');
            buffer.Append(pattern.Id);
            buffer.Append("): \"");
            if (newline >= 0)
            {
                if (newline > 0 && Image[newline - 1] == '\r')
                {
                    newline--;
                }
                buffer.Append(Image.AsSpan(0, newline));
                buffer.Append("(...)");
            }
            else
            {
                buffer.Append(Image);
            }
            buffer.Append("\", line: ");
            buffer.Append(StartLine);
            buffer.Append(", col: ");
            buffer.Append(StartColumn);

            return buffer.ToString();
        }

        /**
         * Returns a short string representation of this token. The
         * string will only contain the token image and possibly the
         * token pattern name.
         *
         * @return a short string representation of this token
         */
        public string ToShortString()
        {
            StringBuilder buffer = new();
            int newline = Image.IndexOf('\n');

            buffer.Append('"');
            if (newline >= 0)
            {
                if (newline > 0 && Image[newline - 1] == '\r')
                {
                    newline--;
                }
                buffer.Append(Image.AsSpan(0, newline));
                buffer.Append("(...)");
            }
            else
            {
                buffer.Append(Image);
            }
            buffer.Append('"');
            if (pattern.Type == TokenPattern.PatternType.REGEXP)
            {
                buffer.Append(" <");
                buffer.Append(pattern.Name);
                buffer.Append('>');
            }

            return buffer.ToString();
        }
    }
}

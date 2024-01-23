namespace Yale.Parser
{
    /**
     * The token match status. This class contains logic to ensure that
     * only the longest match is considered. It also prefers lower token
     * pattern identifiers if two matches have the same length.
     *

     * @version  1.5
     * @since    1.5
     */
    internal sealed class TokenMatch
    {
        /**
         * The length of the longest match.
         */
        private int length;

        /**
         * The pattern in the longest match.
         */
        private TokenPattern? pattern;

        /**
         * Clears the current match information.
         */
        public void Clear()
        {
            length = 0;
            pattern = null;
        }

        /**
         * The length of the longest match found (read-only).
         */
        public int Length
        {
            get { return length; }
        }

        /**
         * The token pattern for the longest match found (read-only).
         */
        public TokenPattern Pattern
        {
            get { return pattern; }
        }

        /**
         * Updates this match with new values. The new values will only
         * be considered if the length is longer than any previous match
         * found.
         *
         * @param length         the matched length
         * @param pattern        the matched pattern
         */
        public void Update(int length, TokenPattern pattern)
        {
            if (this.length < length)
            {
                this.length = length;
                this.pattern = pattern;
            }
            else if (this.length == length && this.pattern.Id > pattern.Id)
            {
                this.length = length;
                this.pattern = pattern;
            }
        }
    }
}

using System.IO;

namespace Yale.Parser.RE;

/**
 * A regular expression character set element. This element
 * matches a single character inside (or outside) a character set.
 * The character set is user defined and may contain ranges of
 * characters. The set may also be inverted, meaning that only
 * characters not inside the set will be considered to match.
 *

 * @version  1.5
 */
internal sealed class CharacterSetElement : Element
{
    /**
     * The dot ('.') character set. This element matches a single
     * character that is not equal to a newline character.
     */
    public static CharacterSetElement DOT = new(false);

    /**
     * The digit character set. This element matches a single
     * numeric character.
     */
    public static CharacterSetElement DIGIT = new(false);

    /**
     * The non-digit character set. This element matches a single
     * non-numeric character.
     */
    public static CharacterSetElement NON_DIGIT = new(true);

    /**
     * The whitespace character set. This element matches a single
     * whitespace character.
     */
    public static CharacterSetElement WHITESPACE = new(false);

    /**
     * The non-whitespace character set. This element matches a
     * single non-whitespace character.
     */
    public static CharacterSetElement NON_WHITESPACE = new(true);

    /**
     * The word character set. This element matches a single word
     * character.
     */
    public static CharacterSetElement WORD = new(false);

    /**
     * The non-word character set. This element matches a single
     * non-word character.
     */
    public static CharacterSetElement NON_WORD = new(true);

    /**
     * The inverted character set flag.
     */
    private readonly bool inverted;

    /**
     * The character set content. This array may contain either
     * range objects or Character objects.
     */
    private readonly ArrayList contents = new();

    /**
     * Creates a new character set element. If the inverted character
     * set flag is set, only characters NOT in the set will match.
     *
     * @param inverted       the inverted character set flag
     */
    public CharacterSetElement(bool inverted) => this.inverted = inverted;

    /**
     * Adds a single character to this character set.
     *
     * @param c              the character to add
     */
    public void AddCharacter(char c) => contents.Add(c);

    /**
     * Adds multiple characters to this character set.
     *
     * @param str            the string with characters to add
     */
    public void AddCharacters(string str)
    {
        for (var i = 0; i < str.Length; i++)
        {
            AddCharacter(str[i]);
        }
    }

    /**
     * Adds multiple characters to this character set.
     *
     * @param elem           the string element with characters to add
     */
    public void AddCharacters(StringElement elem) => AddCharacters(elem.GetString());

    /**
     * Adds a character range to this character set.
     *
     * @param min            the minimum character value
     * @param max            the maximum character value
     */
    public void AddRange(char min, char max) => contents.Add(new Range(min, max));

    /**
     * Adds a character subset to this character set.
     *
     * @param elem           the character set to add
     */
    public void AddCharacterSet(CharacterSetElement elem) => contents.Add(elem);

    /**
     * Returns this element as the character set shouldn't be
     * modified after creation. This partially breaks the contract
     * of clone(), but as new characters are not added to the
     * character set after creation, this will work correctly.
     *
     * @return this character set element
     */
    public override object Clone() => this;

    /**
     * Returns the length of a matching string starting at the
     * specified position. The number of matches to skip can also be
     * specified, but numbers higher than zero (0) cause a failed
     * match for any element that doesn't attempt to combine other
     * elements.
     *
     * @param m              the matcher being used
     * @param buffer         the input character buffer to match
     * @param start          the starting position
     * @param skip           the number of matches to skip
     *
     * @return the length of the matching string, or
     *         -1 if no match was found
     *
     * @throws IOException if an I/O error occurred
     */
    public override int Match(Matcher m, ReaderBuffer buffer, int start, int skip)
    {
        int c;

        if (skip != 0)
        {
            return -1;
        }
        c = buffer.Peek(start);
        if (c < 0)
        {
            m.SetReadEndOfString();
            return -1;
        }
        if (m.IsCaseInsensitive())
        {
            c = char.ToLower((char)c, CultureInfo.InvariantCulture);
        }
        return InSet((char)c) ? 1 : -1;
    }

    /**
     * Checks if the specified character matches this character
     * set. This method takes the inverted flag into account.
     *
     * @param c               the character to check
     *
     * @return true if the character matches, or
     *         false otherwise
     */
    private bool InSet(char c)
    {
        if (this == DOT)
        {
            return InDotSet(c);
        }
        else if (this == DIGIT || this == NON_DIGIT)
        {
            return InDigitSet(c) != inverted;
        }
        else if (this == WHITESPACE || this == NON_WHITESPACE)
        {
            return InWhitespaceSet(c) != inverted;
        }
        else if (this == WORD || this == NON_WORD)
        {
            return InWordSet(c) != inverted;
        }
        else
        {
            return InUserSet(c) != inverted;
        }
    }

    /**
     * Checks if the specified character is present in the 'dot'
     * set. This method does not consider the inverted flag.
     *
     * @param c               the character to check
     *
     * @return true if the character is present, or
     *         false otherwise
     */
    private static bool InDotSet(char c)
    {
        return c switch
        {
            '\n' or '\r' or '\u0085' or '\u2028' or '\u2029' => false,
            _ => true,
        };
    }

    /**
     * Checks if the specified character is a digit. This method
     * does not consider the inverted flag.
     *
     * @param c               the character to check
     *
     * @return true if the character is a digit, or
     *         false otherwise
     */
    private static bool InDigitSet(char c) => c is >= '0' and <= '9';

    /**
     * Checks if the specified character is a whitespace
     * character. This method does not consider the inverted flag.
     *
     * @param c               the character to check
     *
     * @return true if the character is a whitespace character, or
     *         false otherwise
     */
    private static bool InWhitespaceSet(char c)
    {
        return c switch
        {
            ' ' or '\t' or '\n' or '\f' or '\r' or (char)11 => true,
            _ => false,
        };
    }

    /**
     * Checks if the specified character is a word character. This
     * method does not consider the inverted flag.
     *
     * @param c               the character to check
     *
     * @return true if the character is a word character, or
     *         false otherwise
     */
    private static bool InWordSet(char c) =>
        c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_';

    /**
     * Checks if the specified character is present in the user-
     * defined set. This method does not consider the inverted
     * flag.
     *
     * @param value           the character to check
     *
     * @return true if the character is present, or
     *         false otherwise
     */
    private bool InUserSet(char value)
    {
        object? obj;

        for (var i = 0; i < contents.Count; i++)
        {
            obj = contents[i];
            if (obj is char @char)
            {
                if (@char == value)
                {
                    return true;
                }
            }
            else if (obj is Range range)
            {
                if (range.Inside(value))
                {
                    return true;
                }
            }
            else if (obj is CharacterSetElement element)
            {
                if (element.InSet(value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Prints this element to the specified output stream.
     *
     * @param output         the output stream to use
     * @param indent         the current indentation
     */
    public override void PrintTo(TextWriter output, string indent) =>
        output.WriteLine(indent + ToString());

    /**
     * Returns a string description of this character set.
     *
     * @return a string description of this character set
     */
    public override string ToString()
    {
        StringBuilder buffer;

        // Handle predefined character sets
        if (this == DOT)
        {
            return ".";
        }
        else if (this == DIGIT)
        {
            return "\\d";
        }
        else if (this == NON_DIGIT)
        {
            return "\\D";
        }
        else if (this == WHITESPACE)
        {
            return "\\s";
        }
        else if (this == NON_WHITESPACE)
        {
            return "\\S";
        }
        else if (this == WORD)
        {
            return "\\w";
        }
        else if (this == NON_WORD)
        {
            return "\\W";
        }

        // Handle user-defined character sets
        buffer = new StringBuilder();
        if (inverted)
        {
            buffer.Append("^[");
        }
        else
        {
            buffer.Append('[');
        }
        for (var i = 0; i < contents.Count; i++)
        {
            buffer.Append(contents[i]);
        }
        buffer.Append(']');

        return buffer.ToString();
    }

    /**
     * A character range class.
     */
    private sealed class Range
    {
        /**
         * The minimum character value.
         */
        private readonly char min;

        /**
         * The maximum character value.
         */
        private readonly char max;

        /**
         * Creates a new character range.
         *
         * @param min        the minimum character value
         * @param max        the maximum character value
         */
        public Range(char min, char max)
        {
            this.min = min;
            this.max = max;
        }

        /**
         * Checks if the specified character is inside the range.
         *
         * @param c          the character to check
         *
         * @return true if the character is in the range, or
         *         false otherwise
         */
        public bool Inside(char c) => min <= c && c <= max;

        /**
         * Returns a string representation of this object.
         *
         * @return a string representation of this object
         */
        public override string ToString() => min + "-" + max;
    }
}

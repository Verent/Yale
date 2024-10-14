using System.IO;

namespace Yale.Parser.RE;

/**
 * A regular expression. This class creates and holds an internal
 * data structure representing a regular expression. It also
 * allows creating matchers. This class is thread-safe. Multiple
 * matchers may operate simultanously on the same regular
 * expression.
 *

 * @version  1.5
 */
internal sealed class RegExp
{
    /**
     * The base regular expression element.
     */
    private readonly Element element;

    /**
     * The regular expression pattern.
     */
    private readonly string pattern;

    /**
     * The character case ignore flag.
     */
    private readonly bool ignoreCase;

    /**
     * The current position in the pattern. This variable is used by
     * the parsing methods.
     */
    private int pos;

    /**
     * Creates a new case-sensitive regular expression.
     *
     * @param pattern        the regular expression pattern
     *
     * @throws RegExpException if the regular expression couldn't be
     *             parsed correctly
     */
    public RegExp(string pattern)
        : this(pattern, false) { }

    /**
     * Creates a new regular expression. The regular expression
     * can be either case-sensitive or case-insensitive.
     *
     * @param pattern        the regular expression pattern
     * @param ignoreCase     the character case ignore flag
     *
     * @throws RegExpException if the regular expression couldn't be
     *             parsed correctly
     *
     */
    public RegExp(string pattern, bool ignoreCase)
    {
        this.pattern = pattern;
        this.ignoreCase = ignoreCase;
        pos = 0;
        element = ParseExpr();
        if (pos < pattern.Length)
        {
            throw new RegExpException(RegExpException.ErrorType.UnexpectedCharacter, pos, pattern);
        }
    }

    /**
     * Creates a new matcher for the specified string.
     *
     * @param str            the string to work with
     *
     * @return the regular expresion matcher
     */
    public Matcher Matcher(string str) => Matcher(new ReaderBuffer(new StringReader(str)));

    /**
     * Creates a new matcher for the specified look-ahead
     * character input stream.
     *
     * @param buffer         the character input buffer
     *
     * @return the regular expresion matcher
     *
     */
    public Matcher Matcher(ReaderBuffer buffer) =>
        new((Element)element.Clone(), buffer, ignoreCase);

    /**
     * Returns a string representation of the regular expression.
     *
     * @return a string representation of the regular expression
     */
    public override string ToString()
    {
        StringWriter str;

        str = new StringWriter();
        str.WriteLine("Regular Expression");
        str.WriteLine("  Pattern: " + pattern);
        str.Write("  Flags:");
        if (ignoreCase)
        {
            str.Write(" caseignore");
        }
        str.WriteLine();
        str.WriteLine("  Compiled:");
        element.PrintTo(str, "    ");
        return str.ToString();
    }

    /**
     * Parses a regular expression. This method handles the Expr
     * production in the grammar (see regexp.grammar).
     *
     * @return the element representing this expression
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseExpr()
    {
        Element first;
        Element second;

        first = ParseTerm();
        if (PeekChar(0) != '|')
        {
            return first;
        }
        else
        {
            ReadChar('|');
            second = ParseExpr();
            return new AlternativeElement(first, second);
        }
    }

    /**
     * Parses a regular expression term. This method handles the
     * Term production in the grammar (see regexp.grammar).
     *
     * @return the element representing this term
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseTerm()
    {
        List<Element> list = new() { ParseFact() };
        while (true)
        {
            switch (PeekChar(0))
            {
                case -1:
                case ')':
                case ']':
                case '{':
                case '}':
                case '?':
                case '+':
                case '|':
                    return CombineElements(list);
                default:
                    list.Add(ParseFact());
                    break;
            }
        }
    }

    /**
     * Parses a regular expression factor. This method handles the
     * Fact production in the grammar (see regexp.grammar).
     *
     * @return the element representing this factor
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseFact()
    {
        Element elem;

        elem = ParseAtom();
        switch (PeekChar(0))
        {
            case '?':
            case '*':
            case '+':
            case '{':
                return ParseAtomModifier(elem);
            default:
                return elem;
        }
    }

    /**
     * Parses a regular expression atom. This method handles the
     * Atom production in the grammar (see regexp.grammar).
     *
     * @return the element representing this atom
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseAtom()
    {
        Element elem;

        switch (PeekChar(0))
        {
            case '.':
                ReadChar('.');
                return CharacterSetElement.DOT;
            case '(':
                ReadChar('(');
                elem = ParseExpr();
                ReadChar(')');
                return elem;
            case '[':
                ReadChar('[');
                elem = ParseCharSet();
                ReadChar(']');
                return elem;
            case -1:
            case ')':
            case ']':
            case '{':
            case '}':
            case '?':
            case '*':
            case '+':
            case '|':
                throw new RegExpException(
                    RegExpException.ErrorType.UnexpectedCharacter,
                    pos,
                    pattern
                );
            default:
                return ParseChar();
        }
    }

    /**
     * Parses a regular expression atom modifier. This method handles
     * the AtomModifier production in the grammar (see regexp.grammar).
     *
     * @param elem           the element to modify
     *
     * @return the modified element
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private RepeatElement ParseAtomModifier(Element elem)
    {
        RepeatElement.RepeatType type;
        int firstPos;

        // Read min and max
        type = RepeatElement.RepeatType.GREEDY;
        int min;
        int max;
        switch (ReadChar())
        {
            case '?':
                min = 0;
                max = 1;
                break;
            case '*':
                min = 0;
                max = -1;
                break;
            case '+':
                min = 1;
                max = -1;
                break;
            case '{':
                firstPos = pos - 1;
                min = ReadNumber();
                max = min;
                if (PeekChar(0) == ',')
                {
                    ReadChar(',');
                    max = -1;
                    if (PeekChar(0) != '}')
                    {
                        max = ReadNumber();
                    }
                }
                ReadChar('}');
                if (max == 0 || max > 0 && min > max)
                {
                    throw new RegExpException(
                        RegExpException.ErrorType.InvalidRepeatCount,
                        firstPos,
                        pattern
                    );
                }
                break;
            default:
                throw new RegExpException(
                    RegExpException.ErrorType.UnexpectedCharacter,
                    pos - 1,
                    pattern
                );
        }

        // Read operator mode
        if (PeekChar(0) == '?')
        {
            ReadChar('?');
            type = RepeatElement.RepeatType.RELUCTANT;
        }
        else if (PeekChar(0) == '+')
        {
            ReadChar('+');
            type = RepeatElement.RepeatType.POSSESSIVE;
        }

        return new RepeatElement(elem, min, max, type);
    }

    /**
     * Parses a regular expression character set. This method handles
     * the contents of the '[...]' construct in a regular expression.
     *
     * @return the element representing this character set
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private CharacterSetElement ParseCharSet()
    {
        CharacterSetElement charset;
        Element element;
        var repeat = true;
        char start;
        char end;

        if (PeekChar(0) == '^')
        {
            ReadChar('^');
            charset = new CharacterSetElement(true);
        }
        else
        {
            charset = new CharacterSetElement(false);
        }

        while (PeekChar(0) > 0 && repeat)
        {
            start = (char)PeekChar(0);
            switch (start)
            {
                case ']':
                    repeat = false;
                    break;
                case '\\':
                    element = ParseEscapeChar();
                    if (element is StringElement stringElement)
                    {
                        charset.AddCharacters(stringElement);
                    }
                    else
                    {
                        charset.AddCharacterSet((CharacterSetElement)element);
                    }
                    break;
                default:
                    ReadChar(start);
                    if (PeekChar(0) == '-' && PeekChar(1) > 0 && PeekChar(1) != ']')
                    {
                        ReadChar('-');
                        end = ReadChar();
                        charset.AddRange(FixChar(start), FixChar(end));
                    }
                    else
                    {
                        charset.AddCharacter(FixChar(start));
                    }
                    break;
            }
        }

        return charset;
    }

    /**
     * Parses a regular expression character. This method handles
     * a single normal character in a regular expression.
     *
     * @return the element representing this character
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseChar()
    {
        switch (PeekChar(0))
        {
            case '\\':
                return ParseEscapeChar();
            case '^':
            case '$':
                throw new RegExpException(
                    RegExpException.ErrorType.UnsupportedSpecialCharacter,
                    pos,
                    pattern
                );
            default:
                return new StringElement(FixChar(ReadChar()));
        }
    }

    /**
     * Parses a regular expression character escape. This method
     * handles a single character escape in a regular expression.
     *
     * @return the element representing this character escape
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private Element ParseEscapeChar()
    {
        char c;
        string str;
        int value;

        ReadChar('\\');
        c = ReadChar();
        switch (c)
        {
            case '0':
                c = ReadChar();
                if (c is < '0' or > '3')
                {
                    throw new RegExpException(
                        RegExpException.ErrorType.UnsupportedEscapeCharacter,
                        pos - 3,
                        pattern
                    );
                }
                value = c - '0';
                c = (char)PeekChar(0);
                if (c is >= '0' and <= '7')
                {
                    value *= 8;
                    value += ReadChar() - '0';
                    c = (char)PeekChar(0);
                    if (c is >= '0' and <= '7')
                    {
                        value *= 8;
                        value += ReadChar() - '0';
                    }
                }
                return new StringElement(FixChar((char)value));
            case 'x':
                str = ReadChar().ToString() + ReadChar().ToString();
                try
                {
                    value = int.Parse(
                        str,
                        NumberStyles.AllowHexSpecifier,
                        provider: CultureInfo.InvariantCulture
                    );
                    return new StringElement(FixChar((char)value));
                }
                catch (FormatException)
                {
                    throw new RegExpException(
                        RegExpException.ErrorType.UnsupportedEscapeCharacter,
                        pos - str.Length - 2,
                        pattern
                    );
                }
            case 'u':
                str =
                    ReadChar().ToString()
                    + ReadChar().ToString()
                    + ReadChar().ToString()
                    + ReadChar().ToString();
                try
                {
                    value = int.Parse(
                        str,
                        NumberStyles.AllowHexSpecifier,
                        provider: CultureInfo.InvariantCulture
                    );
                    return new StringElement(FixChar((char)value));
                }
                catch (FormatException)
                {
                    throw new RegExpException(
                        RegExpException.ErrorType.UnsupportedEscapeCharacter,
                        pos - str.Length - 2,
                        pattern
                    );
                }
            case 't':
                return new StringElement('\t');
            case 'n':
                return new StringElement('\n');
            case 'r':
                return new StringElement('\r');
            case 'f':
                return new StringElement('\f');
            case 'a':
                return new StringElement('\u0007');
            case 'e':
                return new StringElement('\u001B');
            case 'd':
                return CharacterSetElement.DIGIT;
            case 'D':
                return CharacterSetElement.NON_DIGIT;
            case 's':
                return CharacterSetElement.WHITESPACE;
            case 'S':
                return CharacterSetElement.NON_WHITESPACE;
            case 'w':
                return CharacterSetElement.WORD;
            case 'W':
                return CharacterSetElement.NON_WORD;
            default:
                if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
                {
                    throw new RegExpException(
                        RegExpException.ErrorType.UnsupportedEscapeCharacter,
                        pos - 2,
                        pattern
                    );
                }
                return new StringElement(FixChar(c));
        }
    }

    /**
     * Adjusts a character for inclusion in a string or character
     * set element. For case-insensitive regular expressions, this
     * transforms the character to lower-case.
     *
     * @param c               the input character
     *
     * @return the adjusted character
     */
    private char FixChar(char c) => ignoreCase ? char.ToLower(c, CultureInfo.InvariantCulture) : c;

    /**
     * Reads a number from the pattern. If the next character isn't a
     * numeric character, an exception is thrown. This method reads
     * several consecutive numeric characters.
     *
     * @return the numeric value read
     *
     * @throws RegExpException if an error was encountered in the
     *             pattern string
     */
    private int ReadNumber()
    {
        StringBuilder buf = new();
        int c;

        c = PeekChar(0);
        while (c is >= '0' and <= '9')
        {
            buf.Append(ReadChar());
            c = PeekChar(0);
        }
        if (buf.Length <= 0)
        {
            throw new RegExpException(RegExpException.ErrorType.UnexpectedCharacter, pos, pattern);
        }
        return int.Parse(buf.ToString(), provider: CultureInfo.InvariantCulture);
    }

    /**
     * Reads the next character in the pattern. If no next character
     * exists, an exception is thrown.
     *
     * @return the character read
     *
     * @throws RegExpException if no next character was available in
     *             the pattern string
     */
    private char ReadChar()
    {
        var c = PeekChar(0);

        if (c < 0)
        {
            throw new RegExpException(RegExpException.ErrorType.UnterminatedPattern, pos, pattern);
        }
        else
        {
            pos++;
            return (char)c;
        }
    }

    /**
     * Reads the next character in the pattern. If the character
     * wasn't the specified one, an exception is thrown.
     *
     * @param c              the character to read
     *
     * @return the character read
     *
     * @throws RegExpException if the character read didn't match the
     *             specified one, or if no next character was
     *             available in the pattern string
     */
    private char ReadChar(char c)
    {
        if (c != ReadChar())
        {
            throw new RegExpException(
                RegExpException.ErrorType.UnexpectedCharacter,
                pos - 1,
                pattern
            );
        }
        return c;
    }

    /**
     * Returns a character that has not yet been read from the
     * pattern. If the requested position is beyond the end of the
     * pattern string, -1 is returned.
     *
     * @param count          the preview position, from zero (0)
     *
     * @return the character found, or
     *         -1 if beyond the end of the pattern string
     */
    private int PeekChar(int count)
    {
        if (pos + count < pattern.Length)
        {
            return pattern[pos + count];
        }

        return -1;
    }

    /**
     * Combines a list of elements. This method takes care to always
     * concatenate adjacent string elements into a single string
     * element.
     *
     * @param list           the list with elements
     *
     * @return the combined element
     */
    private static Element CombineElements(List<Element> list)
    {
        Element currentElement;
        string str;
        int i;

        // Concatenate string elements
        var prevElement = list[0];
        for (i = 1; i < list.Count; i++)
        {
            currentElement = list[i];
            if (
                prevElement is StringElement prevAsStringElement
                && currentElement is StringElement elementAsStringElement
            )
            {
                str = prevAsStringElement.GetString() + elementAsStringElement.GetString();
                currentElement = new StringElement(str);
                list.RemoveAt(i);
                list[i - 1] = currentElement;
                i--;
            }
            prevElement = currentElement;
        }

        // Combine all remaining elements
        currentElement = list[^1];
        for (i = list.Count - 2; i >= 0; i--)
        {
            prevElement = list[i];
            currentElement = new CombineElement(prevElement, currentElement);
        }

        return currentElement;
    }
}

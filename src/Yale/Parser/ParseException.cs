namespace Yale.Parser;

public class ParseException : Exception
{
    /**
     * The error type enumeration.
     */
    public enum ErrorType
    {
        /**
         * The internal error type is only used to signal an error
         * that is a result of a bug in the parser or tokenizer
         * code.
         */
        Internal,

        /**
         * The I/O error type is used for stream I/O errors.
         */
        IO,

        /**
         * The unexpected end of file error type is used when end
         * of file is encountered instead of a valid token.
         */
        UnexpectedEof,

        /**
         * The unexpected character error type is used when a
         * character is read that isn't handled by one of the
         * token patterns.
         */
        UnexpectedChar,

        /**
         * The unexpected token error type is used when another
         * token than the expected one is encountered.
         */
        UnexpectedToken,

        /**
         * The invalid token error type is used when a token
         * pattern with an error message is matched. The
         * additional information provided should contain the
         * error message.
         */
        InvalidToken,

        /**
         * The analysis error type is used when an error is
         * encountered in the analysis. The additional information
         * provided should contain the error message.
         */
        Analysis
    }

    /**
     * Creates a new parse exception.
     *
     * @param type           the parse error type
     * @param info           the additional information
     * @param line           the line number, or -1 for unknown
     * @param column         the column number, or -1 for unknown
     */
    public ParseException(ErrorType type, string? info, int line, int column)
        : this(type, info, details: null, line, column) { }

    /**
     * Creates a new parse exception. This constructor is only
     * used to supply the detailed information array, which is
     * only used for expected token errors. The list then contains
     * descriptions of the expected tokens.
     *
     * @param type           the parse error type
     * @param info           the additional information
     * @param details        the additional detailed information
     * @param line           the line number, or -1 for unknown
     * @param column         the column number, or -1 for unknown
     */
    public ParseException(ErrorType type, string? info, List<string>? details, int line, int column)
    {
        Type = type;
        Info = info;
        Details = details;
        Line = line;
        Column = column;
    }

    /**
     * The error type property (read-only).
     *
     * @since 1.5
     */
    public ErrorType Type { get; }

    /**
     * The additional error information property (read-only).
     *
     * @since 1.5
     */
    public string? Info { get; }

    /**
     * The additional detailed error information property
     * (read-only).
     *
     * @since 1.5
     */
    public List<string>? Details { get; }

    /**
     * The line number property (read-only). This is the line
     * number where the error occurred, or -1 if unknown.
     *
     * @since 1.5
     */
    public int Line { get; }

    /**
     * The column number property (read-only). This is the column
     * number where the error occurred, or -1 if unknown.
     *
     * @since 1.5
     */
    public int Column { get; }

    /**
     * The message property (read-only). This property contains
     * the detailed exception error message, including line and
     * column numbers when available.
     *
     * @see #ErrorMessage
     */
    public override string Message
    {
        get
        {
            StringBuilder buffer = new();

            // Add error description
            buffer.Append(ErrorMessage);

            // Add line and column
            if (Line > 0 && Column > 0)
            {
                buffer.Append(", on line: ");
                buffer.Append(Line);
                buffer.Append(" column: ");
                buffer.Append(Column);
            }

            return buffer.ToString();
        }
    }

    /**
     * The error message property (read-only). This property
     * contains all the information available, except for the line
     * and column number information.
     *
     * @see #Message
     *
     * @since 1.5
     */
    public string ErrorMessage
    {
        get
        {
            StringBuilder buffer = new();

            // Add type and info
            switch (Type)
            {
                case ErrorType.IO:
                    buffer.Append("I/O error: ");
                    buffer.Append(Info);
                    break;
                case ErrorType.UnexpectedEof:
                    buffer.Append("unexpected end of file");
                    break;
                case ErrorType.UnexpectedChar:
                    buffer.Append("unexpected character '");
                    buffer.Append(Info);
                    buffer.Append('\'');
                    break;
                case ErrorType.UnexpectedToken:
                    buffer.Append("unexpected token ");
                    buffer.Append(Info);
                    if (Details is not null)
                    {
                        buffer.Append(", expected ");
                        if (Details.Count > 1)
                        {
                            buffer.Append("one of ");
                        }
                        buffer.Append(GetMessageDetails(Details));
                    }
                    break;
                case ErrorType.InvalidToken:
                    buffer.Append(Info);
                    break;
                case ErrorType.Analysis:
                    buffer.Append(Info);
                    break;
                default:
                    buffer.Append("internal error");
                    if (Info is not null)
                    {
                        buffer.Append(": ");
                        buffer.Append(Info);
                    }
                    break;
            }

            return buffer.ToString();
        }
    }

    /**
     * Returns a string containing all the detailed information in
     * a list. The elements are separated with a comma.
     *
     * @return the detailed information string
     */
    private static string GetMessageDetails(List<string> details)
    {
        StringBuilder buffer = new();

        for (var i = 0; i < details.Count; i++)
        {
            if (i > 0)
            {
                buffer.Append(", ");
                if (i + 1 == details.Count)
                {
                    buffer.Append("or ");
                }
            }
            buffer.Append(details[i]);
        }

        return buffer.ToString();
    }
}

namespace Yale.Parser;

/**
 * A parser log exception. This class contains a list of all the
 * parse errors encountered while parsing.
 */
public class ParserLogException : Exception
{
    /**
     * The list of errors found.
     */
    private readonly ArrayList errors = new();

    /**
     * Creates a new empty parser log exception.
     */
    public ParserLogException() { }

    /**
     * The message property (read-only). This property contains
     * the detailed exception error message.
     */
    public override string Message
    {
        get
        {
            StringBuilder buffer = new();

            for (var i = 0; i < Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append('\n');
                }
                buffer.Append(this[i].Message);
            }
            return buffer.ToString();
        }
    }

    /**
     * The error count property (read-only).
     *
     * @since 1.5
     */
    public int Count
    {
        get { return errors.Count; }
    }

    /**
     * The error index (read-only). This index contains all the
     * errors in this error log.
     *
     * @param index          the error index, 0 <= index < Count
     *
     * @return the parse error requested
     *
     * @since 1.5
     */
    public ParseException this[int index]
    {
        get { return (ParseException)errors[index]; }
    }

    /**
     * Returns a specific error from the log.
     *
     * @param index          the error index, 0 <= index < count
     *
     * @return the parse error requested
     *
     * @deprecated Use the class indexer instead.
     */
    public ParseException GetError(int index) => this[index];

    /**
     * Adds a parse error to the log.
     *
     * @param e              the parse error to add
     */
    public void AddError(ParseException e) => errors.Add(e);
}

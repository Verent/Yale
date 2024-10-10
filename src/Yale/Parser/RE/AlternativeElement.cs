using System.IO;

namespace Yale.Parser.RE;

/**
 * A regular expression alternative element. This element matches
 * the longest alternative element.
 *

 * @version  1.5
 */
internal sealed class AlternativeElement : Element
{
    /**
     * The first alternative element.
     */
    private readonly Element elem1;

    /**
     * The second alternative element.
     */
    private readonly Element elem2;

    /**
     * Creates a new alternative element.
     *
     * @param first          the first alternative
     * @param second         the second alternative
     */
    public AlternativeElement(Element first, Element second)
    {
        elem1 = first;
        elem2 = second;
    }

    /**
     * Creates a copy of this element. The copy will be an
     * instance of the same class matching the same strings.
     * Copies of elements are necessary to allow elements to cache
     * intermediate results while matching strings without
     * interfering with other threads.
     *
     * @return a copy of this element
     */
    public override object Clone() => new AlternativeElement(elem1, elem2);

    /**
     * Returns the length of a matching string starting at the
     * specified position. The number of matches to skip can also
     * be specified, but numbers higher than zero (0) cause a
     * failed match for any element that doesn't attempt to
     * combine other elements.
     *
     * @param m              the matcher being used
     * @param buffer         the input character buffer to match
     * @param start          the starting position
     * @param skip           the number of matches to skip
     *
     * @return the length of the longest matching string, or
     *         -1 if no match was found
     *
     * @throws IOException if an I/O error occurred
     */
    public override int Match(Matcher m, ReaderBuffer buffer, int start, int skip)
    {
        var length = 0;
        var length1 = -1;
        var length2 = -1;
        var skip1 = 0;
        var skip2 = 0;

        while (length >= 0 && skip1 + skip2 <= skip)
        {
            length1 = elem1.Match(m, buffer, start, skip1);
            length2 = elem2.Match(m, buffer, start, skip2);
            if (length1 >= length2)
            {
                length = length1;
                skip1++;
            }
            else
            {
                length = length2;
                skip2++;
            }
        }
        return length;
    }

    /**
     * Prints this element to the specified output stream.
     *
     * @param output         the output stream to use
     * @param indent         the current indentation
     */
    public override void PrintTo(TextWriter output, string indent)
    {
        output.WriteLine(indent + "Alternative 1");
        elem1.PrintTo(output, indent + "  ");
        output.WriteLine(indent + "Alternative 2");
        elem2.PrintTo(output, indent + "  ");
    }
}

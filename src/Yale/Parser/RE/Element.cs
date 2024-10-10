using System.IO;

namespace Yale.Parser.RE;

/**
 * A regular expression element. This is the common base class for
 * all regular expression elements, i.e. the parts of the regular
 * expression.
 *

 * @version  1.5
 */
internal abstract class Element : ICloneable
{
    /**
     * Creates a copy of this element. The copy will be an
     * instance of the same class matching the same strings.
     * Copies of elements are necessary to allow elements to cache
     * intermediate results while matching strings without
     * interfering with other threads.
     *
     * @return a copy of this element
     */
    public abstract object Clone();

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
     * @return the length of the matching string, or
     *         -1 if no match was found
     *
     * @throws IOException if an I/O error occurred
     */
    public abstract int Match(Matcher m, ReaderBuffer buffer, int start, int skip);

    /**
     * Prints this element to the specified output stream.
     *
     * @param output         the output stream to use
     * @param indent         the current indentation
     */
    public abstract void PrintTo(TextWriter output, string indent);
}

using System.IO;

namespace Yale.Parser;

/**
 * An abstract parse tree node. This class is inherited by all
 * nodes in the parse tree, i.e. by the token and production
 * classes.
 */
internal abstract class Node
{
    /**
     * Checks if this node is hidden, i.e. if it should not be
     * visible outside the parser.
     */
    internal virtual bool IsHidden() => false;

    /**
     * The node type id property (read-only). This value is set as
     * a unique identifier for each type of node, in order to
     * simplify later identification.
     */
    public abstract TokenId TypeId { get; }

    /**
     * The node name property (read-only).
     */
    public abstract string Name { get; }

    /**
     * The line number property of the first character in this
     * node (read-only). If the node has child elements, this
     * value will be fetched from the first child.
     */
    public abstract int StartLine { get; }

    /**
     * The column number property of the first character in this
     * node (read-only). If the node has child elements, this
     * value will be fetched from the first child.
     */
    public abstract int StartColumn { get; }

    /**
     * The line number property of the last character in this node
     * (read-only). If the node has child elements, this value
     * will be fetched from the last child.
     */
    public abstract int EndLine { get; }

    /**
     * The column number property of the last character in this
     * node (read-only). If the node has child elements, this
     * value will be fetched from the last child.
     */
    public abstract int EndColumn { get; }

    public Node? Parent { get; set; }

    public virtual int Count => 0;

    /**
    * Returns the number of descendant nodes.
    */
    public int GetDescendantCount()
    {
        var count = 0;

        for (var i = 0; i < Count; i++)
        {
            count += 1 + this[i].GetDescendantCount();
        }
        return count;
    }

    ///**
    // * The child node index (read-only).
    // *
    // * @param index          the child index, 0 <= index < Count
    // *
    // * @return the child node found, or null if index out of bounds
    // *
    // */
    public virtual Node this[int index] => throw new NotImplementedException();

    ///**
    // * The node values property. This property provides direct
    // * access to the list of computed values associated with this
    // * node during analysis. Note that setting this property to
    // * null will remove all node values. Any operation on the
    // * value array list is allowed and is immediately reflected
    // * through the various value reading and manipulation methods.
    // *
    // * @since 1.5
    // */

    private List<object>? values;
    public List<object> Values
    {
        get
        {
            values ??= new List<object>();
            return values;
        }
        set => values = value;
    }

    /**
     * Prints this node and all subnodes to the specified output
     * stream.
     *
     * @param output         the output stream to use
     */
    public void PrintTo(TextWriter output)
    {
        PrintTo(output, "");
        output.Flush();
    }

    /**
     * Prints this node and all subnodes to the specified output
     * stream.
     *
     * @param output         the output stream to use
     * @param indent         the indentation string
     */
    private void PrintTo(TextWriter output, string indent)
    {
        output.WriteLine(indent + ToString());
        indent += "  ";
        for (var i = 0; i < Count; i++)
        {
            this[i]?.PrintTo(output, indent);
        }
    }
}

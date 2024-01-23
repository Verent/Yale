namespace Yale.Parser;

internal sealed class Production : Node
{
    private readonly List<Node> children = new();

    public Production(ProductionPattern pattern)
    {
        Pattern = pattern;
    }

    public override TokenId TypeId
    {
        get { return Pattern.Id; }
    }

    public override string Name
    {
        get { return Pattern.Name; }
    }

    public override int Count
    {
        get { return children.Count; }
    }

    public void AddChild(Node child)
    {
        child.Parent = this;
        children.Add(child);
    }

    public ProductionPattern Pattern { get; }

    public override int StartLine
    {
        get
        {
            for (var i = 0; i < Count; i++)
            {
                var line = this[i].StartLine;
                if (line >= 0)
                {
                    return line;
                }
            }
            return -1;
        }
    }

    public override int StartColumn
    {
        get
        {
            for (var i = 0; i < Count; i++)
            {
                var col = this[i].StartColumn;
                if (col >= 0)
                {
                    return col;
                }
            }
            return -1;
        }
    }

    public override int EndLine
    {
        get
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var line = this[i].EndLine;
                if (line >= 0)
                {
                    return line;
                }
            }
            return -1;
        }
    }

    public override int EndColumn
    {
        get
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var col = this[i].EndColumn;
                if (col >= 0)
                {
                    return col;
                }
            }
            return -1;
        }
    }

    public override Node this[int index]
    {
        get { return children[index]; }
    }

    /**
     * Checks if this node is hidden, i.e. if it should not be visible
     * outside the parser.
     *
     * @return true if the node should be hidden, or
     *         false otherwise
     */
    internal override bool IsHidden()
    {
        return Pattern.Synthetic;
    }

    /**
     * Returns a string representation of this production.
     */
    public override string ToString()
    {
        return Pattern.Name + '(' + Pattern.Id + ')';
    }
}

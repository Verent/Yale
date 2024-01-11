/*
 * Node.cs
 *
 * This program is free software: you can redistribute it and/or
 * modify it under the terms of the BSD license.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * LICENSE.txt file for more details.
 *
 * Copyright (c) 2003-2015 Per Cederberg. All rights reserved.
 */

using System.IO;

namespace Yale.Parser
{
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
        internal virtual bool IsHidden()
        {
            return false;
        }

        /**
         * The node type id property (read-only). This value is set as
         * a unique identifier for each type of node, in order to
         * simplify later identification.
         *
         */
        public abstract TokenId TypeId { get; }

        /**
         * The node name property (read-only).
         *
         * @since 1.5
         */
        public abstract string Name { get; }

        /**
         * The line number property of the first character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the first child.
         *
         * @since 1.5
         */
        public abstract int StartLine { get; }

        /**
         * The column number property of the first character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the first child.
         *
         * @since 1.5
         */
        public abstract int StartColumn { get; }

        /**
         * The line number property of the last character in this node
         * (read-only). If the node has child elements, this value
         * will be fetched from the last child.
         *
         * @since 1.5
         */
        public abstract int EndLine { get; }

        /**
         * The column number property of the last character in this
         * node (read-only). If the node has child elements, this
         * value will be fetched from the last child.
         *
         * @since 1.5
         */
        public abstract int EndColumn { get; }

        /**
         * The parent node property (read-only).
         * @since 1.5
         */
        public Node? Parent { get; set; }
        public abstract bool HasChildren { get; }

        ///**
        // * The child node count property (read-only).
        // *
        // * @since 1.5
        // */
        //public virtual int Count
        //{
        //    get { return 0; }
        //}

        ///**
        // * Returns the number of child nodes.
        // *
        // * @return the number of child nodes
        // *
        // * @deprecated Use the Count property instead.
        // */
        //public virtual int GetChildCount()
        //{
        //    return Count;
        //}

        ///**
        // * Returns the number of descendant nodes.
        // *
        // * @return the number of descendant nodes
        // *
        // * @since 1.2
        // */
        //public int GetDescendantCount()
        //{
        //    int count = 0;

        //    for (int i = 0; i < Count; i++)
        //    {
        //        count += 1 + this[i].GetDescendantCount();
        //    }
        //    return count;
        //}

        ///**
        // * The child node index (read-only).
        // *
        // * @param index          the child index, 0 <= index < Count
        // *
        // * @return the child node found, or
        // *         null if index out of bounds
        // *
        // * @since 1.5
        // */
        //public virtual Node this[int index]
        //{
        //    get { return null; }
        //}

        ///**
        // * Returns the child node with the specified index.
        // *
        // * @param index          the child index, 0 <= index < count
        // *
        // * @return the child node found, or
        // *         null if index out of bounds
        // *
        // * @deprecated Use the class indexer instead.
        // */
        //public virtual Node GetChildAt(int index)
        //{
        //    return this[index];
        //}

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
        //public ArrayList Values
        //{
        //    get
        //    {
        //        values ??= new ArrayList();
        //        return values;
        //    }
        //    set { this.values = value; }
        //}

        ///**
        // * Returns a computed value of this node, if previously set. A
        // * value may be used for storing intermediate results in the
        // * parse tree during analysis.
        // *
        // * @param pos             the value position, 0 <= pos < count
        // *
        // * @return the computed node value, or
        // *         null if not set
        // *
        // * @see #Values
        // *
        // * @deprecated Use the Values property and it's array indexer
        // *     instead.
        // */
        //public object GetValue(int pos)
        //{
        //    return Values[pos];
        //}

        ///**
        // * Adds a computed value to this node. The computed value may
        // * be used for storing intermediate results in the parse tree
        // * during analysis.
        // *
        // * @param value          the node value
        // *
        // * @see #Values
        // *
        // * @deprecated Use the Values property and the Values.Add
        // *     method instead.
        // */
        //public void AddValue(object value)
        //{
        //    if (value != null)
        //    {
        //        Values.Add(value);
        //    }
        //}

        ///**
        // * Adds a set of computed values to this node.
        // *
        // * @param values         the vector with node values
        // *
        // * @see #Values
        // *
        // * @deprecated Use the Values property and the Values.AddRange
        // *     method instead.
        // */
        //public void AddValues(ArrayList values)
        //{
        //    if (values != null)
        //    {
        //        Values.AddRange(values);
        //    }
        //}

        ///**
        // * Removes all computed values stored in this node.
        // *
        // * @see #Values
        // *
        // * @deprecated Use the Values property and the Values.Clear
        // *     method instead. Alternatively the Values property can
        // *     be set to null.
        // */
        //public void RemoveAllValues()
        //{
        //    values = null;
        //}


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
            //for (var i = 0; i < Count; i++)
            //{
            //    this[i].PrintTo(output, indent);
            //}
        }
    }
}

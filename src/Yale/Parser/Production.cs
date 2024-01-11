/*
 * Production.cs
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

using System.Collections.Generic;
using Yale.Expression.Elements.Base;

namespace Yale.Parser
{
    /**
     * A production node. This class represents a grammar production
     * (i.e. a list of child nodes) in a parse tree. The productions
     * are created by a parser, that adds children a according to a
     * set of production patterns (i.e. grammar rules).
     *
     * @author   Per Cederberg
     * @version  1.5
     */
    internal sealed class Production : Node
    {
        /**
         * The child nodes.
         */
        private readonly List<Node> children = new();

        /**
         * Creates a new production node.
         *
         * @param pattern        the production pattern
         */
        public Production(ProductionPattern pattern)
        {
            Pattern = pattern;
        }

        /**
         * The node type id property (read-only). This value is set as
         * a unique identifier for each type of node, in order to
         * simplify later identification.
         *
         * @since 1.5
         */
        public override TokenId TypeId
        {
            get { return Pattern.Id; }
        }

        /**
         * The node name property (read-only).
         *
         * @since 1.5
         */
        public override string Name
        {
            get { return Pattern.Name; }
        }

        /**
         * The child node count property (read-only).
         *
         * @since 1.5
         */
        public int Count
        {
            get { return children.Count; }
        }

        public List<BaseExpressionElement> Values
        {
            get;
        } = new();


        #region Children
        public Node this[int index]
        {
            get
            {
                return children[index];
            }
        }

        /**
         * Adds a child node. The node will be added last in the list of
         * children.
         *
         * @param child  the child node to add
         */
        public void AddChild(Node child)
        {
            child.Parent = this;
            children.Add(child);
        }

        #endregion Children

        /**
         * The production pattern property (read-only). This property
         * contains the production pattern linked to this production.
         *
         * @since 1.5
         */
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

        public override bool HasChildren => Values.Count > 0;

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
         *
         * @return a string representation of this production
         */
        public override string ToString()
        {
            return Pattern.Name + '(' + Pattern.Id + ')';
        }
    }
}

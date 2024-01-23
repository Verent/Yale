namespace Yale.Parser
{
    internal class Analyzer
    {
        /**
         * Creates a new parse tree analyzer.
         */
        public Analyzer() { }

        /**
         * Resets this analyzer when the parser is reset for another
         * input stream. The default implementation of this method does
         * nothing.
         *
         * @since 1.5
         */
        public virtual void Reset()
        {
            // Default implementation does nothing
        }

        /**
         * Analyzes a parse tree node by traversing all it's child nodes.
         * The tree traversal is depth-first, and the appropriate
         * callback methods will be called. If the node is a production
         * node, a new production node will be created and children will
         * be added by recursively processing the children of the
         * specified production node. This method is used to process a
         * parse tree after creation.
         *
         * @param node           the parse tree node to process
         *
         * @return the resulting parse tree node
         *
         * @throws ParserLogException if the node analysis discovered
         *             errors
         */
        public Node? Analyze(Node node)
        {
            ParserLogException log = new();

            node = Analyze(node, log);
            if (log.Count > 0)
            {
                throw log;
            }
            return node;
        }

        /**
         * Analyzes a parse tree node by traversing all it's child nodes.
         * The tree traversal is depth-first, and the appropriate
         * callback methods will be called. If the node is a production
         * node, a new production node will be created and children will
         * be added by recursively processing the children of the
         * specified production node. This method is used to process a
         * parse tree after creation.
         *
         * @param node           the parse tree node to process
         * @param log            the parser error log
         *
         * @return the resulting parse tree node
         */
        private Node? Analyze(Node node, ParserLogException log)
        {
            var errorCount = log.Count;

            if (node is Production prod)
            {
                prod = NewProduction(prod.Pattern);
                try
                {
                    Enter(prod);
                }
                catch (ParseException e)
                {
                    log.AddError(e);
                }
                for (var i = 0; i < prod.Count; i++)
                {
                    try
                    {
                        Child(prod, Analyze(prod[i], log));
                    }
                    catch (ParseException e)
                    {
                        log.AddError(e);
                    }
                }
                try
                {
                    return Exit(prod);
                }
                catch (ParseException e)
                {
                    if (errorCount == log.Count)
                    {
                        log.AddError(e);
                    }
                }
            }
            else if (node is Token token)
            {
                node.Values.Clear();
                try
                {
                    Enter(token);
                }
                catch (ParseException e)
                {
                    log.AddError(e);
                }
                try
                {
                    return Exit(token);
                }
                catch (ParseException e)
                {
                    if (errorCount == log.Count)
                    {
                        log.AddError(e);
                    }
                }
            }
            return null;
        }

        /**
         * Factory method to create a new production node. This method
         * can be overridden to provide other production implementations
         * than the default one.
         *
         * @param pattern        the production pattern
         *
         * @return the new production node
         *
         * @since 1.5
         */
        public virtual Production NewProduction(ProductionPattern pattern)
        {
            return new Production(pattern);
        }

        public virtual void Enter(Production production) { }

        public virtual void Enter(Token token) { }

        public virtual Token Exit(Token token)
        {
            return token;
        }

        public virtual Production Exit(Production production)
        {
            return production;
        }

        /**
         * Called when adding a child to a parse tree node. By default
         * this method adds the child to the production node. A subclass
         * can override this method to handle each node separately. Note
         * that the child node may be null if the corresponding exit()
         * method returned null.
         *
         * @param node           the parent node
         * @param child          the child node, or null
         *
         * @throws ParseException if the node analysis discovered errors
         */
        public virtual void Child(Production node, Node child)
        {
            node.AddChild(child);
        }

        /**
         * Returns a child at the specified position. If either the node
         * or the child node is null, this method will throw a parse
         * exception with the internal error type.
         *
         * @param node           the parent node
         * @param pos            the child position
         *
         * @return the child node
         *
         * @throws ParseException if either the node or the child node
         *             was null
         */
        protected static Node GetChildAt(Production production, int pos)
        {
            if (production is null)
            {
                throw new ParseException(
                    ParseException.ErrorType.Internal,
                    "attempt to read 'null' parse tree node",
                    -1,
                    -1
                );
            }
            var child = production[pos];
            if (child is null)
            {
                throw new ParseException(
                    ParseException.ErrorType.Internal,
                    "node '" + production.Name + "' has no child at " + "position " + pos,
                    production.StartLine,
                    production.StartColumn
                );
            }
            return child;
        }

        /**
         * Returns the node value at the specified position. If either
         * the node or the value is null, this method will throw a parse
         * exception with the internal error type.
         *
         * @param node           the parse tree node
         * @param pos            the child position
         *
         * @return the value object
         *
         * @throws ParseException if either the node or the value was null
         */
        protected static object? GetValue(Node? node, int pos)
        {
            if (node is null)
            {
                throw new ParseException(
                    ParseException.ErrorType.Internal,
                    "attempt to read 'null' parse tree node",
                    -1,
                    -1
                );
            }

            var value = node.Values[pos];
            if (value is not null)
                return value;

            throw new ParseException(
                ParseException.ErrorType.Internal,
                "node '" + node.Name + "' has no value at " + "position " + pos,
                node.StartLine,
                node.StartColumn
            );
        }

        /**
         * Returns the node integer value at the specified position. If
         * either the node is null, or the value is not an instance of
         * the Integer class, this method will throw a parse exception
         * with the internal error type.
         *
         * @param node           the parse tree node
         * @param pos            the child position
         *
         * @return the value object
         *
         * @throws ParseException if either the node was null, or the
         *             value wasn't an integer
         */
        protected static int GetIntValue(Node node, int pos)
        {
            var value = GetValue(node, pos);
            if (value is int intValue)
            {
                return intValue;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.Internal,
                    "node '" + node.Name + "' has no integer value " + "at position " + pos,
                    node.StartLine,
                    node.StartColumn
                );
            }
        }

        /**
         * Returns the node string value at the specified position. If
         * either the node is null, or the value is not an instance of
         * the String class, this method will throw a parse exception
         * with the internal error type.
         *
         * @param node           the parse tree node
         * @param pos            the child position
         *
         * @return the value object
         *
         * @throws ParseException if either the node was null, or the
         *             value wasn't a string
         */
        protected static string GetStringValue(Node node, int pos)
        {
            var value = GetValue(node, pos);
            if (value is string stringValue)
            {
                return stringValue;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.Internal,
                    "node '" + node.Name + "' has no string value " + "at position " + pos,
                    node.StartLine,
                    node.StartColumn
                );
            }
        }

        /**
         * Returns all the node values for all child nodes.
         *
         * @param node           the parse tree node
         *
         * @return a list with all the child node values
         *
         * @since 1.3
         */
        protected static List<object> GetChildValues(Production node)
        {
            List<object> result = new();

            for (var i = 0; i < node.Count; i++)
            {
                if (node[i] is Node child)
                {
                    var values = child.Values;
                    if (values is not null)
                    {
                        result.AddRange(values);
                    }
                }
            }
            return result;
        }
    }
}

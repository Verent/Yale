using PerCederberg.Grammatica.Runtime;
using System;
using System.Collections.Generic;
using Yale.Expression;
using Token = Yale.Parser.Token;

namespace Yale.Engine.Internal
{
    //Todo: Create a short description
    internal class IdentifierAnalyzer : Analyzer
    {
        private readonly IDictionary<int, string> _identifiers;
        private int _memberExpressionCount;

        private bool _inFieldPropertyExpression;

        public IdentifierAnalyzer()
        {
            _identifiers = new Dictionary<int, string>();
        }

        public override Node Exit(Node node)
        {
            switch (node.Id)
            {
                case (int)Token.IDENTIFIER:
                    ExitIdentifier((PerCederberg.Grammatica.Runtime.Token)node);
                    break;

                case (int)Token.FIELD_PROPERTY_EXPRESSION:
                    ExitFieldPropertyExpression();
                    break;
            }

            return node;
        }

        public override void Enter(Node node)
        {
            switch (node.Id)
            {
                case (int)Token.MEMBER_EXPRESSION:
                    EnterMemberExpression();
                    break;

                case (int)Token.FIELD_PROPERTY_EXPRESSION:
                    EnterFieldPropertyExpression();
                    break;
            }
        }

        private void ExitIdentifier(PerCederberg.Grammatica.Runtime.Token node)
        {
            if (_inFieldPropertyExpression == false)
            {
                return;
            }

            if (_identifiers.ContainsKey(_memberExpressionCount) == false)
            {
                _identifiers.Add(_memberExpressionCount, node.Image);
            }
        }

        private void EnterMemberExpression()
        {
            _memberExpressionCount += 1;
        }

        private void EnterFieldPropertyExpression()
        {
            _inFieldPropertyExpression = true;
        }

        private void ExitFieldPropertyExpression()
        {
            _inFieldPropertyExpression = false;
        }

        public override void Reset()
        {
            _identifiers.Clear();
            _memberExpressionCount = -1;
        }

        public ICollection<string> GetIdentifiers(ExpressionContext context)
        {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var options = context.Imports;

            foreach (var identifier in _identifiers.Values)
            {
                // Skip names registered as namespaces
                if (options.HasNamespace(identifier))
                {
                    continue;
                }

                // Get only the unique values
                dictionary[identifier] = null;
            }

            return dictionary.Keys;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Yale.Expression
{
    /// <summary>
    /// Holds information about a compiled expression.
    /// </summary>
    public sealed class ExpressionInfo
    {
        private readonly IDictionary<string, string> _referencedDictionary =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        internal ExpressionInfo()
        { }

        internal void AddReferencedVariable(string name)
        {
            _referencedDictionary[name] = name;
        }

        /// <summary>
        /// Gets the variables that are used in an expression.
        /// </summary>
        /// <returns></returns>
        public string[] GetReferencedVariables()
        {
            var referencedVariables = new string[_referencedDictionary.Count];
            _referencedDictionary.Keys.CopyTo(referencedVariables, 0);
            return referencedVariables;
        }
    }
}
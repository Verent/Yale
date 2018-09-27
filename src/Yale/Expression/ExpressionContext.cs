using System;
using System.Collections.Generic;
using System.Linq;
using Yale.Core;
using Yale.Engine;

namespace Yale.Expression
{
    internal class ExpressionContext
    {
        public ExpressionContext(string expressionName)
        {
            ExpressionName = expressionName;
        }

        public ExpressionContext(ExpressionBuilderOptions builderOptions, string expressionName, object owner)
        {
            BuilderOptions = builderOptions;
            ExpressionName = expressionName;
            Owner = owner;
        }

        public object Owner { get; }

        public Type OwnerType => Owner.GetType();

        internal ExpressionBuilderOptions BuilderOptions { get; } = new ExpressionBuilderOptions();

        public TypeImports Imports { get; set; }

        public ValueCollection Values { get; set; }

        public ComputeInstance ComputeInstance { get; set; }

        public string ExpressionName { get; }

        //Todo: What does the StringComparer do in this HashSet? Remove this construct?
        private readonly HashSet<string> _referencedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal void AddReferencedVariable(string name)
        {
            _referencedValues.Add(name);
        }

        public string[] GetReferencedValues()
        {
            return _referencedValues.ToArray();
        }
    }
}
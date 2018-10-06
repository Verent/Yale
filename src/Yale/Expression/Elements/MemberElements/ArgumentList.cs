using System;
using System.Collections;
using System.Collections.Generic;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.MemberElements
{
    /// <summary>
    /// Encapsulates an argument list
    /// </summary>
    internal class ArgumentList
    {
        private readonly IList<ExpressionElement> _elements;

        public ArgumentList(ICollection elements)
        {
            var arr = new ExpressionElement[elements.Count];
            elements.CopyTo(arr, 0);
            _elements = arr;
        }

        private string[] GetArgumentTypeNames()
        {
            var list = new List<string>();

            foreach (var expressionElement in _elements)
            {
                list.Add(expressionElement.ResultType.Name);
            }

            return list.ToArray();
        }

        public Type[] GetArgumentTypes()
        {
            var list = new List<Type>();

            foreach (var e in _elements)
            {
                list.Add(e.ResultType);
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            var typeNames = GetArgumentTypeNames();
            return Utility.FormatList(typeNames);
        }

        public ExpressionElement[] ToArray()
        {
            var arr = new ExpressionElement[_elements.Count];
            _elements.CopyTo(arr, 0);
            return arr;
        }

        public ExpressionElement this[int index] => _elements[index];

        public int Count => _elements.Count;
    }
}
using System;
using Yale.Core.Interface;

namespace Yale.Core
{
    internal class Variable : IVariable
    {
        public Variable(object value)
        {
            Type = value?.GetType();
            ValueAsObject = value;
        }

        public Type Type { get; }

        public object ValueAsObject { get; }
    }
}
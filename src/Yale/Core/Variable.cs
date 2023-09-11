using System;
using Yale.Core.Interfaces;

namespace Yale.Core
{
    internal class Variable : IVariable
    {
        public Variable(object value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            Type = value.GetType();
            ValueAsObject = value;
        }

        public Type Type { get; }

        public object ValueAsObject { get; }
    }
}
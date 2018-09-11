using System;
using Yale.Core.Interface;

namespace Yale.Core
{
    public class Value : IValue
    {
        public Value(object value)
        {
            VariableType = value?.GetType();
            ValueAsObject = value;
        }

        public Type VariableType { get; }

        public object ValueAsObject { get; }
    }
}
using System;
using Yale.Core.Interface;

namespace Yale.Core
{
    internal class GenericValue<T> : IValue
    {
        public GenericValue(T value)
        {
            Value = value;
        }

        public Type VariableType => typeof(T);

        public object ValueAsObject => Value;

        public T Value { get; private set; }
    }
}
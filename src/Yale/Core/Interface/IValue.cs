using System;

namespace Yale.Core.Interface
{
    public interface IValue
    {
        Type VariableType { get; }

        object ValueAsObject { get; }
    }
}
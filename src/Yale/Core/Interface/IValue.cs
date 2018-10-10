using System;

namespace Yale.Core.Interface
{
    internal interface IValue
    {
        Type VariableType { get; }

        object ValueAsObject { get; }
    }
}
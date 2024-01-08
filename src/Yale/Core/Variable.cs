using System;
using Yale.Core.Interfaces;

namespace Yale.Core;

internal sealed class Variable : IVariable
{
    public Variable(object value)
    {
        Type = value.GetType();
        ValueAsObject = value;
    }

    public Type Type { get; }

    public object ValueAsObject { get; }
}

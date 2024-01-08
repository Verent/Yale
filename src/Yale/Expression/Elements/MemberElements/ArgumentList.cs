using System;
using System.Collections;
using System.Collections.Generic;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.MemberElements;

/// <summary>
/// Encapsulates an argument list
/// </summary>
internal sealed class ArgumentList
{
    private readonly BaseExpressionElement[] _elements;

    public ArgumentList(ICollection elements)
    {
        var arr = new BaseExpressionElement[elements.Count];
        elements.CopyTo(arr, 0);
        _elements = arr;
    }

    private string[] GetArgumentTypeNames()
    {
        List<string> list = new();

        foreach (var expressionElement in _elements)
        {
            list.Add(expressionElement.ResultType.Name);
        }

        return list.ToArray();
    }

    public Type[] GetArgumentTypes()
    {
        List<Type> list = new();

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

    public BaseExpressionElement[] ToArray()
    {
        var arr = new BaseExpressionElement[_elements.Length];
        _elements.CopyTo(arr, 0);
        return arr;
    }

    public BaseExpressionElement this[int index] => _elements[index];

    public int Count => _elements.Length;
}

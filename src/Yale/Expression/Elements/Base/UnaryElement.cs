using System;

namespace Yale.Expression.Elements.Base;

/// <summary>
/// Base element that takes one operand (example: Not and Negate)
/// </summary>
internal abstract class UnaryElement : BaseExpressionElement
{
    public UnaryElement(BaseExpressionElement child)
    {
        MyChild = child;
    }

    protected BaseExpressionElement MyChild;

    protected abstract Type GetResultType(Type childType);
}

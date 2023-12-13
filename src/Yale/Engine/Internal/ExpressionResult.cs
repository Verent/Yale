using System;
using Yale.Engine.Interface;
using Yale.Expression;

namespace Yale.Engine.Internal;

/// <summary>
/// Represents the calculated result of an expression
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Runtime.InteropServices.Guid("7B091BCF-2B30-4056-8D51-054E93947FB4")]
internal sealed class ExpressionResult<T> : IExpressionResult
{
    public Expression<T> Expression { get; }
    public string Name { get; }

    public T Result { get; private set; }

    internal ExpressionResult(string name, Expression<T> expression)
    {
        Name = name;
        Expression = expression;
        Result = Expression.Evaluate();
    }

    public void Recalculate()
    {
        Result = Expression.Evaluate();
        Dirty = false;
    }

    public bool Dirty { get; set; }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    public Type ResultType => Result.GetType();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8603 // Possible null reference return.
    public object ResultAsObject => Result;
#pragma warning restore CS8603 // Possible null reference return.

    public Expression<T> GetExpression() => Expression;

    public override string ToString() => Name;
}

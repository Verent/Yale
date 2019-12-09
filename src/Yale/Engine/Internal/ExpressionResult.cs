using System;
using Yale.Engine.Interface;
using Yale.Expression;

namespace Yale.Engine.Internal
{
    /// <summary>
    /// Represents the calculated result of an expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Runtime.InteropServices.Guid("7B091BCF-2B30-4056-8D51-054E93947FB4")]
    internal class ExpressionResult<T> : IExpressionResult
    {
        public Expression<T> Expression { get; }
        public string Name { get; }
        public T Result { get; private set; }

        internal ExpressionResult(string name, Expression<T> expression)
        {
            Expression = expression;
            Name = name;
            Result = Expression.Evaluate();
        }

        public void Recalculate()
        {
            Result = Expression.Evaluate();
            Dirty = false;
        }

        public bool Dirty { get; set; }

        public Type ResultType => Result.GetType();

        public object ResultAsObject => Result;

        public Expression<T> GetExpression() => Expression;

        public override string ToString() => Name;

    }
}
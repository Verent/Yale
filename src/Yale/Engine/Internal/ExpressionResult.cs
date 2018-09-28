using System;
using Yale.Engine.Interface;
using Yale.Expression;

namespace Yale.Engine.Internal
{
    /// <summary>
    /// Represents the calculated result of an expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ExpressionResult<T> : IExpressionResult
    {
        public Expression<T> Expression { get; }
        public string Name { get; }
        public T Result { get; set; }

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

        public Expression<T> GetExpression()
        {
            return Expression;
        }

        public Type ResultType => Result.GetType();

        public object ResultAsObject => Result;

        public override string ToString()
        {
            return Name;
        }
    }
}
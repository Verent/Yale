using System;

namespace Yale.Expression.Elements.Base
{
    internal abstract class UnaryElement : ExpressionElement
    {
        public UnaryElement(ExpressionElement child)
        {
            MyChild = child;
        }

        protected ExpressionElement MyChild;

        protected abstract Type GetResultType(Type childType);
    }
}
using System;

namespace Yale.Expression.Elements.Base
{
    internal abstract class UnaryElement : BaseExpressionElement
    {
        public UnaryElement(BaseExpressionElement child)
        {
            MyChild = child;
        }

        protected BaseExpressionElement MyChild;

        protected abstract Type GetResultType(Type childType);
    }
}
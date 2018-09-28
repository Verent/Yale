﻿using System;
using Flee.Resources;
using Yale.Resources;

namespace Yale.Expression.Elements.Base
{
    internal abstract class UnaryElement : ExpressionElement
    {

        protected ExpressionElement MyChild;

        private Type _myResultType;
        public void SetChild(ExpressionElement child)
        {
            MyChild = child;
            _myResultType = GetResultType(child.ResultType);

            if (_myResultType == null)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.OperationNotDefinedForType, CompileExceptionReason.TypeMismatch, MyChild.ResultType.Name);
            }
        }

        protected abstract Type GetResultType(Type childType);

        public override Type ResultType => _myResultType;
    }

}
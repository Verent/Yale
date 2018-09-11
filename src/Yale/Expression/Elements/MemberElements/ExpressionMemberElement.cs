using System;
using Yale.Expression.Elements.Base;
using Yale.Internal;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.MemberElements
{
    internal class ExpressionMemberElement : MemberElement
    {
        private readonly ExpressionElement _myElement;

        public ExpressionMemberElement(ExpressionElement element)
        {
            _myElement = element;
        }

        protected override void ResolveInternal()
        {
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            base.Emit(ilGenerator, context);
            _myElement.Emit(ilGenerator, context);
            if (_myElement.ResultType.IsValueType)
            {
                EmitValueTypeLoadAddress(ilGenerator, ResultType);
            }
        }

        protected override bool SupportsInstance => true;

        protected override bool IsPublic => true;

        public override bool IsStatic => false;

        public override Type ResultType => _myElement.ResultType;
    }
}
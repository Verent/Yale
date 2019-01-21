using System;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals
{
    internal class BooleanLiteralElement : LiteralElement
    {
        private readonly bool _value;

        public BooleanLiteralElement(bool value)
        {
            _value = value;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            EmitLoad(_value, ilGenerator);
        }

        public override Type ResultType => typeof(bool);
    }
}
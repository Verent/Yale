using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals
{
    internal class StringLiteralElement : LiteralElement
    {
        private readonly string _value;

        public StringLiteralElement(string value)
        {
            _value = value;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            ilGenerator.Emit(OpCodes.Ldstr, _value);
        }

        public override Type ResultType => typeof(string);
    }
}
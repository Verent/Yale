using System;
using Yale.Expression.Elements.Base.Literals;
using Yale.Internal;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals
{
    internal class CharLiteralElement : LiteralElement
    {
        private readonly char _value;

        public CharLiteralElement(char value)
        {
            _value = value;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var intValue = Convert.ToInt32(_value);
            EmitLoad(intValue, ilGenerator);
        }

        public override Type ResultType => typeof(char);
    }
}
using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals
{
    internal class NullLiteralElement : LiteralElement
    {
        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            ilGenerator.Emit(OpCodes.Ldnull);
        }

        public override Type ResultType => typeof(Null);
    }
}
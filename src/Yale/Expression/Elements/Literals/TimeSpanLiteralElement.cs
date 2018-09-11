using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Internal;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Literals
{
    internal class TimeSpanLiteralElement : LiteralElement
    {
        private TimeSpan _myValue;

        public TimeSpanLiteralElement(string image)
        {
            if (TimeSpan.TryParse(image, out _myValue) == false)
            {
                ThrowCompileException(CompileErrorResourceKeys.CannotParseType, CompileExceptionReason.InvalidFormat, typeof(TimeSpan).Name);
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var index = ilGenerator.GetTempLocalIndex(typeof(TimeSpan));

            Utility.EmitLoadLocalAddress(ilGenerator, index);

            EmitLoad(_myValue.Ticks, ilGenerator);

            var constructorInfo = typeof(TimeSpan).GetConstructor(new[] { typeof(Int64) });

            ilGenerator.Emit(OpCodes.Call, constructorInfo);

            Utility.EmitLoadLocal(ilGenerator, index);
        }

        public override Type ResultType => typeof(TimeSpan);
    }
}
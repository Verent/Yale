﻿using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Literals;

internal sealed class TimeSpanLiteralElement : LiteralElement
{
    private readonly TimeSpan _value;

    public TimeSpanLiteralElement(string image)
    {
        if (TimeSpan.TryParse(image, out _value) == false)
        {
            throw CreateCompileException(
                CompileErrors.CannotParseType,
                CompileExceptionReason.InvalidFormat,
                typeof(TimeSpan).Name
            );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        int index = ilGenerator.GetTempLocalIndex(typeof(TimeSpan));

        Utility.EmitLoadLocalAddress(ilGenerator, index);

        EmitLoad(_value.Ticks, ilGenerator);

        ConstructorInfo constructorInfo = typeof(TimeSpan).GetConstructor(new[] { typeof(long) })!;

        ilGenerator.Emit(OpCodes.Call, constructorInfo);

        Utility.EmitLoadLocal(ilGenerator, index);
    }

    public override Type ResultType => typeof(TimeSpan);
}

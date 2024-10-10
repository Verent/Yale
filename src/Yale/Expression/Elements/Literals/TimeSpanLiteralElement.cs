using Yale.Expression.Elements.Base.Literals;
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
                nameof(TimeSpan)
            );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        var index = ilGenerator.GetTempLocalIndex(typeof(TimeSpan));

        Utility.EmitLoadLocalAddress(ilGenerator, index);

        EmitLoad(_value.Ticks, ilGenerator);

        var constructorInfo = typeof(TimeSpan).GetConstructor(new[] { typeof(long) })!;

        ilGenerator.Emit(OpCodes.Call, constructorInfo);

        Utility.EmitLoadLocal(ilGenerator, index);
    }

    public override Type ResultType => typeof(TimeSpan);
}

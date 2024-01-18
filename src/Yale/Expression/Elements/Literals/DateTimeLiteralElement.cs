using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Literals;

internal sealed class DateTimeLiteralElement : LiteralElement
{
    private readonly DateTime _value;

    public DateTimeLiteralElement(string image, ExpressionContext context)
    {
        ExpressionBuilderOptions options = context.BuilderOptions;

        if (
            DateTime.TryParseExact(
                image,
                options.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _value
            ) == false
        )
        {
            throw CreateCompileException(
                CompileErrors.CannotParseType,
                CompileExceptionReason.InvalidFormat,
                typeof(DateTime).Name
            );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        int index = ilGenerator.GetTempLocalIndex(typeof(DateTime));

        Utility.EmitLoadLocalAddress(ilGenerator, index);

        EmitLoad(_value.Ticks, ilGenerator);

        System.Reflection.ConstructorInfo constructor = typeof(DateTime).GetConstructor(
            new[] { typeof(long) }
        )!;

        ilGenerator.Emit(OpCodes.Call, constructor);

        Utility.EmitLoadLocal(ilGenerator, index);
    }

    public override Type ResultType => typeof(DateTime);
}

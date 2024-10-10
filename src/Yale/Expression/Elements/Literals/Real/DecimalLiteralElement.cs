using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Real;

internal sealed class DecimalLiteralElement : RealLiteralElement
{
    private static readonly ConstructorInfo OurConstructorInfo = GetConstructor();
    private readonly decimal _value;

    private DecimalLiteralElement() { }

    public DecimalLiteralElement(decimal value) => _value = value;

    private static ConstructorInfo GetConstructor()
    {
        Type[] types = { typeof(Int32), typeof(Int32), typeof(Int32), typeof(bool), typeof(byte) };
        return typeof(decimal).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            null,
            CallingConventions.Any,
            types,
            null
        );
    }

    public static DecimalLiteralElement Parse(string image)
    {
        DecimalLiteralElement element = new();

        try
        {
            var value = decimal.Parse(image, CultureInfo.InvariantCulture);
            return new DecimalLiteralElement(value);
        }
        catch (OverflowException)
        {
            element.OnParseOverflow(image);
            return null;
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        var index = ilGenerator.GetTempLocalIndex(typeof(decimal));
        Utility.EmitLoadLocalAddress(ilGenerator, index);

        var bits = decimal.GetBits(_value);
        EmitLoad(bits[0], ilGenerator);
        EmitLoad(bits[1], ilGenerator);
        EmitLoad(bits[2], ilGenerator);

        var flags = bits[3];

        EmitLoad((flags >> 31) == -1, ilGenerator);

        EmitLoad(flags >> 16, ilGenerator);

        ilGenerator.Emit(OpCodes.Call, OurConstructorInfo);

        Utility.EmitLoadLocal(ilGenerator, index);
    }

    public override Type ResultType => typeof(decimal);
}

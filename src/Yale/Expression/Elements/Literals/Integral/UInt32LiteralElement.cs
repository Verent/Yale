using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Integral;

internal sealed class UInt32LiteralElement : IntegralLiteralElement
{
    private readonly uint _myValue;

    public UInt32LiteralElement(uint value) => _myValue = value;

    public static UInt32LiteralElement? TryCreate(string image, NumberStyles numberStyles)
    {
        if (uint.TryParse(image, numberStyles, null, out var value))
        {
            return new UInt32LiteralElement(value);
        }

        return null;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context) =>
        EmitLoad(Convert.ToInt32(_myValue), ilGenerator);

    public override Type ResultType => typeof(uint);
}

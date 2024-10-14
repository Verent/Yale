using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Integral;

internal sealed class UInt64LiteralElement : IntegralLiteralElement
{
    private readonly UInt64 _value;

    public UInt64LiteralElement(string image, NumberStyles ns)
    {
        try
        {
            _value = UInt64.Parse(image, ns);
        }
        catch (OverflowException)
        {
            OnParseOverflow(image);
        }
    }

    public UInt64LiteralElement(UInt64 value) => _value = value;

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context) =>
        EmitLoad(Convert.ToInt64(_value), ilGenerator);

    public override Type ResultType => typeof(UInt64);
}

using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Integral;

internal sealed class Int64LiteralElement : IntegralLiteralElement
{
    private Int64 _value;
    private const string MinValue = "9223372036854775808";
    private readonly bool _isMinValue;

    public Int64LiteralElement(Int64 value) => _value = value;

    private Int64LiteralElement() => _isMinValue = true;

    public static Int64LiteralElement TryCreate(string image, bool isHex, bool negated)
    {
        if (negated & image == MinValue)
        {
            return new Int64LiteralElement();
        }

        if (isHex)
        {
            if (Int64.TryParse(image, NumberStyles.AllowHexSpecifier, null, out var value) == false)
            {
                return null;
            }

            //Todo: What does this do?
            if (value >= 0 & value <= Int64.MaxValue)
            {
                return new Int64LiteralElement(value);
            }

            return null;
        }
        else
        {
            if (Int64.TryParse(image, out var value))
            {
                return new Int64LiteralElement(value);
            }
            else
            {
                return null;
            }
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context) =>
        EmitLoad(_value, ilGenerator);

    public void Negate()
    {
        if (_isMinValue)
        {
            _value = Int64.MinValue;
        }
        else
        {
            _value = -_value;
        }
    }

    public override Type ResultType => typeof(Int64);
}

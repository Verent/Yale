using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Integral;

internal sealed class Int32LiteralElement : IntegralLiteralElement
{
    private const string MinValue = "2147483648";
    private readonly bool isMinValue;

    public Int32LiteralElement(int value)
    {
        Value = value;
    }

    private Int32LiteralElement()
    {
        isMinValue = true;
    }

    /// <summary>
    /// Todo: Add description
    /// </summary>
    /// <param name="image"></param>
    /// <param name="isHex"></param>
    /// <param name="negated"></param>
    /// <returns></returns>
    public static Int32LiteralElement? TryCreate(string image, bool isHex, bool negated)
    {
        if (negated & image == MinValue)
        {
            return new Int32LiteralElement();
        }

        if (isHex)
        {
            // Since Int32.TryParse will succeed for a string like 0xFFFFFFFF we have to do some special handling
            if (int.TryParse(image, NumberStyles.AllowHexSpecifier, null, out int value) == false)
            {
                return null;
            }

            if (value >= 0 & value <= int.MaxValue)
            {
                return new Int32LiteralElement(value);
            }

            return null;
        }
        else
        {
            return int.TryParse(image, out int value) ? new Int32LiteralElement(value) : null;
        }
    }

    public void Negate()
    {
        if (isMinValue)
        {
            Value = int.MinValue;
        }
        else
        {
            Value = -Value;
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        EmitLoad(Value, ilGenerator);
    }

    public override Type ResultType => typeof(int);

    public int Value { get; private set; }
}

using System;
using System.Globalization;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Integral;

internal class UInt32LiteralElement : IntegralLiteralElement
{
    private readonly UInt32 _myValue;

    public UInt32LiteralElement(UInt32 value)
    {
        _myValue = value;
    }

    public static UInt32LiteralElement TryCreate(string image, NumberStyles numberStyles)
    {
        if (UInt32.TryParse(image, numberStyles, null, out uint value))
        {
            return new UInt32LiteralElement(value);
        }

        return null;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        EmitLoad(Convert.ToInt32(_myValue), ilGenerator);
    }

    public override Type ResultType => typeof(UInt32);
}

using System;
using System.Globalization;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Real;

internal class SingleLiteralElement : RealLiteralElement
{
    private readonly float _value;

    private SingleLiteralElement() { }

    public SingleLiteralElement(float value)
    {
        _value = value;
    }

    public static SingleLiteralElement Parse(string image)
    {
        SingleLiteralElement element = new();

        try
        {
            float value = float.Parse(image, CultureInfo.InvariantCulture);
            return new SingleLiteralElement(value);
        }
        catch (OverflowException)
        {
            element.OnParseOverflow(image);
            return null;
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        ilGenerator.Emit(OpCodes.Ldc_R4, _value);
    }

    public override Type ResultType => typeof(float);
}

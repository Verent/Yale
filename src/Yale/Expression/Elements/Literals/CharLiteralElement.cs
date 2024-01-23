using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals;

internal sealed class CharLiteralElement : LiteralElement
{
    private readonly char _value;

    public CharLiteralElement(char value)
    {
        _value = value;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        int intValue = Convert.ToInt32(_value);
        EmitLoad(intValue, ilGenerator);
    }

    public override Type ResultType => typeof(char);
}

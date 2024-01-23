using Yale.Expression;
using Yale.Expression.Elements.Base;

namespace Yale.Parser.Internal;

/// <summary>
/// Wraps an expression element so that it is loaded from a local slot
/// </summary>
internal sealed class LocalBasedElement : BaseExpressionElement
{
    private readonly int _index;

    private readonly BaseExpressionElement _target;

    public LocalBasedElement(BaseExpressionElement target, int index)
    {
        _target = target;
        _index = index;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        Utility.EmitLoadLocal(ilGenerator, _index);
    }

    public override Type ResultType => _target.ResultType;
}

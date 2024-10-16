﻿using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.MemberElements;

internal sealed class ExpressionMemberElement : MemberElement
{
    private readonly BaseExpressionElement _myElement;

    public ExpressionMemberElement(BaseExpressionElement element) => _myElement = element;

    protected override void ResolveInternal() { }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        base.Emit(ilGenerator, context);
        _myElement.Emit(ilGenerator, context);
        if (_myElement.ResultType.IsValueType)
        {
            EmitValueTypeLoadAddress(ilGenerator, ResultType);
        }
    }

    protected override bool SupportsInstance => true;

    protected override bool IsPublic => true;

    public override bool IsStatic => false;

    public override Type ResultType => _myElement.ResultType;
}

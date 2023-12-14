using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.LogicalBitwise;

/// <summary>
///
/// </summary>
internal class NotElement : UnaryElement
{
    public override Type ResultType { get; }

    public NotElement(BaseExpressionElement child)
        : base(child)
    {
        ResultType = GetResultType(child.ResultType);
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        if (ReferenceEquals(MyChild.ResultType, typeof(bool)))
        {
            EmitLogical(ilGenerator, context);
        }
        else
        {
            MyChild.Emit(ilGenerator, context);
            ilGenerator.Emit(OpCodes.Not);
        }
    }

    private void EmitLogical(YaleIlGenerator ilg, ExpressionContext context)
    {
        MyChild.Emit(ilg, context);
        ilg.Emit(OpCodes.Ldc_I4_0);
        ilg.Emit(OpCodes.Ceq);
    }

    protected override Type GetResultType(Type childType)
    {
        if (ReferenceEquals(childType, typeof(bool)))
        {
            return typeof(bool);
        }
        Type result = Utility.IsIntegralType(childType)
            ? childType
            : throw CreateCompileException(
                CompileErrors.OperationNotDefinedForType,
                CompileExceptionReason.TypeMismatch,
                MyChild.ResultType.Name
            );

        return result;
    }
}

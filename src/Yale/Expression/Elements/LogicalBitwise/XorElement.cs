using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.LogicalBitwise;

internal class XorElement : BinaryExpressionElement
{
    protected override Type GetResultType(Type leftType, Type rightType)
    {
        Type bitwiseType = Utility.GetBitwiseOpType(leftType, rightType);

        if (bitwiseType != null)
        {
            return bitwiseType;
        }

        return AreBothChildrenOfType(typeof(bool)) ? typeof(bool) : null;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        Type resultType = ResultType;

        LeftChild.Emit(ilGenerator, context);
        ImplicitConverter.EmitImplicitConvert(LeftChild.ResultType, resultType, ilGenerator);
        RightChild.Emit(ilGenerator, context);
        ImplicitConverter.EmitImplicitConvert(RightChild.ResultType, resultType, ilGenerator);
        ilGenerator.Emit(OpCodes.Xor);
    }

    protected override void GetOperation(object operation) { }
}

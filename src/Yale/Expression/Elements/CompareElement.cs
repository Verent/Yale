using System.Diagnostics;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements;

internal sealed class CompareElement : BinaryExpressionElement
{
    private LogicalCompareOperation operation;

    public void Initialize(
        BaseExpressionElement leftChild,
        BaseExpressionElement rightChild,
        LogicalCompareOperation operation
    )
    {
        this.operation = operation;
        LeftChild = leftChild;
        RightChild = (Int32LiteralElement)rightChild;
    }

    public void Validate() => ValidateInternal(operation);

    protected override void GetOperation(object operation) =>
        this.operation = (LogicalCompareOperation)operation;

    protected override Type? GetResultType(Type leftType, Type rightType)
    {
        Type binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
        MethodInfo overloadedOperator = GetOverloadedCompareOperator();
        bool isEqualityOp = IsOpTypeEqualOrNotEqual(operation);

        // Use our string equality instead of overloaded operator
        if (
            ReferenceEquals(leftType, typeof(string))
            & ReferenceEquals(rightType, typeof(string))
            & isEqualityOp
        )
        {
            // String equality
            return typeof(bool);
        }

        if (overloadedOperator != null)
        {
            return overloadedOperator.ReturnType;
        }

        if (binaryResultType != null)
        {
            // Comparison of numeric operands
            return typeof(bool);
        }

        if (
            ReferenceEquals(leftType, typeof(bool))
            & ReferenceEquals(rightType, typeof(bool))
            & isEqualityOp
        )
        {
            // Boolean equality
            return typeof(bool);
        }

        if (AreBothChildrenReferenceTypes() & isEqualityOp)
        {
            // Comparison of reference types
            return typeof(bool);
        }

        if (AreBothChildrenSameEnum())
        {
            return typeof(bool);
        }

        // Invalid operands
        return null;
    }

    private MethodInfo GetOverloadedCompareOperator()
    {
        string name = GetCompareOperatorName(operation);
        return GetOverloadedBinaryOperator(name, operation);
    }

    private static string GetCompareOperatorName(LogicalCompareOperation op)
    {
        switch (op)
        {
            case LogicalCompareOperation.Equal:
                return "Equality";

            case LogicalCompareOperation.NotEqual:
                return "Inequality";

            case LogicalCompareOperation.GreaterThan:
                return "GreaterThan";

            case LogicalCompareOperation.LessThan:
                return "LessThan";

            case LogicalCompareOperation.GreaterThanOrEqual:
                return "GreaterThanOrEqual";

            case LogicalCompareOperation.LessThanOrEqual:
                return "LessThanOrEqual";

            default:
                Debug.Assert(false, "unknown compare type");
                return null;
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        Type binaryResultType = ImplicitConverter.GetBinaryResultType(
            LeftChild.ResultType,
            RightChild.ResultType
        );
        MethodInfo overloadedOperator = GetOverloadedCompareOperator();

        if (AreBothChildrenOfType(typeof(string)))
        {
            // String equality
            LeftChild.Emit(ilGenerator, context);
            RightChild.Emit(ilGenerator, context);
            EmitStringEquality(ilGenerator, operation, context);
        }
        else if (overloadedOperator != null)
        {
            EmitOverloadedOperatorCall(overloadedOperator, ilGenerator, context);
        }
        else if (binaryResultType != null)
        {
            // Emit a compare of numeric operands
            EmitChildWithConvert(LeftChild, binaryResultType, ilGenerator, context);
            EmitChildWithConvert(RightChild, binaryResultType, ilGenerator, context);
            EmitCompareOperation(ilGenerator, operation);
        }
        else if (AreBothChildrenOfType(typeof(bool)))
        {
            // Boolean equality
            EmitRegular(ilGenerator, context);
        }
        else if (AreBothChildrenReferenceTypes())
        {
            // Reference equality
            EmitRegular(ilGenerator, context);
        }
        else if (LeftChild.ResultType.IsEnum & RightChild.ResultType.IsEnum)
        {
            EmitRegular(ilGenerator, context);
        }
        else
        {
            Debug.Fail("unknown operand types");
        }
    }

    private void EmitRegular(YaleIlGenerator ilg, ExpressionContext context)
    {
        LeftChild.Emit(ilg, context);
        RightChild.Emit(ilg, context);
        EmitCompareOperation(ilg, operation);
    }

    private static void EmitStringEquality(
        YaleIlGenerator ilg,
        LogicalCompareOperation op,
        ExpressionContext context
    )
    {
        // Get the StringComparison from the options
        ExpressionBuilderOptions options = context.BuilderOptions;
        Int32LiteralElement int32LiteralElement = new((int)options.StringComparison);

        int32LiteralElement.Emit(ilg, context);

        // and emit the method call
        var methodInfo = typeof(string).GetMethod(
            "Equals",
            new[] { typeof(string), typeof(string), typeof(StringComparison) },
            null
        );
        ilg.Emit(OpCodes.Call, methodInfo);

        if (op == LogicalCompareOperation.NotEqual)
        {
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Ceq);
        }
    }

    private static bool IsOpTypeEqualOrNotEqual(LogicalCompareOperation op) =>
        op == LogicalCompareOperation.Equal | op == LogicalCompareOperation.NotEqual;

    private bool AreBothChildrenReferenceTypes() =>
        LeftChild.ResultType.IsValueType == false & RightChild.ResultType.IsValueType == false;

    private bool AreBothChildrenSameEnum() =>
        LeftChild.ResultType.IsEnum && ReferenceEquals(LeftChild.ResultType, RightChild.ResultType);

    /// <summary>
    /// Emit the actual compare
    /// </summary>
    /// <param name="ilg"></param>
    /// <param name="op"></param>
    private void EmitCompareOperation(YaleIlGenerator ilg, LogicalCompareOperation op)
    {
        OpCode ltOpcode = GetCompareGTLTOpcode(false);
        OpCode gtOpcode = GetCompareGTLTOpcode(true);

        switch (op)
        {
            case LogicalCompareOperation.Equal:
                ilg.Emit(OpCodes.Ceq);
                break;

            case LogicalCompareOperation.LessThan:
                ilg.Emit(ltOpcode);
                break;

            case LogicalCompareOperation.GreaterThan:
                ilg.Emit(gtOpcode);
                break;

            case LogicalCompareOperation.NotEqual:
                ilg.Emit(OpCodes.Ceq);
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ceq);
                break;

            case LogicalCompareOperation.LessThanOrEqual:
                ilg.Emit(gtOpcode);
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ceq);
                break;

            case LogicalCompareOperation.GreaterThanOrEqual:
                ilg.Emit(ltOpcode);
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ceq);
                break;

            default:
                Debug.Fail("Unknown op type");
                break;
        }
    }

    /// <summary>
    /// Get the correct greater/less than opcode
    /// </summary>
    /// <param name="greaterThan"></param>
    /// <returns></returns>
    private OpCode GetCompareGTLTOpcode(bool greaterThan)
    {
        Type leftType = LeftChild.ResultType;

        if (ReferenceEquals(leftType, RightChild.ResultType))
        {
            if (
                ReferenceEquals(leftType, typeof(UInt32))
                | ReferenceEquals(leftType, typeof(UInt64))
            )
            {
                return greaterThan ? OpCodes.Cgt_Un : OpCodes.Clt_Un;
            }

            return GetCompareOpcode(greaterThan);
        }

        return GetCompareOpcode(greaterThan);
    }

    private static OpCode GetCompareOpcode(bool greaterThan) =>
        greaterThan ? OpCodes.Cgt : OpCodes.Clt;
}

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements;

internal class ShiftElement : BinaryExpressionElement
{
    private ShiftOperation operation;

    protected override Type? GetResultType(Type leftType, Type rightType)
    {
        // Right argument (shift count) must be convertible to int32
        if (ImplicitConverter.EmitImplicitNumericConvert(rightType, typeof(Int32), null) == false)
        {
            return null;
        }

        // Left argument must be an integer type
        if (Utility.IsIntegralType(leftType) == false)
        {
            return null;
        }

        TypeCode typeCode = Type.GetTypeCode(leftType);

        switch (typeCode)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
                return typeof(Int32);

            case TypeCode.UInt32:
                return typeof(UInt32);

            case TypeCode.Int64:
                return typeof(Int64);

            case TypeCode.UInt64:
                return typeof(UInt64);

            default:
                Debug.Assert(false, "unknown left shift operand");
                return null;
        }
    }

    protected override void GetOperation(object operation) =>
        this.operation = (ShiftOperation)operation;

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        LeftChild.Emit(ilGenerator, context);
        EmitShiftCount(ilGenerator, context);
        EmitShift(ilGenerator);
    }

    // If the shift count is greater than the number of bits in the number, the result is undefined.
    // So we play it safe and force the shift count to 32/64 bits by ANDing it with the appropriate mask.
    private void EmitShiftCount(YaleIlGenerator ilg, ExpressionContext context)
    {
        RightChild.Emit(ilg, context);
        TypeCode typeCode = Type.GetTypeCode(LeftChild.ResultType);
        switch (typeCode)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
                ilg.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(0x1f));
                break;

            case TypeCode.Int64:
            case TypeCode.UInt64:
                ilg.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(0x3f));
                break;

            default:
                Debug.Assert(false, "unknown left shift operand");
                break;
        }

        ilg.Emit(OpCodes.And);
    }

    private void EmitShift(YaleIlGenerator ilg)
    {
        TypeCode typeCode = Type.GetTypeCode(LeftChild.ResultType);
        OpCode opCode = default;

        switch (typeCode)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                // Signed operand, emit a left shift or arithmetic right shift
                opCode = operation == ShiftOperation.LeftShift ? OpCodes.Shl : OpCodes.Shr;
                break;

            case TypeCode.UInt32:
            case TypeCode.UInt64:
                // Unsigned operand, emit left shift or logical right shift
                opCode = operation == ShiftOperation.LeftShift ? OpCodes.Shl : OpCodes.Shr_Un;
                break;

            default:
                Debug.Assert(false, "unknown left shift operand");
                break;
        }

        ilg.Emit(opCode);
    }
}

﻿using System.Diagnostics;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Base.Literals;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements;

internal sealed class ArithmeticElement : BinaryExpressionElement
{
    private static readonly MethodInfo powerMethodInfo = typeof(Math).GetMethod(
        nameof(Math.Pow),
        BindingFlags.Public | BindingFlags.Static
    )!;

    private static readonly MethodInfo stringConcatMethodInfo = typeof(string).GetMethod(
        nameof(string.Concat),
        new[] { typeof(string), typeof(string) },
        null
    )!;

    private static readonly MethodInfo objectConcatMethodInfo = typeof(string).GetMethod(
        nameof(string.Concat),
        new[] { typeof(object), typeof(object) },
        null
    )!;

    private BinaryArithmeticOperation operation;

    protected override void GetOperation(object operation) =>
        this.operation = (BinaryArithmeticOperation)operation;

    protected override Type? GetResultType(Type leftType, Type rightType)
    {
        var binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
        var overloadedMethod = GetOverloadedArithmeticOperator();

        // Is an overloaded operator defined for our left and right children?
        if (overloadedMethod is not null)
        {
            // Yes, so use its return type
            return overloadedMethod.ReturnType;
        }

        if (binaryResultType is not null)
        {
            // Operands are primitive types.  Return computed result type unless we are doing a power operation
            if (operation == BinaryArithmeticOperation.Power)
            {
                return GetPowerResultType(leftType);
            }

            return binaryResultType;
        }

        if (IsEitherChildOfType(typeof(string)) & (operation == BinaryArithmeticOperation.Add))
        {
            // String concatenation
            return typeof(string);
        }

        // Invalid types
        return null;
    }

    private Type GetPowerResultType(Type leftType) =>
        IsOptimizablePower ? leftType : typeof(double);

    /// <summary>
    /// Return an methodInfo based on the type of the left and right child
    /// </summary>
    /// <returns>Overloaded methodInfo or null if no overloaded method is needed</returns>
    private MethodInfo? GetOverloadedArithmeticOperator()
    {
        //Get the name of the operator
        var name = GetOverloadedOperatorFunctionName(operation);
        return GetOverloadedBinaryOperator(name, operation);
    }

    /// <summary>
    /// Returns the correct name used to generate IL operations
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    private static string GetOverloadedOperatorFunctionName(BinaryArithmeticOperation operation)
    {
        return operation switch
        {
            BinaryArithmeticOperation.Add => "Addition",
            BinaryArithmeticOperation.Subtract => "Subtraction",
            BinaryArithmeticOperation.Multiply => "Multiply",
            BinaryArithmeticOperation.Divide => "Division",
            BinaryArithmeticOperation.Mod => "Modulus",
            BinaryArithmeticOperation.Power => "Exponent",
            _
                => throw new InvalidOperationException(
                    $"Operation {operation} is not a valid arithmetic operation"
                ),
        };
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        var overloadedMethod = GetOverloadedArithmeticOperator();

        if (overloadedMethod is not null)
        {
            // Emit a call to an overloaded operator
            EmitOverloadedOperatorCall(overloadedMethod, ilGenerator, context);
        }
        else if (IsEitherChildOfType(typeof(string)))
        {
            // One of our operands is a string so emit a concatenation
            EmitStringConcat(ilGenerator, context);
        }
        else
        {
            // Emit a regular arithmetic operation
            EmitArithmeticOperation(operation, ilGenerator, context);
        }
    }

    private static bool IsUnsignedForArithmetic(Type type) =>
        ReferenceEquals(type, typeof(uint)) | ReferenceEquals(type, typeof(ulong));

    /// <summary>
    /// Emit an arithmetic operation with handling for unsigned and checked contexts
    /// </summary>
    private void EmitArithmeticOperation(
        BinaryArithmeticOperation operation,
        YaleIlGenerator ilGenerator,
        ExpressionContext context
    )
    {
        var options = context.BuilderOptions;
        var unsigned =
            IsUnsignedForArithmetic(LeftChild.ResultType)
            & IsUnsignedForArithmetic(RightChild.ResultType);
        var integral =
            Utility.IsIntegralType(LeftChild.ResultType)
            & Utility.IsIntegralType(RightChild.ResultType);
        var emitOverflow = integral & options.OverflowChecked;

        EmitChildWithConvert(LeftChild, ResultType, ilGenerator, context);

        if (IsOptimizablePower is false)
        {
            EmitChildWithConvert(RightChild, ResultType, ilGenerator, context);
        }

        switch (operation)
        {
            case BinaryArithmeticOperation.Add:
                if (emitOverflow)
                {
                    ilGenerator.Emit(unsigned ? OpCodes.Add_Ovf_Un : OpCodes.Add_Ovf);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Add);
                }
                break;

            case BinaryArithmeticOperation.Subtract:
                if (emitOverflow)
                {
                    ilGenerator.Emit(unsigned ? OpCodes.Sub_Ovf_Un : OpCodes.Sub_Ovf);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Sub);
                }
                break;

            case BinaryArithmeticOperation.Multiply:
                EmitMultiply(ilGenerator, emitOverflow, unsigned);
                break;

            case BinaryArithmeticOperation.Divide:
                ilGenerator.Emit(unsigned ? OpCodes.Div_Un : OpCodes.Div);
                break;

            case BinaryArithmeticOperation.Mod:
                ilGenerator.Emit(unsigned ? OpCodes.Rem_Un : OpCodes.Rem);
                break;

            case BinaryArithmeticOperation.Power:
                EmitPower(ilGenerator, emitOverflow, unsigned);
                break;

            default:
                Debug.Fail("Unknown op type");
                break;
        }
    }

    private void EmitPower(YaleIlGenerator ilGenerator, bool emitOverflow, bool unsigned)
    {
        if (IsOptimizablePower)
        {
            EmitOptimizedPower(ilGenerator, emitOverflow, unsigned);
        }
        else
        {
            ilGenerator.Emit(OpCodes.Call, powerMethodInfo);
        }
    }

    private void EmitOptimizedPower(YaleIlGenerator ilGenerator, bool emitOverflow, bool unsigned)
    {
        Int32LiteralElement right = (Int32LiteralElement)RightChild;

        if (right.Value == 0)
        {
            ilGenerator.Emit(OpCodes.Pop);
            LiteralElement.EmitLoad(1, ilGenerator);
            ImplicitConverter.EmitImplicitNumericConvert(
                typeof(int),
                LeftChild.ResultType,
                ilGenerator
            );
            return;
        }

        if (right.Value == 1)
        {
            return;
        }

        // Start at 1 since left operand has already been emited once
        for (var i = 1; i <= right.Value - 1; i++)
        {
            ilGenerator.Emit(OpCodes.Dup);
        }

        for (var i = 1; i <= right.Value - 1; i++)
        {
            EmitMultiply(ilGenerator, emitOverflow, unsigned);
        }
    }

    private static void EmitMultiply(YaleIlGenerator ilGenerator, bool emitOverflow, bool unsigned)
    {
        if (emitOverflow)
        {
            ilGenerator.Emit(unsigned ? OpCodes.Mul_Ovf_Un : OpCodes.Mul_Ovf);
        }
        else
        {
            ilGenerator.Emit(OpCodes.Mul);
        }
    }

    /// <summary>
    /// Emit a string concatenation
    /// </summary>
    /// <param name="ilGenerator"></param>
    /// <param name="context"></param>
    private void EmitStringConcat(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        Type argType;
        MethodInfo concatMethodInfo;

        // Pick the most specific concat method
        if (AreBothChildrenOfType(typeof(string)))
        {
            concatMethodInfo = stringConcatMethodInfo;
            argType = typeof(string);
        }
        else
        {
            Debug.Assert(IsEitherChildOfType(typeof(string)), "one child must be a string");
            concatMethodInfo = objectConcatMethodInfo;
            argType = typeof(object);
        }

        // Emit the operands and call the function
        LeftChild.Emit(ilGenerator, context);
        ImplicitConverter.EmitImplicitConvert(LeftChild.ResultType, argType, ilGenerator);
        RightChild.Emit(ilGenerator, context);
        ImplicitConverter.EmitImplicitConvert(RightChild.ResultType, argType, ilGenerator);
        ilGenerator.Emit(OpCodes.Call, concatMethodInfo);
    }

    private bool IsOptimizablePower
    {
        get
        {
            if (
                operation != BinaryArithmeticOperation.Power
                || RightChild is Int32LiteralElement == false
            )
            {
                return false;
            }

            return ((Int32LiteralElement)RightChild)?.Value >= 0;
        }
    }
}

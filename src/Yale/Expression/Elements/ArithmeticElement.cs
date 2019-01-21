using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Base.Literals;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements
{
    internal class ArithmeticElement : BinaryExpressionElement
    {
        private static MethodInfo _powerMethodInfo;
        private static MethodInfo _stringConcatMethodInfo;
        private static MethodInfo _objectConcatMethodInfo;

        private BinaryArithmeticOperation _operation;

        public ArithmeticElement()
        {
            _powerMethodInfo = typeof(Math).GetMethod("Pow", BindingFlags.Public | BindingFlags.Static);
            _stringConcatMethodInfo = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }, null);
            _objectConcatMethodInfo = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }, null);
        }

        protected override void GetOperation(object operation)
        {
            _operation = (BinaryArithmeticOperation)operation;
        }

        protected override Type GetResultType(Type leftType, Type rightType)
        {
            var binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
            var overloadedMethod = GetOverloadedArithmeticOperator();

            // Is an overloaded operator defined for our left and right children?
            if (overloadedMethod != null)
            {
                // Yes, so use its return type
                return overloadedMethod.ReturnType;
            }

            if (binaryResultType != null)
            {
                // Operands are primitive types.  Return computed result type unless we are doing a power operation
                if (_operation == BinaryArithmeticOperation.Power)
                {
                    return GetPowerResultType(leftType, rightType, binaryResultType);
                }

                return binaryResultType;
            }

            if (IsEitherChildOfType(typeof(string)) & (_operation == BinaryArithmeticOperation.Add))
            {
                // String concatenation
                return typeof(string);
            }

            // Invalid types
            return null;
        }

        private Type GetPowerResultType(Type leftType, Type rightType, Type binaryResultType)
        {
            if (IsOptimizablePower)
            {
                return leftType;
            }

            return typeof(double);
        }

        /// <summary>
        /// Return an methodInfo based on the type of the left and right child
        /// </summary>
        /// <returns>Overloaded methodInfo or null if no overloaded method is needed</returns>
        private MethodInfo GetOverloadedArithmeticOperator()
        {
            //Get the name of the operator
            var name = GetOverloadedOperatorFunctionName(_operation);
            return GetOverloadedBinaryOperator(name, _operation);
        }

        /// <summary>
        /// Returns the correct name used to generate IL operations
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static string GetOverloadedOperatorFunctionName(BinaryArithmeticOperation operation)
        {
            switch (operation)
            {
                case BinaryArithmeticOperation.Add:
                    return "Addition";

                case BinaryArithmeticOperation.Subtract:
                    return "Subtraction";

                case BinaryArithmeticOperation.Multiply:
                    return "Multiply";

                case BinaryArithmeticOperation.Divide:
                    return "Division";

                case BinaryArithmeticOperation.Mod:
                    return "Modulus";

                case BinaryArithmeticOperation.Power:
                    return "Exponent";

                default:
                    Debug.Assert(false, "unknown operator type");
                    return null;
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var overloadedMethod = GetOverloadedArithmeticOperator();

            if (overloadedMethod != null)
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
                EmitArithmeticOperation(_operation, ilGenerator, context);
            }
        }

        private static bool IsUnsignedForArithmetic(Type type)
        {
            return ReferenceEquals(type, typeof(UInt32)) | ReferenceEquals(type, typeof(UInt64));
        }

        /// <summary>
        /// Emit an arithmetic operation with handling for unsigned and checked contexts
        /// </summary>
        private void EmitArithmeticOperation(BinaryArithmeticOperation operation, YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var options = context.BuilderOptions;
            var unsigned = IsUnsignedForArithmetic(LeftChild.ResultType) & IsUnsignedForArithmetic(RightChild.ResultType);
            var integral = Utility.IsIntegralType(LeftChild.ResultType) & Utility.IsIntegralType(RightChild.ResultType);
            var emitOverflow = integral & options.OverflowChecked;

            EmitChildWithConvert(LeftChild, ResultType, ilGenerator, context);

            if (IsOptimizablePower == false)
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
                ilGenerator.Emit(OpCodes.Call, _powerMethodInfo);
            }
        }

        private void EmitOptimizedPower(YaleIlGenerator ilGenerator, bool emitOverflow, bool unsigned)
        {
            var right = (Int32LiteralElement)RightChild;

            if (right.Value == 0)
            {
                ilGenerator.Emit(OpCodes.Pop);
                IntegralLiteralElement.EmitLoad(1, ilGenerator);
                ImplicitConverter.EmitImplicitNumericConvert(typeof(Int32), LeftChild.ResultType, ilGenerator);
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

        private void EmitMultiply(YaleIlGenerator ilGenerator, bool emitOverflow, bool unsigned)
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
                concatMethodInfo = _stringConcatMethodInfo;
                argType = typeof(string);
            }
            else
            {
                Debug.Assert(IsEitherChildOfType(typeof(string)), "one child must be a string");
                concatMethodInfo = _objectConcatMethodInfo;
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
                if (_operation != BinaryArithmeticOperation.Power || RightChild is Int32LiteralElement == false)
                {
                    return false;
                }

                return ((Int32LiteralElement)RightChild)?.Value >= 0;
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements
{
    internal class CompareElement : BinaryExpressionElement
    {
        private LogicalCompareOperation _operation;

        public void Initialize(ExpressionElement leftChild, ExpressionElement rightChild, LogicalCompareOperation op)
        {
            LeftChild = leftChild;
            RightChild = (Int32LiteralElement)rightChild;
            _operation = op;
        }

        public void Validate()
        {
            ValidateInternal(_operation);
        }

        protected override void GetOperation(object operation)
        {
            _operation = (LogicalCompareOperation)operation;
        }

        protected override Type GetResultType(Type leftType, Type rightType)
        {
            var binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
            var overloadedOperator = GetOverloadedCompareOperator();
            var isEqualityOp = IsOpTypeEqualOrNotEqual(_operation);

            // Use our string equality instead of overloaded operator
            if (ReferenceEquals(leftType, typeof(string)) & ReferenceEquals(rightType, typeof(string)) & isEqualityOp)
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

            if (ReferenceEquals(leftType, typeof(bool)) & ReferenceEquals(rightType, typeof(bool)) & isEqualityOp)
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
            var name = GetCompareOperatorName(_operation);
            return GetOverloadedBinaryOperator(name, _operation);
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
            var binaryResultType = ImplicitConverter.GetBinaryResultType(LeftChild.ResultType, RightChild.ResultType);
            var overloadedOperator = GetOverloadedCompareOperator();

            if (AreBothChildrenOfType(typeof(string)))
            {
                // String equality
                LeftChild.Emit(ilGenerator, context);
                RightChild.Emit(ilGenerator, context);
                EmitStringEquality(ilGenerator, _operation, context);
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
                EmitCompareOperation(ilGenerator, _operation);
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
            EmitCompareOperation(ilg, _operation);
        }

        private static void EmitStringEquality(YaleIlGenerator ilg, LogicalCompareOperation op, ExpressionContext context)
        {
            // Get the StringComparison from the options
            var options = context.BuilderOptions;
            var int32LiteralElement = new Int32LiteralElement((int)options.StringComparison);

            int32LiteralElement.Emit(ilg, context);

            // and emit the method call
            var methodInfo = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string), typeof(StringComparison) }, null);
            ilg.Emit(OpCodes.Call, methodInfo);

            if (op == LogicalCompareOperation.NotEqual)
            {
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ceq);
            }
        }

        private static bool IsOpTypeEqualOrNotEqual(LogicalCompareOperation op)
        {
            return op == LogicalCompareOperation.Equal | op == LogicalCompareOperation.NotEqual;
        }

        private bool AreBothChildrenReferenceTypes()
        {
            return LeftChild.ResultType.IsValueType == false & RightChild.ResultType.IsValueType == false;
        }

        private bool AreBothChildrenSameEnum()
        {
            return LeftChild.ResultType.IsEnum && ReferenceEquals(LeftChild.ResultType, RightChild.ResultType);
        }

        /// <summary>
        /// Emit the actual compare
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="op"></param>
        private void EmitCompareOperation(YaleIlGenerator ilg, LogicalCompareOperation op)
        {
            var ltOpcode = GetCompareGTLTOpcode(false);
            var gtOpcode = GetCompareGTLTOpcode(true);

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
            var leftType = LeftChild.ResultType;

            if (ReferenceEquals(leftType, RightChild.ResultType))
            {
                if (ReferenceEquals(leftType, typeof(UInt32)) | ReferenceEquals(leftType, typeof(UInt64)))
                {
                    return greaterThan ? OpCodes.Cgt_Un : OpCodes.Clt_Un;
                }

                return GetCompareOpcode(greaterThan);
            }

            return GetCompareOpcode(greaterThan);
        }

        private static OpCode GetCompareOpcode(bool greaterThan)
        {
            return greaterThan ? OpCodes.Cgt : OpCodes.Clt;
        }
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.LogicalBitwise
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "<Pending>")]
    internal class NotElement : UnaryElement
    {
        public override Type ResultType { get; }

        public NotElement(ExpressionElement child) : base(child)
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
            var result = Utility.IsIntegralType(childType) ?
                childType :
                throw CompileException(CompileErrors.OperationNotDefinedForType, CompileExceptionReason.TypeMismatch, MyChild.ResultType.Name);

            return result;
        }
    }
}
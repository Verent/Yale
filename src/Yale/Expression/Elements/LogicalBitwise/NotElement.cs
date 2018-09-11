using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Internal;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.LogicalBitwise
{
    internal class NotElement : UnaryElement
    {
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

            if (Utility.IsIntegralType(childType))
            {
                return childType;
            }

            return null;
        }
    }
}
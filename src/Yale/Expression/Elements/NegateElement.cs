using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Internal;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements
{
    internal class NegateElement : UnaryElement
    {
        private const string UnaryNegoation = "UnaryNegation";

        protected override Type GetResultType(Type childType)
        {
            var typeCode = Type.GetTypeCode(childType);

            var methodInfo = Utility.GetSimpleOverloadedOperator(UnaryNegoation, childType, childType);
            if (methodInfo != null)
            {
                return methodInfo.ReturnType;
            }

            switch (typeCode)
            {
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return childType;

                case TypeCode.UInt32:
                    return typeof(Int64);

                default:
                    return null;
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var resultType = ResultType;
            MyChild.Emit(ilGenerator, context);
            ImplicitConverter.EmitImplicitConvert(MyChild.ResultType, resultType, ilGenerator);

            var methodInfo = Utility.GetSimpleOverloadedOperator(UnaryNegoation, resultType, resultType);

            if (methodInfo == null)
            {
                ilGenerator.Emit(OpCodes.Neg);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Call, methodInfo);
            }
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements;

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "<Pending>"
)]
internal sealed class NegateElement : UnaryElement
{
    private const string UnaryNegation = nameof(UnaryNegation);

    public override Type ResultType { get; }

    public NegateElement(BaseExpressionElement child)
        : base(child)
    {
        ResultType = GetResultType(child.ResultType);
    }

    protected override Type GetResultType(Type childType)
    {
        TypeCode typeCode = Type.GetTypeCode(childType);

        System.Reflection.MethodInfo methodInfo = Utility.GetSimpleOverloadedOperator(
            UnaryNegation,
            childType,
            childType
        );
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
                throw CreateCompileException(
                    CompileErrors.OperationNotDefinedForType,
                    CompileExceptionReason.TypeMismatch,
                    MyChild.ResultType.Name
                );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        Type resultType = ResultType;
        MyChild.Emit(ilGenerator, context);
        ImplicitConverter.EmitImplicitConvert(MyChild.ResultType, resultType, ilGenerator);

        System.Reflection.MethodInfo methodInfo = Utility.GetSimpleOverloadedOperator(
            UnaryNegation,
            resultType,
            resultType
        );

        if (methodInfo is null)
        {
            ilGenerator.Emit(OpCodes.Neg);
        }
        else
        {
            ilGenerator.Emit(OpCodes.Call, methodInfo);
        }
    }
}

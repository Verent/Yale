using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements;

internal sealed class RootExpressionElement : BaseExpressionElement
{
    private readonly BaseExpressionElement _child;
    private readonly Type _resultType;

    public RootExpressionElement(BaseExpressionElement child, Type resultType)
    {
        _child = child;
        _resultType = resultType;
        Validate();
    }

    //Entry point of IL Creation
    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        _child.Emit(ilGenerator, context);

        ImplicitConverter.EmitImplicitConvert(_child.ResultType, _resultType, ilGenerator);

        //Todo: Verify if this convert stuff works
        if ("isGeneric".Equals("false", StringComparison.Ordinal))
        {
            ImplicitConverter.EmitImplicitConvert(_resultType, typeof(object), ilGenerator);
        }

        ilGenerator.Emit(OpCodes.Ret);
    }

    private void Validate()
    {
        if (ImplicitConverter.EmitImplicitConvert(_child.ResultType, _resultType, null) == false)
        {
            throw CreateCompileException(
                CompileErrors.CannotConvertTypeToExpressionResult,
                CompileExceptionReason.TypeMismatch,
                _child.ResultType.Name,
                _resultType.Name
            );
        }
    }

    public override Type ResultType => typeof(object);
}

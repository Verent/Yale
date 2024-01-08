using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.MemberElements;

/// <summary>
/// Element representing an array index
/// </summary>
internal sealed class IndexerElement : MemberElement
{
    private BaseExpressionElement _indexerElement;

    private readonly ArgumentList _indexerElements;

    private bool IsArray => Previous.TargetType.IsArray;

    private Type ArrayType => IsArray ? Previous.TargetType : null;

    protected override bool RequiresAddress => IsArray == false;

    public override Type ResultType =>
        IsArray ? ArrayType.GetElementType() : _indexerElement.ResultType;

    protected override bool IsPublic => IsArray || IsElementPublic((MemberElement)_indexerElement);

    public override bool IsStatic => false;

    public IndexerElement(ArgumentList indexer)
    {
        _indexerElements = indexer;
    }

    protected override void ResolveInternal()
    {
        Type target = Previous.TargetType;

        // Are we are indexing on an array?
        if (target.IsArray)
        {
            // Yes, so setup for an array index
            SetupArrayIndexer();
            return;
        }

        // Not an array, so try to find an indexer on the type
        if (FindIndexer(target) == false)
        {
            throw CreateCompileException(
                CompileErrors.TypeNotArrayAndHasNoIndexerOfType,
                CompileExceptionReason.TypeMismatch,
                target.Name,
                _indexerElements
            );
        }
    }

    private void SetupArrayIndexer()
    {
        _indexerElement = _indexerElements[0];

        if (_indexerElements.Count > 1)
        {
            throw CreateCompileException(
                CompileErrors.MultiArrayIndexNotSupported,
                CompileExceptionReason.TypeMismatch
            );
        }
        else if (
            ImplicitConverter.EmitImplicitConvert(_indexerElement.ResultType, typeof(Int32), null)
            == false
        )
        {
            throw CreateCompileException(
                CompileErrors.ArrayIndexersMustBeOfType,
                CompileExceptionReason.TypeMismatch,
                typeof(Int32).Name
            );
        }
    }

    private bool FindIndexer(Type targetType)
    {
        // Get the default members
        MemberInfo[] members = targetType.GetDefaultMembers();

        List<MethodInfo> methods = new();

        // Use the first one that's valid for our indexer type
        foreach (MemberInfo? memberInfo in members)
        {
            PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                methods.Add(propertyInfo.GetGetMethod(true));
            }
        }

        FunctionCallElement functionCallElement =
            new("Indexer", methods.ToArray(), _indexerElements);
        functionCallElement.Resolve(Context);
        _indexerElement = functionCallElement;

        return true;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        base.Emit(ilGenerator, context);

        if (IsArray)
        {
            EmitArrayLoad(ilGenerator, context);
        }
        else
        {
            EmitIndexer(ilGenerator, context);
        }
    }

    private void EmitArrayLoad(YaleIlGenerator ilg, ExpressionContext context)
    {
        _indexerElement.Emit(ilg, context);
        ImplicitConverter.EmitImplicitConvert(_indexerElement.ResultType, typeof(Int32), ilg);

        Type elementType = ResultType;

        if (elementType.IsValueType == false)
        {
            // Simple reference load
            ilg.Emit(OpCodes.Ldelem_Ref);
        }
        else
        {
            EmitValueTypeArrayLoad(ilg, elementType);
        }
    }

    private void EmitValueTypeArrayLoad(YaleIlGenerator ilg, Type elementType)
    {
        if (NextRequiresAddress)
        {
            ilg.Emit(OpCodes.Ldelema, elementType);
        }
        else
        {
            Utility.EmitArrayLoad(ilg, elementType);
        }
    }

    private void EmitIndexer(YaleIlGenerator ilg, ExpressionContext context)
    {
        FunctionCallElement functionCallElement = (FunctionCallElement)_indexerElement;
        functionCallElement.EmitFunctionCall(NextRequiresAddress, ilg, context);
    }
}

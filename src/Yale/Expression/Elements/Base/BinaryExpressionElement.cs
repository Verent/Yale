using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Base;

/// <summary>
/// "Base class for expression elements that operate on two child elements"
/// </summary>
internal abstract class BinaryExpressionElement : BaseExpressionElement
{
    protected BaseExpressionElement LeftChild;
    protected BaseExpressionElement RightChild;
    private Type? resultType;

    /// <summary>
    /// Converts a list of binary elements into a binary tree
    /// </summary>
    /// <param name="childValues"></param>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public static BinaryExpressionElement CreateElement(IList childValues, Type elementType)
    {
        BinaryExpressionElement firstElement = (BinaryExpressionElement)
            Activator.CreateInstance(elementType);
        firstElement.Configure(
            (BaseExpressionElement)childValues[0],
            (BaseExpressionElement)childValues[2],
            childValues[1]
        );

        BinaryExpressionElement lastElement = firstElement;

        for (int i = 3; i <= childValues.Count - 1; i += 2)
        {
            BinaryExpressionElement element = (BinaryExpressionElement)
                Activator.CreateInstance(elementType);
            element.Configure(
                lastElement,
                (BaseExpressionElement)childValues[i + 1],
                childValues[i]
            );
            lastElement = element;
        }

        return lastElement;
    }

    protected abstract void GetOperation(object operation);

    protected void ValidateInternal(object op)
    {
        resultType = GetResultType(LeftChild.ResultType, RightChild.ResultType);

        if (resultType is null)
        {
            ThrowOperandTypeMismatch(op, LeftChild.ResultType, RightChild.ResultType);
        }
    }

    protected MethodInfo? GetOverloadedBinaryOperator(string name, object operation)
    {
        Type leftType = LeftChild.ResultType;
        Type rightType = RightChild.ResultType;
        BinaryOperatorBinder binder = new(leftType, rightType);

        // If both arguments are of the same type, pick either as the owner type
        if (ReferenceEquals(leftType, rightType))
        {
            return Utility.GetOverloadedOperator(name, leftType, binder, leftType, rightType);
        }

        // Get the operator for both types
        MethodInfo? leftMethod = Utility.GetOverloadedOperator(
            name,
            leftType,
            binder,
            leftType,
            rightType
        );
        MethodInfo? rightMethod = Utility.GetOverloadedOperator(
            name,
            rightType,
            binder,
            leftType,
            rightType
        );

        // Pick the right one
        if (leftMethod is null & rightMethod is null)
        {
            // No operator defined for either
            return null;
        }

        if (leftMethod is null)
        {
            return rightMethod;
        }

        if (rightMethod is null)
        {
            return leftMethod;
        }

        //Ambiguous call
        throw CreateCompileException(
            CompileErrors.AmbiguousOverloadedOperator,
            CompileExceptionReason.AmbiguousMatch,
            leftType.Name,
            rightType.Name,
            operation
        );
    }

    protected void EmitOverloadedOperatorCall(
        MethodInfo method,
        YaleIlGenerator ilg,
        ExpressionContext context
    )
    {
        ParameterInfo[] parameters = method.GetParameters();
        ParameterInfo parameterInfoLeft = parameters[0];
        ParameterInfo parameterInfoRight = parameters[1];

        EmitChildWithConvert(LeftChild, parameterInfoLeft.ParameterType, ilg, context);
        EmitChildWithConvert(RightChild, parameterInfoRight.ParameterType, ilg, context);
        ilg.Emit(OpCodes.Call, method);
    }

    protected void ThrowOperandTypeMismatch(object operation, Type leftType, Type rightType) =>
        throw CreateCompileException(
            CompileErrors.OperationNotDefinedForTypes,
            CompileExceptionReason.TypeMismatch,
            operation,
            leftType.Name,
            rightType.Name
        );

    protected abstract Type? GetResultType(Type leftType, Type rightType);

    protected static void EmitChildWithConvert(
        BaseExpressionElement child,
        Type resultType,
        YaleIlGenerator ilg,
        ExpressionContext context
    )
    {
        child.Emit(ilg, context);
        bool converted = ImplicitConverter.EmitImplicitConvert(child.ResultType, resultType, ilg);
        Debug.Assert(converted, "convert failed");
    }

    protected bool AreBothChildrenOfType(Type target) =>
        IsChildOfType(LeftChild, target) & IsChildOfType(RightChild, target);

    protected bool IsEitherChildOfType(Type target) =>
        IsChildOfType(LeftChild, target) || IsChildOfType(RightChild, target);

    protected static bool IsChildOfType(BaseExpressionElement child, Type t) =>
        ReferenceEquals(child.ResultType, t);

    /// <summary>
    /// Set the left and right operands, get the operation, and get the result type
    /// </summary>
    /// <param name="leftChild"></param>
    /// <param name="rightChild"></param>
    /// <param name="op"></param>
    private void Configure(
        BaseExpressionElement leftChild,
        BaseExpressionElement rightChild,
        object op
    )
    {
        LeftChild = leftChild;
        RightChild = rightChild;
        GetOperation(op);

        ValidateInternal(op);
    }

    public sealed override Type ResultType => resultType;
}

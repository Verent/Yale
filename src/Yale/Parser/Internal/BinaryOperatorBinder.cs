using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal;

internal class BinaryOperatorBinder : CustomBinder
{
    private readonly Type leftType;
    private readonly Type rightType;

    public BinaryOperatorBinder(Type leftType, Type rightType)
    {
        this.leftType = leftType;
        this.rightType = rightType;
    }

    public override MethodBase BindToMethod(
        BindingFlags bindingAttr,
        MethodBase[] match,
        ref object?[] args,
        ParameterModifier[]? modifiers,
        CultureInfo? culture,
        string[]? names,
        out object? state
    ) => throw new NotImplementedException();

    public override object ChangeType(object value, Type type, CultureInfo? culture) =>
        throw new NotImplementedException();

    public override MethodBase? SelectMethod(
        BindingFlags bindingAttr,
        MethodBase[] match,
        Type[] types,
        ParameterModifier[]? modifiers
    )
    {
        foreach (var methodBase in match)
        {
            var parameters = methodBase.GetParameters();
            var leftValid = ImplicitConverter.EmitImplicitConvert(
                leftType,
                parameters[0].ParameterType,
                null
            );
            var rightValid = ImplicitConverter.EmitImplicitConvert(
                rightType,
                parameters[1].ParameterType,
                null
            );

            if (leftValid & rightValid)
            {
                return methodBase;
            }
        }
        return null;
    }
}

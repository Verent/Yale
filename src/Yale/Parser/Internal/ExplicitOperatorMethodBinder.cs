using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal;

internal class ExplicitOperatorMethodBinder : CustomBinder
{
    private readonly Type returnType;
    private readonly Type argType;

    public ExplicitOperatorMethodBinder(Type returnType, Type argType)
    {
        this.returnType = returnType;
        this.argType = argType;
    }

    public override MethodBase BindToMethod(
        BindingFlags bindingAttr,
        MethodBase[] match,
        ref object[] args,
        ParameterModifier[] modifiers,
        CultureInfo culture,
        string[] names,
        out object state
    ) => throw new NotImplementedException();

    public override object ChangeType(object value, Type type, CultureInfo? culture) =>
        throw new NotImplementedException();

    public override MethodBase? SelectMethod(
        BindingFlags bindingAttr,
        MethodBase[] match,
        Type[] types,
        ParameterModifier[] modifiers
    )
    {
        foreach (MethodInfo methodInfo in match)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            ParameterInfo firstParameter = parameters[0];
            if (
                ReferenceEquals(firstParameter.ParameterType, argType)
                & object.ReferenceEquals(methodInfo.ReturnType, returnType)
            )
            {
                return methodInfo;
            }
        }
        return null;
    }
}

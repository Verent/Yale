using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal;

internal abstract class CustomBinder : Binder
{
    public override FieldInfo BindToField(
        BindingFlags bindingAttr,
        FieldInfo[] match,
        object value,
        CultureInfo? culture
    ) => null;

    public override void ReorderArgumentArray(ref object?[] args, object state) { }

    public override PropertyInfo? SelectProperty(
        BindingFlags bindingAttr,
        PropertyInfo[] match,
        Type? returnType,
        Type[]? indexes,
        ParameterModifier[]? modifiers
    ) => null;
}

using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Internal
{
    internal abstract class CustomBinder : Binder
    {
        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
        {
            return null;
        }

        public MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, ref object state)
        {
            return null;
        }

        public override object ChangeType(object value, Type type, CultureInfo culture)
        {
            return null;
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            return null;
        }
    }
}
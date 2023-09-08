using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal
{
    internal class BinaryOperatorBinder : CustomBinder
    {
        private readonly Type leftType;
        private readonly Type rightType;
        private readonly CustomBinder customBinderImplementation;

        public BinaryOperatorBinder(Type leftType, Type rightType)
        {
            this.leftType = leftType;
            this.rightType = rightType;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers,
            CultureInfo culture, string[] names, out object state)
        {
            return customBinderImplementation.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (MethodBase methodBase in match)
            {
                ParameterInfo[] parameters = methodBase.GetParameters();
                bool leftValid = ImplicitConverter.EmitImplicitConvert(leftType, parameters[0].ParameterType, null);
                bool rightValid = ImplicitConverter.EmitImplicitConvert(rightType, parameters[1].ParameterType, null);

                if (leftValid & rightValid)
                {
                    return methodBase;
                }
            }
            return null;
        }
    }
}
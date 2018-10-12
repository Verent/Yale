using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal
{
    internal class BinaryOperatorBinder : CustomBinder
    {
        private readonly Type _leftType;
        private readonly Type _rightType;
        private CustomBinder _customBinderImplementation;

        public BinaryOperatorBinder(Type leftType, Type rightType)
        {
            _leftType = leftType;
            _rightType = rightType;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers,
            CultureInfo culture, string[] names, out object state)
        {
            return _customBinderImplementation.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (var methodBase in match)
            {
                var parameters = methodBase.GetParameters();
                var leftValid = ImplicitConverter.EmitImplicitConvert(_leftType, parameters[0].ParameterType, null);
                var rightValid = ImplicitConverter.EmitImplicitConvert(_rightType, parameters[1].ParameterType, null);

                if (leftValid & rightValid)
                {
                    return methodBase;
                }
            }
            return null;
        }
    }
}
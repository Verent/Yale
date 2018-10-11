using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal
{
    internal class BinaryOperatorBinder : CustomBinder
    {
        private readonly Type _myLeftType;
        private readonly Type _myRightType;
        private CustomBinder _customBinderImplementation;

        public BinaryOperatorBinder(Type leftType, Type rightType)
        {
            _myLeftType = leftType;
            _myRightType = rightType;
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
                var leftValid = ImplicitConverter.EmitImplicitConvert(_myLeftType, parameters[0].ParameterType, null);
                var rightValid = ImplicitConverter.EmitImplicitConvert(_myRightType, parameters[1].ParameterType, null);

                if (leftValid & rightValid)
                {
                    return methodBase;
                }
            }
            return null;
        }
    }
}
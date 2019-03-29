using System;
using System.Globalization;
using System.Reflection;

namespace Yale.Parser.Internal
{
    internal class ExplicitOperatorMethodBinder : CustomBinder
    {
        private readonly Type _returnType;
        private readonly Type _argType;
        private readonly CustomBinder _customBinderImplementation;

        public ExplicitOperatorMethodBinder(Type returnType, Type argType)
        {
            _returnType = returnType;
            _argType = argType;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers,
            CultureInfo culture, string[] names, out object state)
        {
            return _customBinderImplementation.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            foreach (MethodInfo methodInfo in match)
            {
                var parameters = methodInfo.GetParameters();
                var firstParameter = parameters[0];
                if (ReferenceEquals(firstParameter.ParameterType, _argType) & object.ReferenceEquals(methodInfo.ReturnType, _returnType))
                {
                    return methodInfo;
                }
            }
            return null;
        }
    }
}
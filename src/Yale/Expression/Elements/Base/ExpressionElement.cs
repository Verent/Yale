using System;
using System.Diagnostics;
using System.Reflection.Emit;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Base
{
    internal abstract class ExpressionElement
    {
        /// <summary>
        /// All expression elements must be able to emit their own Intermediate language
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="context"></param>
        public abstract void Emit(YaleIlGenerator ilGenerator, ExpressionContext context);

        /// <summary>
        /// All expression elements must expose the Type they evaluate to
        /// </summary>
        public abstract Type ResultType { get; }

        public override string ToString()
        {
            return Name;
        }

        protected void ThrowCompileException(string messageKey, CompileExceptionReason reason, params object[] arguments)
        {
            var messageTemplate = FleeResourceManager.Instance.GetCompileErrorString(messageKey);
            var message = string.Format(messageTemplate, arguments);
            message = string.Concat(Name, ": ", message);
            throw new ExpressionCompileException(message, reason);
        }

        protected void ThrowAmbiguousCallException(Type leftType, Type rightType, object operation)
        {
            ThrowCompileException(CompileErrorResourceKeys.AmbiguousOverloadedOperator, CompileExceptionReason.AmbiguousMatch, leftType.Name, rightType.Name, operation);
        }

        protected YaleIlGenerator CreateTempIlGenerator(YaleIlGenerator ilgCurrent)
        {
            var dynamicMethod = new DynamicMethod("temp", typeof(int), null, GetType());
            return new YaleIlGenerator(dynamicMethod.GetILGenerator(), ilgCurrent.Length, true);
        }

        protected string Name
        {
            get
            {
                var key = GetType().Name;
                var value = FleeResourceManager.Instance.GetElementNameString(key);
                Debug.Assert(value != null, $"Element name for '{key}' not in resource file");
                return value;
            }
            set => throw new InvalidOperationException("Can not set name of ExpressionElement.");
        }
    }
}
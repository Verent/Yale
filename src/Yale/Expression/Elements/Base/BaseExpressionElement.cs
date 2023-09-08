using System;
using System.Globalization;
using System.Reflection.Emit;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Base
{
    internal abstract class BaseExpressionElement
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

        public override string ToString() => Name;

        protected string Name => GetType().Name;

        protected ExpressionCompileException CreateCompileException(string messageTemplate, CompileExceptionReason reason, params object[] arguments)
        {
            string message = string.Format(CultureInfo.InvariantCulture, messageTemplate, arguments);
            message = string.Concat(Name, ": ", message);
            return new ExpressionCompileException(message, reason);
        }

        protected YaleIlGenerator CreateTempIlGenerator(YaleIlGenerator ilgCurrent)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("temp", typeof(int), null, GetType());
            return new YaleIlGenerator(dynamicMethod.GetILGenerator(), ilgCurrent.Length, true);
        }
    }
}
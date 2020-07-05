using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using Yale.Parser.Internal;
using Yale.Resources;

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

        public override string ToString()
        {
            return Name;
        }

        protected ExpressionCompileException CreateCompileException(string messageTemplate, CompileExceptionReason reason, params object[] arguments)
        {
            var message = string.Format(CultureInfo.InvariantCulture, messageTemplate, arguments);
            message = string.Concat(Name, ": ", message);
            return new ExpressionCompileException(message, reason);
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
                var value = ElementResourceManager.GetElementNameString(key);
                Debug.Assert(value != null, $"Element name for '{key}' not in resource file");
                return value;
            }
        }
    }
}
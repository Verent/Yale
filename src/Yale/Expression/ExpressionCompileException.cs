using PerCederberg.Grammatica.Runtime;
using System;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression
{
    /// The exception thrown when an expression cannot be compiled.
    public sealed class ExpressionCompileException : Exception
    {
        internal ExpressionCompileException(string message, CompileExceptionReason reason) : base(message)
        {
            Reason = reason;
        }

        internal ExpressionCompileException(ParserLogException parseException) : base(string.Empty, parseException)
        {
            Reason = CompileExceptionReason.SyntaxError;
        }

        private ExpressionCompileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            Reason = (CompileExceptionReason)info.GetInt32("Reason");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Reason", Convert.ToInt32(Reason));
        }

        public override string Message
        {
            get
            {
                if (Reason == CompileExceptionReason.SyntaxError)
                {
                    return $"{CompileErrors.SyntaxError}: {InnerException?.Message}"; ;
                }

                return base.Message;
            }
        }

        /// <summary>
        /// Explains the reason why compilation failed.
        /// </summary>
        public CompileExceptionReason Reason { get; }
    }
}
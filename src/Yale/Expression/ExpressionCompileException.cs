﻿using System.Runtime.Serialization;
using Yale.Parser;
using Yale.Resources;

namespace Yale.Expression;

public sealed class ExpressionCompileException : Exception
{
    internal ExpressionCompileException(string message, CompileExceptionReason reason)
        : base(message) => Reason = reason;

    internal ExpressionCompileException(ParserLogException parseException)
        : base(string.Empty, parseException) => Reason = CompileExceptionReason.SyntaxError;

    private ExpressionCompileException(SerializationInfo info, StreamingContext context)
        : base(info, context) => Reason = (CompileExceptionReason)info.GetInt32("Reason");

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("Reason", (int)Reason);
    }

    public override string Message
    {
        get
        {
            if (Reason is CompileExceptionReason.SyntaxError)
            {
                return $"{CompileErrors.SyntaxError}: {InnerException?.Message}";
            }

            return base.Message;
        }
    }

    /// <summary>
    /// Explains the reason why compilation failed.
    /// </summary>
    public CompileExceptionReason Reason { get; }
}

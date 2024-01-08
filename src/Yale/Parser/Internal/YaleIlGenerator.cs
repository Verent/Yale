using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Yale.Parser.Internal;

internal sealed class YaleIlGenerator
{
    private readonly ILGenerator _ilGenerator;
    private readonly Dictionary<Type, LocalBuilder> _localBuilderTemp;

    public YaleIlGenerator(ILGenerator ilg, int startLength = 0, bool isTemp = false)
    {
        _ilGenerator = ilg;
        _localBuilderTemp = new Dictionary<Type, LocalBuilder>();
        IsTemp = isTemp;
        Length = startLength;
    }

    public int GetTempLocalIndex(Type localType)
    {
        if (_localBuilderTemp.TryGetValue(localType, out LocalBuilder? local) == false)
        {
            local = _ilGenerator.DeclareLocal(localType);
            _localBuilderTemp.Add(localType, local);
        }

        return local.LocalIndex;
    }

    public void Emit(OpCode op)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op);
    }

    public void Emit(OpCode op, Type arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, ConstructorInfo arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, MethodInfo arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, FieldInfo arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, byte arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, sbyte arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, short arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, int arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, long arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, float arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, double arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, string arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void Emit(OpCode op, Label arg)
    {
        RecordOpcode(op);
        _ilGenerator.Emit(op, arg);
    }

    public void MarkLabel(Label lbl) => _ilGenerator.MarkLabel(lbl);

    public Label DefineLabel()
    {
        LabelCount += 1;
        return _ilGenerator.DefineLabel();
    }

    public LocalBuilder DeclareLocal(Type localType) => _ilGenerator.DeclareLocal(localType);

    private void RecordOpcode(OpCode op)
    {
        int operandLength = GetOpcodeOperandSize(op.OperandType);
        Length += op.Size + operandLength;
    }

    private static int GetOpcodeOperandSize(OperandType operand)
    {
        switch (operand)
        {
            case OperandType.InlineNone:
                return 0;

            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
                return 1;

            case OperandType.InlineVar:
                return 2;

            case OperandType.InlineBrTarget:
            case OperandType.InlineField:
            case OperandType.InlineI:
            case OperandType.InlineMethod:
            case OperandType.InlineSig:
            case OperandType.InlineString:
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.ShortInlineR:
                return 4;

            case OperandType.InlineI8:
            case OperandType.InlineR:
                return 8;

            default:
                Debug.Fail("Unknown operand type");
                break;
        }
        return 0;
    }

    [Conditional("DEBUG")]
    public void ValidateLength()
    {
        int ilGen = IlGeneratorLength;
        Debug.Assert(Length == ilGen || ilGen == -1, "YaleIlGenerator length mismatch");
    }

    public int Length { get; private set; }

    public int LabelCount { get; private set; }

    private int IlGeneratorLength => Utility.GetIlGeneratorLength(_ilGenerator);

    public bool IsTemp { get; }
}

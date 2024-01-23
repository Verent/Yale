using System.Diagnostics;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Base.Literals;

internal abstract class LiteralElement : BaseExpressionElement
{
    protected void OnParseOverflow(string image)
    {
        throw CreateCompileException(
            CompileErrors.ValueNotRepresentableInType,
            CompileExceptionReason.ConstantOverflow,
            image,
            ResultType.Name
        );
    }

    public static void EmitLoad(int value, YaleIlGenerator ilg)
    {
        if (value >= -1 & value <= 8)
        {
            EmitSuperShort(value, ilg);
        }
        else if (value >= sbyte.MinValue & value <= sbyte.MaxValue)
        {
            ilg.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(value));
        }
        else
        {
            ilg.Emit(OpCodes.Ldc_I4, value);
        }
    }

    protected static void EmitLoad(long value, YaleIlGenerator ilg)
    {
        if (value >= int.MinValue & value <= int.MaxValue)
        {
            EmitLoad(Convert.ToInt32(value), ilg);
            ilg.Emit(OpCodes.Conv_I8);
        }
        else if (value >= 0 & value <= int.MaxValue)
        {
            EmitLoad(Convert.ToInt32(value), ilg);
            ilg.Emit(OpCodes.Conv_U8);
        }
        else
        {
            ilg.Emit(OpCodes.Ldc_I8, value);
        }
    }

    protected static void EmitLoad(bool value, YaleIlGenerator ilGenerator)
    {
        ilGenerator.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
    }

    private static void EmitSuperShort(int value, YaleIlGenerator ilGenerator)
    {
        OpCode ldcOpcode = default;

        switch (value)
        {
            case 0:
                ldcOpcode = OpCodes.Ldc_I4_0;
                break;

            case 1:
                ldcOpcode = OpCodes.Ldc_I4_1;
                break;

            case 2:
                ldcOpcode = OpCodes.Ldc_I4_2;
                break;

            case 3:
                ldcOpcode = OpCodes.Ldc_I4_3;
                break;

            case 4:
                ldcOpcode = OpCodes.Ldc_I4_4;
                break;

            case 5:
                ldcOpcode = OpCodes.Ldc_I4_5;
                break;

            case 6:
                ldcOpcode = OpCodes.Ldc_I4_6;
                break;

            case 7:
                ldcOpcode = OpCodes.Ldc_I4_7;
                break;

            case 8:
                ldcOpcode = OpCodes.Ldc_I4_8;
                break;

            case -1:
                ldcOpcode = OpCodes.Ldc_I4_M1;
                break;

            default:
                Debug.Assert(false, "value out of range");
                break;
        }

        ilGenerator.Emit(ldcOpcode);
    }
}

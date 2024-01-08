using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements;

internal sealed class ConditionalElement : BaseExpressionElement
{
    private readonly BaseExpressionElement condition;
    private readonly BaseExpressionElement whenTrue;
    private readonly BaseExpressionElement whenFalse;
    private readonly Type resultType;

    public ConditionalElement(
        BaseExpressionElement condition,
        BaseExpressionElement whenTrue,
        BaseExpressionElement whenFalse
    )
    {
        this.condition = condition;
        this.whenTrue = whenTrue;
        this.whenFalse = whenFalse;

        if (!ReferenceEquals(this.condition.ResultType, typeof(bool)))
        {
            throw CreateCompileException(
                CompileErrors.FirstArgNotBoolean,
                CompileExceptionReason.TypeMismatch
            );
        }

        // The result type is the type that is common to the true/false operands
        if (
            ImplicitConverter.EmitImplicitConvert(
                this.whenFalse.ResultType,
                this.whenTrue.ResultType,
                null
            )
        )
        {
            resultType = this.whenTrue.ResultType;
        }
        else if (
            ImplicitConverter.EmitImplicitConvert(
                this.whenTrue.ResultType,
                this.whenFalse.ResultType,
                null
            )
        )
        {
            resultType = this.whenFalse.ResultType;
        }
        else
        {
            throw CreateCompileException(
                CompileErrors.NeitherArgIsConvertibleToTheOther,
                CompileExceptionReason.TypeMismatch,
                this.whenTrue.ResultType.Name,
                this.whenFalse.ResultType.Name
            );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        BranchManager branchManager = new();
        branchManager.GetLabel("falseLabel", ilGenerator);
        branchManager.GetLabel("endLabel", ilGenerator);

        if (ilGenerator.IsTemp)
        {
            // If this is a fake emit, then do a fake emit and return
            EmitConditional(ilGenerator, context, branchManager);
            return;
        }

        YaleIlGenerator ilgTemp = CreateTempIlGenerator(ilGenerator);
        Utility.SyncFleeIlGeneratorLabels(ilGenerator, ilgTemp);

        // Emit fake conditional to get branch target positions
        EmitConditional(ilgTemp, context, branchManager);

        branchManager.ComputeBranches();

        // Emit real conditional now that we have the branch target locations
        EmitConditional(ilGenerator, context, branchManager);
    }

    private void EmitConditional(
        YaleIlGenerator ilg,
        ExpressionContext context,
        BranchManager branchManager
    )
    {
        Label falseLabel = branchManager.FindLabel("falseLabel");
        Label endLabel = branchManager.FindLabel("endLabel");

        // Emit the condition
        condition.Emit(ilg, context);

        // On false go to the false operand
        if (ilg.IsTemp)
        {
            branchManager.AddBranch(ilg, falseLabel);
            ilg.Emit(OpCodes.Brfalse_S, falseLabel);
        }
        else if (branchManager.IsLongBranch(ilg, falseLabel) == false)
        {
            ilg.Emit(OpCodes.Brfalse_S, falseLabel);
        }
        else
        {
            ilg.Emit(OpCodes.Brfalse, falseLabel);
        }

        // Emit the true operand
        whenTrue.Emit(ilg, context);
        ImplicitConverter.EmitImplicitConvert(whenTrue.ResultType, resultType, ilg);

        // Jump to end
        if (ilg.IsTemp)
        {
            branchManager.AddBranch(ilg, endLabel);
            ilg.Emit(OpCodes.Br_S, endLabel);
        }
        else if (branchManager.IsLongBranch(ilg, endLabel) == false)
        {
            ilg.Emit(OpCodes.Br_S, endLabel);
        }
        else
        {
            ilg.Emit(OpCodes.Br, endLabel);
        }

        branchManager.MarkLabel(ilg, falseLabel);
        ilg.MarkLabel(falseLabel);

        // Emit the false operand
        whenFalse.Emit(ilg, context);
        ImplicitConverter.EmitImplicitConvert(whenFalse.ResultType, resultType, ilg);
        // Fall through to end
        branchManager.MarkLabel(ilg, endLabel);
        ilg.MarkLabel(endLabel);
    }

    public override Type ResultType => resultType;
}

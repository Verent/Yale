using System;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Internal;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements
{
    internal class ConditionalElement : ExpressionElement
    {
        private readonly ExpressionElement _condition;
        private readonly ExpressionElement _whenTrue;
        private readonly ExpressionElement _whenFalse;
        private readonly Type _resultType;

        public ConditionalElement(ExpressionElement condition, ExpressionElement whenTrue, ExpressionElement whenFalse)
        {
            _condition = condition;
            _whenTrue = whenTrue;
            _whenFalse = whenFalse;

            if (!ReferenceEquals(_condition.ResultType, typeof(bool)))
            {
                ThrowCompileException(CompileErrorResourceKeys.FirstArgNotBoolean, CompileExceptionReason.TypeMismatch);
            }

            // The result type is the type that is common to the true/false operands
            if (ImplicitConverter.EmitImplicitConvert(_whenFalse.ResultType, _whenTrue.ResultType, null))
            {
                _resultType = _whenTrue.ResultType;
            }
            else if (ImplicitConverter.EmitImplicitConvert(_whenTrue.ResultType, _whenFalse.ResultType, null))
            {
                _resultType = _whenFalse.ResultType;
            }
            else
            {
                ThrowCompileException(CompileErrorResourceKeys.NeitherArgIsConvertibleToTheOther, CompileExceptionReason.TypeMismatch, _whenTrue.ResultType.Name, _whenFalse.ResultType.Name);
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            var branchManager = new BranchManager();
            branchManager.GetLabel("falseLabel", ilGenerator);
            branchManager.GetLabel("endLabel", ilGenerator);

            if (ilGenerator.IsTemp)
            {
                // If this is a fake emit, then do a fake emit and return
                EmitConditional(ilGenerator, context, branchManager);
                return;
            }

            var ilgTemp = CreateTempIlGenerator(ilGenerator);
            Utility.SyncFleeILGeneratorLabels(ilGenerator, ilgTemp);

            // Emit fake conditional to get branch target positions
            EmitConditional(ilgTemp, context, branchManager);

            branchManager.ComputeBranches();

            // Emit real conditional now that we have the branch target locations
            EmitConditional(ilGenerator, context, branchManager);
        }

        private void EmitConditional(YaleIlGenerator ilg, ExpressionContext context, BranchManager branchManager)
        {
            var falseLabel = branchManager.FindLabel("falseLabel");
            var endLabel = branchManager.FindLabel("endLabel");

            // Emit the condition
            _condition.Emit(ilg, context);

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
            _whenTrue.Emit(ilg, context);
            ImplicitConverter.EmitImplicitConvert(_whenTrue.ResultType, _resultType, ilg);

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
            _whenFalse.Emit(ilg, context);
            ImplicitConverter.EmitImplicitConvert(_whenFalse.ResultType, _resultType, ilg);
            // Fall through to end
            branchManager.MarkLabel(ilg, endLabel);
            ilg.MarkLabel(endLabel);
        }

        public override Type ResultType => _resultType;
    }
}
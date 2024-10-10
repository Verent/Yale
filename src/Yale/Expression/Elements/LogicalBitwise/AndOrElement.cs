using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.LogicalBitwise;

internal sealed class AndOrElement : BinaryExpressionElement
{
    private AndOrOperation myOperation;
    private static readonly object OurTrueTerminalKey = new();
    private static readonly object OurFalseTerminalKey = new();
    private static readonly object OurEndLabelKey = new();

    protected override void GetOperation(object operation) =>
        myOperation = (AndOrOperation)operation;

    protected override Type? GetResultType(Type leftType, Type rightType)
    {
        var bitwiseOpType = Utility.GetBitwiseOpType(leftType, rightType);
        if (bitwiseOpType != null)
        {
            return bitwiseOpType;
        }

        if (AreBothChildrenOfType(typeof(bool)))
        {
            return typeof(bool);
        }

        return null;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        var resultType = ResultType;

        if (ReferenceEquals(resultType, typeof(bool)))
        {
            DoEmitLogical(ilGenerator, context);
        }
        else
        {
            LeftChild.Emit(ilGenerator, context);
            ImplicitConverter.EmitImplicitConvert(LeftChild.ResultType, resultType, ilGenerator);
            RightChild.Emit(ilGenerator, context);
            ImplicitConverter.EmitImplicitConvert(RightChild.ResultType, resultType, ilGenerator);
            EmitBitwiseOperation(ilGenerator, myOperation);
        }
    }

    private static void EmitBitwiseOperation(YaleIlGenerator ilg, AndOrOperation op)
    {
        switch (op)
        {
            case AndOrOperation.And:
                ilg.Emit(OpCodes.And);
                break;

            case AndOrOperation.Or:
                ilg.Emit(OpCodes.Or);
                break;

            default:
                throw new InvalidOperationException("Unknown op type");
        }
    }

    private void DoEmitLogical(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        // We have to do a 'fake' emit so we can get the positions of the labels
        ShortCircuitInfo info = new();
        // Create a temporary IL generator
        var ilgTemp = CreateTempIlGenerator(ilGenerator);

        // We have to make sure that the label count for the temp YaleIlGenerator matches our real YaleIlGenerator
        Utility.SyncFleeIlGeneratorLabels(ilGenerator, ilgTemp);
        // Do the fake emit
        EmitLogical(ilgTemp, info, context);

        // Clear everything except the label positions
        info.ClearTempState();
        info.Branches.ComputeBranches();

        Utility.SyncFleeIlGeneratorLabels(ilgTemp, ilGenerator);

        // Do the real emit
        EmitLogical(ilGenerator, info, context);
    }

    /// <summary>
    /// Emit a short-circuited logical operation sequence
    /// The idea: Store all the leaf operands in a stack with the leftmost at the top and rightmost at the bottom.
    /// For each operand, emit it and try to find an end point for when it short-circuits.  This means we go up through
    /// the stack of operators (ignoring siblings) until we find a different operation (then emit a branch to its right operand)
    /// or we reach the root (emit a branch to a true/false).
    /// Repeat the process for all operands and then emit the true/false/last operand end cases.
    /// </summary>
    private void EmitLogical(YaleIlGenerator ilg, ShortCircuitInfo info, ExpressionContext context)
    {
        // We always have an end label
        info.Branches.GetLabel(OurEndLabelKey, ilg);

        // Populate our data structures
        PopulateData(info);

        // Emit the sequence
        EmitLogicalShortCircuit(ilg, info, context);

        // Get the last operand
        BaseExpressionElement terminalOperand = (BaseExpressionElement)info.Operands.Pop();
        // Emit it
        EmitOperand(terminalOperand, info, ilg, context);
        // And jump to the end
        var endLabel = info.Branches.FindLabel(OurEndLabelKey);
        ilg.Emit(OpCodes.Br_S, endLabel);

        // Emit our true/false terminals
        EmitTerminals(info, ilg, endLabel);

        // Mark the end
        ilg.MarkLabel(endLabel);
    }

    /// <summary>
    /// Emit a sequence of and/or expressions with short-circuiting
    /// </summary>
    /// <param name="ilg"></param>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private static void EmitLogicalShortCircuit(
        YaleIlGenerator ilg,
        ShortCircuitInfo info,
        ExpressionContext context
    )
    {
        while (info.Operators.Count != 0)
        {
            // Get the operator
            AndOrElement op = (AndOrElement)info.Operators.Pop();
            // Get the left operand
            BaseExpressionElement leftOperand = (BaseExpressionElement)info.Operands.Pop();

            // Emit the left
            EmitOperand(leftOperand, info, ilg, context);

            // Get the label for the short-circuit case
            var label = GetShortCircuitLabel(op, info, ilg);
            // Emit the branch
            EmitBranch(op, ilg, label, info);
        }
    }

    private static void EmitBranch(
        AndOrElement op,
        YaleIlGenerator ilg,
        Label target,
        ShortCircuitInfo info
    )
    {
        if (ilg.IsTemp)
        {
            info.Branches.AddBranch(ilg, target);

            // Temp mode; just emit a short branch and return
            var shortBranch = GetBranchOpcode(op, false);
            ilg.Emit(shortBranch, target);

            return;
        }

        // Emit the proper branch opcode

        // Determine if it is a long branch
        var longBranch = info.Branches.IsLongBranch(ilg, target);

        // Get the branch opcode
        var brOpcode = GetBranchOpcode(op, longBranch);

        // Emit the branch
        ilg.Emit(brOpcode, target);
    }

    /// <summary>
    /// Emit a short/long branch for an And/Or element
    /// </summary>
    private static OpCode GetBranchOpcode(AndOrElement op, bool longBranch)
    {
        if (op.myOperation == AndOrOperation.And)
        {
            return longBranch ? OpCodes.Brfalse : OpCodes.Brfalse_S;
        }

        return longBranch ? OpCodes.Brtrue : OpCodes.Brtrue_S;
    }

    /// <summary>
    /// Get the label for a short-circuit
    /// </summary>
    private static Label GetShortCircuitLabel(
        AndOrElement current,
        ShortCircuitInfo info,
        YaleIlGenerator ilg
    )
    {
        // We modify the given stacks so we need to clone them
        Stack cloneOperands = (Stack)info.Operands.Clone();
        Stack cloneOperators = (Stack)info.Operators.Clone();

        // Pop all siblings
        current.PopRightChild(cloneOperands, cloneOperators);

        // Go until we run out of operators
        while (cloneOperators.Count > 0)
        {
            // Get the top operator
            AndOrElement top = (AndOrElement)cloneOperators.Pop();

            // Is is a different operation?
            if (top.myOperation != current.myOperation)
            {
                // Yes, so return a label to its right operand
                var nextOperand = cloneOperands.Pop();
                return GetLabel(nextOperand, ilg, info);
            }

            // No, so keep going up the stack
            top.PopRightChild(cloneOperands, cloneOperators);
        }

        // We've reached the end of the stack so return the label for the appropriate true/false terminal
        if (current.myOperation == AndOrOperation.And)
        {
            return GetLabel(OurFalseTerminalKey, ilg, info);
        }

        return GetLabel(OurTrueTerminalKey, ilg, info);
    }

    private void PopRightChild(Stack operands, Stack operators)
    {
        // What kind of child do we have?
        if (RightChild is AndOrElement andOrChild)
        {
            // Another and/or expression so recurse
            andOrChild.Pop(operands, operators);
        }
        else
        {
            // A terminal so pop it off the operands stack
            operands.Pop();
        }
    }

    /// <summary>
    /// Recursively pop operators and operands
    /// </summary>
    /// <param name="operands"></param>
    /// <param name="operators"></param>
    private void Pop(Stack operands, Stack operators)
    {
        operators.Pop();

        if (LeftChild is not AndOrElement andOrChild)
        {
            operands.Pop();
        }
        else
        {
            andOrChild.Pop(operands, operators);
        }

        andOrChild = RightChild as AndOrElement;

        if (andOrChild is null)
        {
            operands.Pop();
        }
        else
        {
            andOrChild.Pop(operands, operators);
        }
    }

    private static void EmitOperand(
        BaseExpressionElement operand,
        ShortCircuitInfo info,
        YaleIlGenerator ilg,
        ExpressionContext context
    )
    {
        // Is this operand the target of a label?
        if (info.Branches.HasLabel(operand))
        {
            // Yes, so mark it
            var leftLabel = info.Branches.FindLabel(operand);
            ilg.MarkLabel(leftLabel);

            // Note the label's position
            MarkBranchTarget(info, leftLabel, ilg);
        }

        // Emit the operand
        operand.Emit(ilg, context);
    }

    /// <summary>
    /// Emit the end cases for a short-circuit
    /// </summary>
    /// <param name="info"></param>
    /// <param name="ilg"></param>
    /// <param name="endLabel"></param>
    private static void EmitTerminals(ShortCircuitInfo info, YaleIlGenerator ilg, Label endLabel)
    {
        // Emit the false case if it was used
        if (info.Branches.HasLabel(OurFalseTerminalKey))
        {
            var falseLabel = info.Branches.FindLabel(OurFalseTerminalKey);

            // Mark the label and note its position
            ilg.MarkLabel(falseLabel);
            MarkBranchTarget(info, falseLabel, ilg);

            ilg.Emit(OpCodes.Ldc_I4_0);

            // If we also have a true terminal, then skip over it
            if (info.Branches.HasLabel(OurTrueTerminalKey))
            {
                ilg.Emit(OpCodes.Br_S, endLabel);
            }
        }

        // Emit the true case if it was used
        if (info.Branches.HasLabel(OurTrueTerminalKey))
        {
            var trueLabel = info.Branches.FindLabel(OurTrueTerminalKey);

            // Mark the label and note its position
            ilg.MarkLabel(trueLabel);
            MarkBranchTarget(info, trueLabel, ilg);

            ilg.Emit(OpCodes.Ldc_I4_1);
        }
    }

    /// <summary>
    /// Note a label's position if we are in mark mode
    /// </summary>
    /// <param name="info"></param>
    /// <param name="target"></param>
    /// <param name="ilg"></param>
    private static void MarkBranchTarget(ShortCircuitInfo info, Label target, YaleIlGenerator ilg)
    {
        if (ilg.IsTemp)
        {
            info.Branches.MarkLabel(ilg, target);
        }
    }

    private static Label GetLabel(object key, YaleIlGenerator ilg, ShortCircuitInfo info) =>
        info.Branches.GetLabel(key, ilg);

    /// <summary>
    /// Visit the nodes of the tree (right then left) and populate some data structures
    /// </summary>
    /// <param name="info"></param>
    private void PopulateData(ShortCircuitInfo info)
    {
        // Is our right child a leaf or another And/Or expression?
        if (RightChild is not AndOrElement andOrChild)
        {
            // Leaf so push it on the stack
            info.Operands.Push(RightChild);
        }
        else
        {
            // Another And/Or expression so recurse
            andOrChild.PopulateData(info);
        }

        // Add ourselves as an operator
        info.Operators.Push(this);

        // Do the same thing for the left child
        andOrChild = LeftChild as AndOrElement;

        if (andOrChild is null)
        {
            info.Operands.Push(LeftChild);
        }
        else
        {
            andOrChild.PopulateData(info);
        }
    }
}

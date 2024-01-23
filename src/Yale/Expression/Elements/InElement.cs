using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements;

internal sealed class InElement : BaseExpressionElement
{
    // Element we will search for
    private readonly BaseExpressionElement operand;

    // Elements we will compare against
    private readonly List<BaseExpressionElement> arguments;

    // Collection to look in
    private readonly BaseExpressionElement targetCollectionElement;

    // Type of the collection
    private Type targetCollectionType;

    // Initialize for searching a list of values
    public InElement(BaseExpressionElement operand, IList listElements)
    {
        this.operand = operand;

        BaseExpressionElement[] elements = new BaseExpressionElement[listElements.Count];
        listElements.CopyTo(elements, 0);

        arguments = new List<BaseExpressionElement>(elements);
        ResolveForListSearch();
    }

    // Initialize for searching a collection
    public InElement(BaseExpressionElement operand, BaseExpressionElement targetCollection)
    {
        this.operand = operand;
        targetCollectionElement = targetCollection;
        ResolveForCollectionSearch();
    }

    private void ResolveForListSearch()
    {
        CompareElement compareElement = new();

        // Validate that our operand is comparable to all elements in the list
        foreach (BaseExpressionElement argumentElement in arguments)
        {
            compareElement.Initialize(operand, argumentElement, LogicalCompareOperation.Equal);
            compareElement.Validate();
        }
    }

    private void ResolveForCollectionSearch()
    {
        // Try to find a collection type
        targetCollectionType = GetTargetCollectionType();

        if (targetCollectionType is null)
        {
            throw CreateCompileException(
                CompileErrors.SearchArgIsNotKnownCollectionType,
                CompileExceptionReason.TypeMismatch,
                targetCollectionElement.ResultType.Name
            );
        }

        // Validate that the operand type is compatible with the collection
        MethodInfo methodInfo = GetCollectionContainsMethod();
        ParameterInfo firstParameter = methodInfo.GetParameters()[0];

        if (
            ImplicitConverter.EmitImplicitConvert(
                operand.ResultType,
                firstParameter.ParameterType,
                null
            ) == false
        )
        {
            throw CreateCompileException(
                CompileErrors.OperandNotConvertibleToCollectionType,
                CompileExceptionReason.TypeMismatch,
                operand.ResultType.Name,
                firstParameter.ParameterType.Name
            );
        }
    }

    private Type GetTargetCollectionType()
    {
        Type collType = targetCollectionElement.ResultType;

        // Try to see if the collection is a generic ICollection or IDictionary
        Type[] interfaces = collType.GetInterfaces();

        foreach (Type? interfaceType in interfaces)
        {
            if (interfaceType.IsGenericType == false)
            {
                continue;
            }

            Type genericTypeDef = interfaceType.GetGenericTypeDefinition();

            if (
                ReferenceEquals(genericTypeDef, typeof(ICollection<>))
                | ReferenceEquals(genericTypeDef, typeof(IDictionary<,>))
            )
            {
                return interfaceType;
            }
        }

        // Try to see if it is a regular IList or IDictionary
        if (typeof(IList<>).IsAssignableFrom(collType))
        {
            return typeof(IList<>);
        }

        if (typeof(IDictionary<,>).IsAssignableFrom(collType))
        {
            return typeof(IDictionary<,>);
        }

        // Not a known collection type
        return null;
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
    {
        if ((targetCollectionType != null))
        {
            EmitCollectionIn(ilGenerator, context);
        }
        else
        {
            BranchManager branchManager = new();
            branchManager.GetLabel("endLabel", ilGenerator);
            branchManager.GetLabel("trueTerminal", ilGenerator);

            // Do a fake emit to get branch positions
            YaleIlGenerator ilgTemp = CreateTempIlGenerator(ilGenerator);
            Utility.SyncFleeIlGeneratorLabels(ilGenerator, ilgTemp);

            EmitListIn(ilgTemp, context, branchManager);

            branchManager.ComputeBranches();

            // Do the real emit
            EmitListIn(ilGenerator, context, branchManager);
        }
    }

    private void EmitCollectionIn(YaleIlGenerator ilg, ExpressionContext context)
    {
        // Get the contains method
        MethodInfo methodInfo = GetCollectionContainsMethod();
        ParameterInfo firstParameter = methodInfo.GetParameters()[0];

        // Load the collection
        targetCollectionElement.Emit(ilg, context);
        // Load the argument
        operand.Emit(ilg, context);
        // Do an implicit convert if necessary
        ImplicitConverter.EmitImplicitConvert(
            operand.ResultType,
            firstParameter.ParameterType,
            ilg
        );
        // Call the contains method
        ilg.Emit(OpCodes.Callvirt, methodInfo);
    }

    private MethodInfo GetCollectionContainsMethod()
    {
        string methodName = "Contains";

        if (
            targetCollectionType.IsGenericType
            && ReferenceEquals(
                targetCollectionType.GetGenericTypeDefinition(),
                typeof(IDictionary<,>)
            )
        )
        {
            methodName = "ContainsKey";
        }

        return targetCollectionType.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );
    }

    private void EmitListIn(
        YaleIlGenerator ilg,
        ExpressionContext context,
        BranchManager branchManager
    )
    {
        CompareElement compareElement = new();
        Label endLabel = branchManager.FindLabel("endLabel");
        Label trueTerminal = branchManager.FindLabel("trueTerminal");

        // Cache the operand since we will be comparing against it a lot
        LocalBuilder lb = ilg.DeclareLocal(operand.ResultType);
        int targetIndex = lb.LocalIndex;

        operand.Emit(ilg, context);
        Utility.EmitStoreLocal(ilg, targetIndex);

        // Wrap our operand in a local shim
        LocalBasedElement targetShim = new(operand, targetIndex);

        // Emit the compares
        foreach (BaseExpressionElement argumentElement in arguments)
        {
            compareElement.Initialize(targetShim, argumentElement, LogicalCompareOperation.Equal);
            compareElement.Emit(ilg, context);

            EmitBranchToTrueTerminal(ilg, trueTerminal, branchManager);
        }

        ilg.Emit(OpCodes.Ldc_I4_0);
        ilg.Emit(OpCodes.Br_S, endLabel);

        branchManager.MarkLabel(ilg, trueTerminal);
        ilg.MarkLabel(trueTerminal);

        ilg.Emit(OpCodes.Ldc_I4_1);

        branchManager.MarkLabel(ilg, endLabel);
        ilg.MarkLabel(endLabel);
    }

    private static void EmitBranchToTrueTerminal(
        YaleIlGenerator ilg,
        Label trueTerminal,
        BranchManager branchManager
    )
    {
        if (ilg.IsTemp)
        {
            branchManager.AddBranch(ilg, trueTerminal);
            ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
        }
        else if (branchManager.IsLongBranch(ilg, trueTerminal) == false)
        {
            ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
        }
        else
        {
            ilg.Emit(OpCodes.Brtrue, trueTerminal);
        }
    }

    public override Type ResultType => typeof(bool);
}

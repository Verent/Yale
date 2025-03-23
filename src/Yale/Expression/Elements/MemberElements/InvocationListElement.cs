using Yale.Core;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.MemberElements;

/// <summary>
/// Todo: What is invocation list element?
/// </summary>
internal sealed class InvocationListElement : BaseExpressionElement
{
    //Todo: Some description would be nice
    private readonly MemberElement tail;

    public InvocationListElement(IList elements, ExpressionContext context)
    {
        HandleFirstElement(elements, context);
        LinkElements(elements);
        Resolve(elements, context);

        //Todo: What is the _tail reference?
        tail = (MemberElement)elements[elements.Count - 1];
    }

    /// <summary>
    /// Arrange elements as a linked list
    /// </summary>
    /// <param name="elements"></param>
    private static void LinkElements(IList elements)
    {
        for (var i = 0; i <= elements.Count - 1; i++)
        {
            var currentElement = (MemberElement)elements[i];
            MemberElement nextElement = null;
            if (i + 1 < elements.Count)
            {
                nextElement = (MemberElement)elements[i + 1];
            }
            currentElement.Link(nextElement);
        }
    }

    private void HandleFirstElement(IList elements, ExpressionContext context)
    {
        var firstElement = (BaseExpressionElement)elements[0];

        // If the first element is not a member element, then we assume it
        //is an expression and replace it with the correct member element
        if (firstElement is not MemberElement)
        {
            ExpressionMemberElement actualFirst = new(firstElement);
            elements[0] = actualFirst;
        }
        else
        {
            ResolveNamespaces(elements, context);
        }
    }

    private void ResolveNamespaces(IList elements, ExpressionContext context)
    {
        ImportBase currentImport = context.Imports.RootImport;

        while (true)
        {
            var name = GetName(elements);
            if (name is null)
            {
                break; // TODO: might not be correct. Was : Exit While
            }

            var import = currentImport.FindImport(name);
            if (import is null)
            {
                break; // TODO: might not be correct. Was : Exit While
            }

            currentImport = import;
            elements.RemoveAt(0);

            if (elements.Count > 0)
            {
                var newFirst = (MemberElement)elements[0];
                newFirst.SetImport(currentImport);
            }
        }

        if (elements.Count == 0)
        {
            throw CreateCompileException(
                CompileErrors.NamespaceCannotBeUsedAsType,
                CompileExceptionReason.TypeMismatch,
                currentImport.Name
            );
        }
    }

    private static string? GetName(IList elements)
    {
        if (elements.Count == 0)
        {
            return null;
        }
        var fpe = elements[0] as IdentifierElement;
        return fpe?.MemberName;
    }

    private static void Resolve(IEnumerable elements, ExpressionContext context)
    {
        foreach (MemberElement element in elements)
        {
            element.Resolve(context);
        }
    }

    /// <summary>
    /// Todo: Add description
    /// </summary>
    /// <param name="ilGenerator"></param>
    /// <param name="context"></param>
    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context) =>
        tail.Emit(ilGenerator, context);

    public override Type ResultType => tail.ResultType;
}

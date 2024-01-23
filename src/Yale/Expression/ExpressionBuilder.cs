using System.IO;
using Yale.Core;
using Yale.Engine;
using Yale.Expression.Elements;
using Yale.Expression.Elements.Base;
using Yale.Parser;
using Yale.Parser.Internal;

namespace Yale.Expression;

internal sealed class ExpressionBuilder
{
    public ExpressionBuilderOptions Options { get; private set; }
    internal ComputeInstance ComputeInstance { get; set; }

    private ExpressionParser Parser { get; set; }
    private YaleExpressionAnalyzer Analyzer { get; set; }

    private const string DynamicMethodName = "DynamicMethod";

    internal VariableCollection Variables { get; } = new VariableCollection();

    public ImportCollection Imports { get; }

    public ExpressionBuilder(ComputeInstance instance)
    {
        Options = new ExpressionBuilderOptions();
        ComputeInstance = instance;
        Imports = new ImportCollection(Options);
        Analyzer = new YaleExpressionAnalyzer();
        Parser = new ExpressionParser(TextReader.Null, Analyzer);
    }

    public ExpressionBuilder(ExpressionBuilderOptions options, ComputeInstance instance)
    {
        Options = options;
        ComputeInstance = instance;
        Imports = new ImportCollection(Options);
        Analyzer = new YaleExpressionAnalyzer();
        Parser = new ExpressionParser(TextReader.Null, Analyzer);
    }

    internal Expression<T> BuildExpression<T>(string expressionName, string expression)
    {
        object owner = DefaultExpressionOwner.Instance;
        var ownerType = DefaultExpressionOwner.Type;

        Imports.ImportOwner(ownerType);

        ExpressionContext context =
            new(
                builderOptions: Options,
                expressionName: expressionName,
                owner: owner,
                imports: Imports,
                variables: Variables,
                computeInstance: ComputeInstance
            );

        var topElement = Parse(expression, context);

        RootExpressionElement rootElement = new(topElement, typeof(T));
        DynamicMethod dynamicMethod = CreateDynamicMethod<T>(ownerType);

        YaleIlGenerator ilGenerator = new(dynamicMethod.GetILGenerator());
        rootElement.Emit(ilGenerator, context);

#if DEBUG
        ilGenerator.ValidateLength();
#endif

        var delegateType = typeof(ExpressionEvaluator<>).MakeGenericType(typeof(T));
        var evaluator = (ExpressionEvaluator<T>)dynamicMethod.CreateDelegate(delegateType);

        return new Expression<T>(expression, evaluator, context);
    }

    private BaseExpressionElement Parse(string expression, ExpressionContext context)
    {
        StringReader stringReader = new(expression);

        Parser.Reset(stringReader);

        Analyzer.SetContext(context);
        var rootNode = Parse();
        Analyzer.Reset();

        BaseExpressionElement topElement = (BaseExpressionElement)rootNode.Values[0];
        return topElement;
    }

    private static DynamicMethod CreateDynamicMethod<T>(Type ownerType)
    {
        Type[] parameterTypes =
        {
            typeof(object),
            typeof(ExpressionContext),
            typeof(VariableCollection)
        };
        return new DynamicMethod(DynamicMethodName, typeof(T), parameterTypes, ownerType);
    }

    private Node Parse()
    {
        try
        {
            return Parser.Parse();
        }
        catch (ParserLogException ex)
        {
            throw new ExpressionCompileException(ex);
        }
    }
}

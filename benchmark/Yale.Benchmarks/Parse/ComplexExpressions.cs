using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using Yale.Engine;

namespace Yale.Benchmarks.Parse;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ComplexExpressions
{
    private const string expressionOne = "true <> false AND (1 + 2 > 3) AND a < 10";
    private const string expressionTwo =
        "true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a AND true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a";
    private const string expressionTwo_Flee =
        "true <> false AND (1 + 2 > 3) OR If(expr_a, NOT expr_a, false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a, NOT expr_a, false) AND NOT false AND true OR false AND expr_a AND true <> false AND (1 + 2 > 3) OR If(expr_a, NOT expr_a, false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a, NOT expr_a, false) AND NOT false AND true OR false AND expr_a";

    [GlobalSetup]
    public void Setup() { }

    [Benchmark(Baseline = true)]
    public void AddExpression_Recalculate_Never()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    Recalculate = ComputeInstanceOptions.RecalculateMode.Never
                }
            );
        Parse(instance);
    }

    [Benchmark]
    public void AddExpression_Recalculate_Auto()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    Recalculate = ComputeInstanceOptions.RecalculateMode.Auto
                }
            );
        Parse(instance);
    }

    [Benchmark]
    public void AddExpression_Recalculate_Lazy()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    Recalculate = ComputeInstanceOptions.RecalculateMode.Lazy
                }
            );
        Parse(instance);
    }

    [Benchmark]
    public void Flee_Recalculate_Off()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();

        ParseFlee(engine, context);
    }

    [Benchmark]
    public void Flee_Recalculate_Manual()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();

        ParseFlee(engine, context);
    }

    private static void ParseFlee(CalculationEngine engine, ExpressionContext context)
    {
        context.Variables["a"] = 0;
        engine.Add("expr_a", expressionOne, context);
        engine.Add("expr_b", expressionTwo_Flee, context);

        _ = engine.GetResult<bool>("expr_b");
    }

    private static void Parse(ComputeInstance instance)
    {
        instance.Variables["a"] = 0;
        instance.AddExpression("expr_a", expressionOne);
        instance.AddExpression("expr_b", expressionTwo);
        _ = instance.GetResult<bool>("expr_b");
    }
}

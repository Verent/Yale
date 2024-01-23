using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using Yale.Engine;

namespace Yale.Benchmarks.Engine;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class Recalculate
{
    private const string expr_a = "if(true <> false AND ((1 + 2 > 3) OR a < 10.0); 1; 0)";
    private const string expr_a_flee = "if(true <> false AND ((1 + 2 > 3) OR a < 10.0), 1, 0)";
    private const string expr_b = "expr_a + 1";

    [GlobalSetup]
    public void Setup() { }

    [Benchmark(Baseline = true)]
    public void AddExpression_Recalculate_Off()
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

        _ = engine.GetResult<int>("expr_b");
    }

    [Benchmark]
    public void Flee_Recalculate_Manual()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();

        ParseFlee(engine, context);

        engine.Recalculate("expr_b");

        _ = engine.GetResult<int>("expr_b");
    }

    private static void ParseFlee(CalculationEngine engine, ExpressionContext context)
    {
        context.Variables["a"] = 0;
        engine.Add("expr_a", expr_a_flee, context);
        engine.Add("expr_b", expr_b, context);
        context.Variables["a"] = 2;
        context.Variables["a"] = 5;
        context.Variables["a"] = 10;
    }

    private static void Parse(ComputeInstance instance)
    {
        instance.Variables["a"] = 0;
        instance.AddExpression("expr_a", expr_a);
        instance.AddExpression("expr_b", expr_b);
        instance.Variables["a"] = 2;
        instance.Variables["a"] = 5;
        instance.Variables["a"] = 10;

        _ = instance.GetResult<int>("expr_b");
    }
}

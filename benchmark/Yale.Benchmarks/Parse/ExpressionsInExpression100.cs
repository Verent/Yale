using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using Yale.Engine;

namespace Yale.Benchmarks.Parse;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ExpressionsInExpression100
{
    private const int Runs = 100;

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
        var key = Parse(instance);

        _ = instance.GetResult<int>(key);
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
        var key = Parse(instance);

        _ = instance.GetResult<int>(key);
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
        var key = Parse(instance);

        _ = instance.GetResult<int>(key);
    }

    [Benchmark]
    public void Flee_Recalculate_Off()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();
        var key = ParseFlee(engine, context);
        _ = engine.GetResult<int>(key);
    }

    [Benchmark]
    public void Flee_Recalculate_Manual()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();
        var key = ParseFlee(engine, context);

        engine.Recalculate(key);
        _ = engine.GetResult<int>(key);
    }

    private static string ParseFlee(CalculationEngine engine, ExpressionContext context)
    {
        var variables = context.Variables;
        variables.Add("x", 1);

        var key = $"key0";
        engine.Add(key, $"1", context);
        for (var i = 1; i < Runs; i++)
        {
            var newKey = $"key{i}";
            var op = i % 2 == 0 ? "+" : "-";

            engine.Add(newKey, $"{i} {op} {key} + x", context);
            key = newKey;
        }
        return key;
    }

    private static string Parse(ComputeInstance instance)
    {
        instance.Variables.Add("x", 1);

        var key = $"key0";
        instance.AddExpression<int>(key, $"1");
        for (var i = 1; i < Runs; i++)
        {
            if (i % 20 == 0)
            {
                instance.Variables["x"] = i / 20;
            }
            var newKey = $"key{i}";
            var op = i % 2 == 0 ? "+" : "-";

            instance.AddExpression<int>(newKey, $"{i} {op} {key} + x");
            key = newKey;
        }
        return key;
    }
}

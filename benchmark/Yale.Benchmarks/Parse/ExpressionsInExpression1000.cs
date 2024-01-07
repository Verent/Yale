using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using Yale.Engine;

namespace Yale.Benchmarks.Parse;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ExpressionsInExpression1000
{
    private const int Runs = 1_000;

    [GlobalSetup]
    public void Setup() { }

    [Benchmark(Baseline = true)]
    public void AddExpression_Recalculate_Off()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions { Recalculate = false, LazyRecalculate = false }
            );
        var key = Parse(instance);

        var result = instance.GetResult<int>(key);
    }

    [Benchmark]
    public void AddExpression_Recalculate_Auto()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions { Recalculate = true, LazyRecalculate = false, }
            );
        var key = Parse(instance);

        var result = instance.GetResult<int>(key);
    }

    [Benchmark]
    public void AddExpression_Recalculate_Lazy()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions { Recalculate = true, LazyRecalculate = true, }
            );
        var key = Parse(instance);

        var result = instance.GetResult<int>(key);
    }

    [Benchmark]
    public void Flee_Recalculate_Off()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();
        var key = ParseFlee(engine, context);
        var result = engine.GetResult<int>(key);
    }

    [Benchmark]
    public void Flee_Recalculate_Manual()
    {
        CalculationEngine engine = new();
        ExpressionContext context = new();
        var key = ParseFlee(engine, context);

        engine.Recalculate(key);
        var result = engine.GetResult<int>(key);
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
            var newKey = $"key{i}";
            var op = i % 2 == 0 ? "+" : "-";

            instance.AddExpression<int>(newKey, $"{i} {op} {key} + x");
            key = newKey;
        }
        return key;
    }
}

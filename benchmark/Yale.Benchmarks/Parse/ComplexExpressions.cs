using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Yale.Engine;

namespace Yale.Benchmarks.Parse;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ComplexExpressions
{
    private const string expressionOne = "true <> false AND (1 + 2 > 3) AND a < 10";
    private const string expressionTwo = "true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a AND true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a OR true <> false AND (1 + 2 > 3) OR If(expr_a; NOT expr_a; false) AND NOT false AND true OR false AND expr_a";

    [GlobalSetup]
    public void Setup() { }

    [Benchmark(Baseline = true)]
    public void AddExpression_No_Recalculate()
    {
        ComputeInstance instance =
            new(options: new ComputeInstanceOptions { Recalculate = false, });
        Parse(instance);
    }

    [Benchmark]
    public void AddExpression_AutoRecalculate()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    Recalculate = true,
                    LazyRecalculate = false,
                }
            );
        Parse(instance);
    }

    [Benchmark]
    public void AddExpression_AutoRecalculate_Lazy()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    Recalculate = true,
                    LazyRecalculate = true,
                }
            );
        Parse(instance);
    }

    private static void Parse(ComputeInstance instance)
    {
        instance.Variables["a"] = 0;
        instance.AddExpression("expr_a", expressionOne);
        instance.AddExpression("expr_b", expressionTwo);
        var result = instance.GetResult<bool>("expr_b");
    }
}

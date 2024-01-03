using System.Reflection.Metadata;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;
using Yale.Engine;

namespace Yale.Benchmarks.Parse;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ParseExpression
{
    [GlobalSetup]
    public void Setup() { }

    [Benchmark(Baseline = true)]
    public void AddExpression_No_Recalculate()
    {
        ComputeInstance instance =
            new(options: new ComputeInstanceOptions { AutoRecalculate = false, });
        Parse(instance);
    }

    [Benchmark]
    public void AddExpression_AutoRecalculate()
    {
        ComputeInstance instance =
            new(
                options: new ComputeInstanceOptions
                {
                    AutoRecalculate = true,
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
                    AutoRecalculate = true,
                    LazyRecalculate = true,
                }
            );
        Parse(instance);
    }

    private static void Parse(ComputeInstance instance)
    {
        instance.Variables["a"] = 0;
        instance.AddExpression("exp_a", "a + 2");
        instance.AddExpression("exp_b", "exp_a + exp_a + a");
        for (var i = 1; i < 1000; i++)
        {
            instance.Variables["a"] = i;
        }
        var result = instance.GetResult<int>("exp_b");
    }
}

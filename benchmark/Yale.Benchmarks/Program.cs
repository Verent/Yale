global using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;

ConsoleLogger.Default.WriteLine($"Started");

var config = new ManualConfig
{
    ArtifactsPath = "BenchmarkArtifacts",
    UnionRule = ConfigUnionRule.AlwaysUseLocal,
    Options =
        ConfigOptions.Default | ConfigOptions.KeepBenchmarkFiles | ConfigOptions.DisableLogFile
};
config.KeepBenchmarkFiles(value: false);
config.AddLogger(ConsoleLogger.Default);
config.AddExporter(AsciiDocExporter.Default);
config.AddAnalyser(EnvironmentAnalyser.Default);
config.AddColumn(TargetMethodColumn.Method);
config.AddColumn(JobCharacteristicColumn.AllColumns);
config.AddColumn(StatisticColumn.AllStatistics);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll(args: args, config: config);

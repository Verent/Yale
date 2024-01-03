global using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

ConsoleLogger.Default.WriteLine($"Started");

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

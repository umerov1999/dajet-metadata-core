using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace DaJet.Metadata.Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            Summary summary = BenchmarkRunner.Run<MetadataServiceBenchmark>();

            _ = Console.ReadKey(false);
        }
    }
}
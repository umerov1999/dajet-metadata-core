using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;

namespace DaJet.Metadata.Benchmark
{
    [MemoryDiagnoser] [Config(typeof(Config))]
    public class MetadataServiceBenchmark
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        
        private class Config : ManualConfig
        {
            public Config()
            {
                //AddJob(Job.Dry.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
                AddJob(Job.Dry.WithGcServer(true).WithGcForce(false).WithId("Server"));
                //AddJob(Job.Dry.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
                //AddJob(Job.Dry.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
            }
        }

        [Params(1, 5)] public int Iterations;

        [Benchmark(Description = "MS TryOpenInfoBase")]
        public void Benchmark_MS_TryOpenInfoBase()
        {
            for (int i = 0; i < Iterations; i++)
            {
                MS_TryOpenInfoBase(out InfoBase infoBase);
            }
        }
        private void MS_TryOpenInfoBase(out InfoBase infoBase)
        {
            MetadataService metadataService = new MetadataService();

            metadataService.OpenInfoBase(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, out infoBase);
        }

        [Benchmark(Description = "PG TryOpenInfoBase")]
        public void Benchmark_PG_TryOpenInfoBase()
        {
            for (int i = 0; i < Iterations; i++)
            {
                PG_TryOpenInfoBase(out InfoBase infoBase);
            }
        }
        private void PG_TryOpenInfoBase(out InfoBase infoBase)
        {
            MetadataService metadataService = new MetadataService();

            metadataService.OpenInfoBase(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, out infoBase);
        }
    }
}
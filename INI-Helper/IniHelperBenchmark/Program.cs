using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using IniHelper;
using IniHelperTest;

namespace IniHelperBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CommonBenchmark>();
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class CommonBenchmark
    {
        private static readonly int Weight = 200;

        private string sample;

        [GlobalSetup]
        public void GlobalSetup()
        {
            sample = IniSample.Text;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [Benchmark(Description = "IniParser.Parse()")]
        public Action Method1()
        {
            Action _ = null;

            for (var i = 0; i < Weight; i++)
            {
                var a = IniParser.Parse(sample);
            }

            return _;
        }
    }

    // 参考: http://engineering.grani.jp/entry/2017/07/28/145035
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddExporter(MarkdownExporter.GitHub);
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(Job.ShortRun);
        }
    }
}

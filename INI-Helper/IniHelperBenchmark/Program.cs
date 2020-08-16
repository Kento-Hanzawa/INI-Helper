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

		[Benchmark(Description = "IniTextParser.Parse()")]
		public Action Method1()
		{
			Action _ = null;

			for (var i = 0; i < Weight; i++)
			{
				using (var reader = new IniTextParser(sample))
				{
					var a = reader.Parse();
				}
			}

			return _;
		}
	}

	// 参考: http://engineering.grani.jp/entry/2017/07/28/145035
	public class BenchmarkConfig : ManualConfig
	{
		public BenchmarkConfig()
		{
			// ベンチマーク結果を書く時に出力させとくとベンリ
			AddExporter(MarkdownExporter.GitHub);
			AddDiagnoser(MemoryDiagnoser.Default);

			// ShortRunを使うとサクッと終わらせられる、デフォルトだと本気で長いので短めにしとく。
			// ShortRunは LaunchCount=1  TargetCount=3 WarmupCount = 3 のショートカット
			AddJob(Job.ShortRun);

			// ダルいのでShortRunどころか1回, 1回でやる
			//Add( Job.ShortRun.With( BenchmarkDotNet.Environments.Platform.X64 ).WithWarmupCount( 1 ).WithTargetCount( 1 ) );
		}
	}
}

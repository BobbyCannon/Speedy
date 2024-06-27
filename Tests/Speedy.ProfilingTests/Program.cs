#region References

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Speedy.Storage;

#endregion

namespace Speedy.ProfilingTests;

public class Program
{
	#region Methods

	public static void Main(string[] args)
	{
		BenchmarkRunner.Run<Benchmarks>(new DebugBuildConfig());
	}

	#endregion

	#region Classes

	[MemoryDiagnoser]
	[SimpleJob(iterationCount: 30)]
	public class Benchmarks
	{
		#region Constructors

		static Benchmarks()
		{
			Cache = new MemoryCache();
		}

		#endregion

		#region Properties

		public static MemoryCache Cache { get; }

		#endregion

		#region Methods

		[Benchmark]
		public void CacheTest()
		{
			if (Cache.HasKey("aoeu"))
			{
				Cache.Remove("aoeu");
				return;
			}

			Cache.Set("aoeu", null, TimeSpan.FromTicks(1));
		}

		#endregion
	}

	#endregion
}
#region References

using System;
using Speedy.Benchmark.Benchmarks;

#endregion

namespace Speedy.Benchmark
{
	public static class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			var benchmark = new SyncEngineBenchmark();
			//SyncEntityBenchmark.Run();
			//SyncClientBenchmark.Run();
			
			benchmark.Run();

			Console.WriteLine("press any key");
			Console.ReadKey(true);
		}

		#endregion
	}
}